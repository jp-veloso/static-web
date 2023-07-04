using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace RiskAnalysis;

public class RiskAnalysisDataService
{
    private readonly MLContext _context = new();

    private PredictionEngine<RatePoint, RatePointPrediction>? _engine;

    private readonly IRiskAnalysisDataProvider _provider;

    private readonly ConcurrentDictionary<int, float[,]> _rankingIntervals = new();
    
    private readonly Dictionary<char, string[]> _values = new()
    {
        {'A', new[] {"AA", "A", "A1", "1000 a 750"}},
        {'B', new[] {"B1", "BB", "749 a 550"}},
        {'C', new[] {"B", "B2", "B3", "549 a 450"}},
        {'D', new[] {"CC", "C", "C2", "C1", "449 a 250"}},
        {'E', new[] {"D", "D1", "250 a 0"}}
    };

    public RiskAnalysisDataService(IRiskAnalysisDataProvider provider)
    {
        _provider = provider;
        
        Initialize();
    }

    public ConcurrentDictionary<int, float[,]> GetRankingInterval() => _rankingIntervals;

    public Dictionary<char, string[]> GetRatingDictionary() => _values;

    private static void PrintMetrics(RegressionMetrics metrics)
    {
        Console.WriteLine("Mean Absolute Error: " + metrics.MeanAbsoluteError);
        Console.WriteLine("Mean Squared Error: " + metrics.MeanSquaredError);
        Console.WriteLine("Root Mean Squared Error: " + metrics.RootMeanSquaredError);
        Console.WriteLine("RSquared: " + metrics.RSquared);
    }
    
    public static void PrettyPrintFloatMatrix(float[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            Console.Write(Convert.ToChar(65 + i) + ": ");
            for (int j = 0; j < cols; j++)
            {
                Console.Write(matrix[i, j].ToString("F2") + "\t");
            }

            Console.WriteLine();
        }
    }
    
    private string ConvertRate(int insurerId, float rate)
    {
        float[,] values = _rankingIntervals[insurerId];

        int possibleValue = 65;
        float minDiff = float.MaxValue;

        for (int i = 0; i < values.GetLength(0); i++)
        {
            for (int j = 0; j < values.GetLength(1); j++)
            {
                float diff = Math.Abs(rate - values[i, j]); 
                
                if (diff < minDiff)
                {
                    minDiff = diff;
                    possibleValue = i + 65;
                }
                
            }
        }
        
        return Convert.ToChar(possibleValue).ToString();
    }

    public void Ranking()
    {
        float[][] rates = _provider.FindRates();

        for (int i = 1000; i <= 1011; i++)
        {
            var result = Ranking(rates.Select(array => array[i - 1000 + 1]).ToArray());
            _rankingIntervals.GetOrAdd(i, result);
        }

    }

    public void AddCustomRanking(int id, float[,] array) => _rankingIntervals[id] = array;
    
    private PredictionEngine<RatePoint, RatePointPrediction> CreatePredictEngine()
    {
        string[][] samples = _provider.FindSamples();
        
        RatePoint[] dataPoints = new RatePoint[samples.Length];

        int aux = 0;
        
        foreach (var strings in samples)
        {
            string rating = strings[0].Replace(" ", "").Replace("SERASA:", "");

            if (string.IsNullOrEmpty(rating) || float.TryParse(rating, out _))
            {
                rating = ConvertRate(int.Parse(strings[2]), float.Parse(strings[1]));
            }
            else
            {
                foreach (var (key, value) in _values)
                {
                    if (!value.Contains(rating)) continue;
                    
                    rating = key.ToString();
                    break;
                }
            }

            dataPoints[aux] = new RatePoint { Rating = rating, Cia = strings[2], Rate = float.Parse(strings[1]) };
            aux++;
        }

        IDataView dataView = _context.Data.LoadFromEnumerable(dataPoints);
        dataView = _context.Data.ShuffleRows(dataView);

        var dataSplit = _context.Data.TrainTestSplit(dataView, 0.2);

        var pipeline = _context.Transforms.Categorical.OneHotEncoding(new InputOutputColumnPair[]
            {
                new("CiaCategory", "Cia"),
                new("RatingCategory", "Rating")
            })
            .Append(_context.Transforms.Concatenate("Features", "RatingCategory", "CiaCategory"))
            .Append(_context.Regression.Trainers.FastTree());


        var model = pipeline.Fit(dataSplit.TrainSet);
        
        //PrintMetrics(_context.Regression.Evaluate(model.Transform(dataSplit.TestSet)));
        
        return _context.Model.CreatePredictionEngine<RatePoint,RatePointPrediction>(model);
    }

