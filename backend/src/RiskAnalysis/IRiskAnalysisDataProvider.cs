namespace RiskAnalysis;

public interface IRiskAnalysisDataProvider
{
    public float[][] FindRates();
    public string[][] FindSamples();
}