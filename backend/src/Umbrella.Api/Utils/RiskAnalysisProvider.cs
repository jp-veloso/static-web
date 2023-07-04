using System.Data;
using Microsoft.EntityFrameworkCore;
using RiskAnalysis;
using Umbrella.Api.Contexts;

namespace Umbrella.Api.Utils;

public class RiskAnalysisProvider : IRiskAnalysisDataProvider
{
    public float[][] FindRates()
    {
        using var db = new RepositoryContext();
        using var conn = db.Database.GetDbConnection();
        using var command = conn.CreateCommand();
        
        conn.Open();
        command.CommandText = @"
				SELECT rate, 
					COALESCE(MAX(CASE WHEN insurerFK = 1000 THEN rate END),-1),
					COALESCE(MAX(CASE WHEN insurerFK = 1001 THEN rate END),-1),
					COALESCE(MAX(CASE WHEN insurerFK = 1002 THEN rate END),-1),
					COALESCE(MAX(CASE WHEN insurerFK = 1003 THEN rate END),-1),
					COALESCE(MAX(CASE WHEN insurerFK = 1004 THEN rate END),-1),
					COALESCE(MAX(CASE WHEN insurerFK = 1005 THEN rate END),-1),
					COALESCE(MAX(CASE WHEN insurerFK = 1006 THEN rate END),-1),
					COALESCE(MAX(CASE WHEN insurerFK = 1007 THEN rate END),-1),
					COALESCE(MAX(CASE WHEN insurerFK = 1008 THEN rate END),-1),
					COALESCE(MAX(CASE WHEN insurerFK = 1009 THEN rate END),-1),
					COALESCE(MAX(CASE WHEN insurerFK = 1010 THEN rate END),-1),
					COALESCE(MAX(CASE WHEN insurerFK = 1011 THEN rate END),-1)
				FROM portal.Taker t WHERE category = 'TRADICIONAL' AND (rate BETWEEN 0.25 AND 2.0) GROUP BY rate ORDER BY rate ASC";
        
        using var dbReader = command.ExecuteReader();

        if (dbReader.HasRows)
        {
            ICollection<float[]> values = new List<float[]>();

            while (dbReader.Read())
            {
                float[] array = new float[dbReader.FieldCount];

                for (var i = 0; i < dbReader.FieldCount; i++)
                {
                    array[i] = dbReader.GetFloat(i);
                }
		        
                values.Add(array);
            }
	        
            conn.Close();
            return values.ToArray();
        }
        
        conn.Close();
        throw new DataException("DbDataReader " + nameof(dbReader) + " is empty");
    }

    public string[][] FindSamples()
    {
	    using var db = new RepositoryContext();
	    using var conn = db.Database.GetDbConnection();
	    using var command = conn.CreateCommand();

	    conn.Open();

	    command.CommandText = @"
				SELECT COALESCE(rating,''), rate, insurerPK FROM portal.Enrollment INNER JOIN portal.Taker ON insurerFK = insurerPK AND clientFK = clientPK
					WHERE category = 'TRADICIONAL' AND rate != 0";
	    
	    using var dbReader = command.ExecuteReader();

	    if (dbReader.HasRows)
	    {
		    ICollection<string[]> values = new List<string[]>();
		    
		    while (dbReader.Read())
		    {
			    string[] array = new string[dbReader.FieldCount];

			    for (var i = 0; i < dbReader.FieldCount; i++)
			    {
				    array[i] = dbReader.GetValue(i).ToString()!;
			    }
		        
			    values.Add(array);
		    }
	        
		    conn.Close();
		    return values.ToArray();
	    }
	    
	    conn.Close();
	    throw new DataException("DbDataReader " + nameof(dbReader) + " is empty");
    }
}