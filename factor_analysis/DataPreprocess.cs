using System;
using System.Collections.Generic;
using System.IO;

namespace FactorAnalysis
{
    class DataPreprocess
    {
        //read csv file
        public static List<string[]> ReadCsvFile(string path)
        {
            List<string[]> lineArrays = new List<string[]>();
            string line = "";
            using (StreamReader sr = new StreamReader(path))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    string[] sa = line.Split(',');
                    lineArrays.Add(sa);
                }
            }

            return lineArrays;
        }

        //convert the string to dictionary of dictionary
        public static Dictionary<string, Dictionary<DateTime, double>> ConvertDict(List<string[]> dataString, string dupValue = "last")
        {
            Dictionary<string, Dictionary<DateTime, double>> dataDict = new Dictionary<string, Dictionary<DateTime, double>>();

            for (int i = 1; i < dataString.Count; i++)
            {
                string[] dataPair = dataString[i];
                string ticker = dataPair[0];
                DateTime date = System.Convert.ToDateTime(dataPair[1]);
                double value;
                if (dataPair[2] == "NULL")
                {
                    value = Double.NaN;
                }
                else
                {
                    value = System.Convert.ToDouble(dataPair[2]);
                }

                if (!dataDict.ContainsKey(ticker))
                {
                    dataDict.Add(ticker, new Dictionary<DateTime, double>() { { date, value } });
                }
                else if (!dataDict[ticker].ContainsKey(date))
                {
                    dataDict[ticker].Add(date, value);
                }
                else
                {
                    if (dupValue == "first") { }
                    else if (dupValue == "last")
                    {
                        dataDict[ticker][date] = value;
                    }
                    else if (dupValue == "sum")
                    {
                        dataDict[ticker][date] += value;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            return dataDict;
        }

        public static SortedSet<DateTime> GetDates(Dictionary<string, Dictionary<DateTime, double>> priceDict)
        {
            SortedSet<DateTime> dates = new SortedSet<DateTime>();
            foreach (var prices in priceDict.Values)
            {
                foreach (var date in prices.Keys)
                {
                    dates.Add(date);
                }
            }
            return dates;
        }

    }
}
