using Microsoft.EntityFrameworkCore;
using RiskAnalysis;
using Umbrella.Api.Contexts;
using Umbrella.Api.Entities;
using Umbrella.Api.Entities.Enums;
using Umbrella.Api.Resources.Exceptions;
using Umbrella.Api.Services.Exceptions;

namespace Umbrella.Api.Services;

public class RiskAnalysisService
{
    private readonly RiskAnalysisDataService _dataService;

    public RiskAnalysisService(RiskAnalysisDataService dataService)
    {
        _dataService = dataService;
    }

    public object GetAnalysisTable()
    {
        var data = _dataService.GetRankingInterval();

        using var db = new RepositoryContext();
        
        List<Insurer> insurers = db.Insurers.Where(x => x.Id <= 1011).ToList();

        var mergedArray = data.Zip(insurers, (kv, insurer) => new
        {
            id = insurer.Id,
            valueId = kv.Key,
            name = insurer.Name,
            values = ToJaggedArray(kv.Value)
        }).ToList();

        var dic = _dataService.GetRatingDictionary();

        return new { mergedArray, dic };
    }

    private float[][] ToJaggedArray(float[,] values)
    {
        var myJaggedArray = new float [values.GetLength(0)][];
        
        for(int i = 0 ; i < values.GetLength(0) ; i ++)
        {
            myJaggedArray[i] = new float[values.GetLength(1)];

            for(int j = 0 ; j < values.GetLength(1) ; j ++)
                myJaggedArray[i][j] = values[i,j]; 
        }

        return myJaggedArray;
    }

    public object GetRatingFromClient(string cnpj)
    {
        using var db = new RepositoryContext();

        Client? client = db.Clients.Include(x => x.Enrollments.Where(y => y.Status == Status.CREATED))
            .ThenInclude(x => x.Insurer).SingleOrDefault(x => x.Cnpj == cnpj);

        if (client == null)
        {
            throw new ServiceException("Client not found", new StandardError()
            {
                Error = "Cnpj not found",
                Message = $"Cnpj with value: {cnpj} not found",
                Timestamp = DateTime.UtcNow,
                Status = 404
            });
        }

        return client.Enrollments.Where(x => !string.IsNullOrEmpty(x.Rating)).Select(x => new
        {
            rating = x.Rating,
            insurerId = x.Insurer.Id,
            insurerName = x.Insurer.Name
        }).ToList();
    }

    public object Predict(string rating, bool useCompanyRating)
    {
        List<object> results = new List<object>();

        for (int i = 1000; i <= 1011; i++)
        {
            var predicted = _dataService.Predict(i, rating, useCompanyRating);
            
            results.Add(new { predicted, insurerId = i });
        }

        return results;
    }
}