    private float[,] Ranking(float[] rates)
    {
        rates = rates.Where(x => x != -1).ToArray();
        
        RankingPoint[] dataPoints = new RankingPoint[rates.Length];
        
        for (int i = 0; i < rates.Length; i++)
        {
            dataPoints[i] = new RankingPoint{ Features = new[]{ rates[i] } };
        }

        var dataView = _context.Data.LoadFromEnumerable(dataPoints);
        var pipeline = _context.Clustering.Trainers.KMeans("Features", numberOfClusters: 5);
        var predictions = pipeline.Fit(dataView).Transform(dataView);
        
        var clusterColumn = predictions.GetColumn<uint>("PredictedLabel").ToArray();

        float begin = rates[0];
        int j = 0;

        float[,] results = new float[5,2];
        
        for (int i = 1; i < rates.Length; i++)
        {
            if (clusterColumn[i] != clusterColumn[i - 1])
            {
                results[j, 0] = begin;
                results[j, 1] = rates[i - 1];
                j++;
                begin = rates[i];
            }
        }

        results[j, 0] = begin;
        results[j, 1] = rates[^1];

        return results;
    }

    public void Initialize()
    {
        // Ranking Geral
        Ranking();

        // JNS Table
        AddCustomRanking(1002, new[,]
        {
            {0.25f, 0.30f},
            {0.32f, 0.55f},
            {0.60f, 0.60f},
            {01.0f, 01.0f},
            {1.25f, 02.0f}
        });
        
        // Sombrero Table
        AddCustomRanking(1011, new[,]
        {
            {0.30f, 0.43f},
            {0.65f, 0.70f},
            {0.95f, 1.13f},
            {1.96f, 1.96f},
            {3.39f, 3.50f}
        });
        
        // Excelsior Table
        AddCustomRanking(1009, new[,]
        {
            {0.30f, 0.43f},
            {0.52f, 0.75f},
            {1.07f, 1.29f},
            {2.00f, 2.29f},
            {3.85f, 3.85f}
        });
        
        // Essor Table
        AddCustomRanking(1008, new[,]
        {
            {0.30f, 0.43f},
            {0.52f, 0.75f},
            {1.07f, 1.29f},
            {2.00f, 2.29f},
            {3.85f, 3.85f}
        });
        
        // BMG Table
        AddCustomRanking(1004, new[,]
        {
            {0.35f, 0.40f},
            {0.45f, 0.45f},
            {0.50f, 0.60f},
            {0.75f, 0.90f},
            {01.0f, 01.0f}
        });
        
        _engine = CreatePredictEngine();
    }

    public string ConvertToDicRating(string companyRate)
    {
        string rate = "default";
        
        foreach (var (key, values) in _values)
        {

            if (int.TryParse(companyRate, out var score))
            {
                string value = values[^1];
                value = value.Replace("a", ";").Replace(" ", "");
                
                int[] minMax = value.Split(";").Select(int.Parse).ToArray();

                if (score <= minMax[0] && score >= minMax[1])
                {
                    rate = key.ToString();
                    break;
                }
            }
            else
            {
                if (!values.Contains(companyRate)) continue;
                
                rate = key.ToString();
                break;
            }
        }

        return rate;
    }

    public float Predict(int insurerId, string rating, bool useCompanyRating)
    {
        if (_engine == null)
        {
            throw new NullReferenceException("Engine is null");
        }

        if (useCompanyRating)
        {
            rating = ConvertToDicRating(rating);
        }

        if (insurerId is 1005 or 1010)
        {
            int toIncrease = Convert.ToInt32(rating[0]) + 1;
            rating = Convert.ToChar(toIncrease > 69 ? 69 : toIncrease).ToString();
        }

        RatePointPrediction result = _engine.Predict(new RatePoint { Cia = $"{insurerId}", Rating = rating });

        return result.PredictedRate;
    }


    public record RatePoint { public string? Rating { get; init; } [ColumnName("Label")] public float Rate { get; init; } public string? Cia { get; init; } } 
    public record RatePointPrediction { [ColumnName("Score")] public float PredictedRate { get; init; } }
    public record RankingPoint { [VectorType(1)] public float[] Features { get; init; } = default!; }
    
}