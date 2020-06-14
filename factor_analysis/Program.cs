using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FactorAnalysis
{
    class Program
    {
        private static void Main()
        {
            //read data
            List<string[]> priceString = DataPreprocess.ReadCsvFile(@"/Users/tushilan/Desktop/factoranalysis/MarketDataPrice.csv");
            List<string[]> dividendString = DataPreprocess.ReadCsvFile(@"/Users/tushilan/Desktop/factoranalysis/MarketDataDividend.csv");

            //convert string data to dictionary
            Dictionary<string, Dictionary<DateTime, double>> priceDict = DataPreprocess.ConvertDict(priceString);
            Dictionary<string, Dictionary<DateTime, double>> dividendDict = DataPreprocess.ConvertDict(dividendString, "sum");

            //
            SortedSet<DateTime> dates = new SortedSet<DateTime>();
            foreach (var prices in priceDict.Values)
            {
                foreach (var date in prices.Keys)
                {
                    dates.Add(date);
                }
            }

            Dictionary<string, Dictionary<DateTime, double>> returnDict = new Dictionary<string, Dictionary<DateTime, double>>();
            foreach (var item in priceDict)
            {
                string ticker = item.Key;
                Dictionary<DateTime, double> prices = item.Value;
                Dictionary<DateTime, double> dividends;
                if (!dividendDict.TryGetValue(ticker, out dividends))
                {
                    dividends = new Dictionary<DateTime, double>();
                }

                Dictionary<DateTime, double> returns = Calculation.CalculateReturns(dates, prices, dividends);
                returnDict.Add(ticker, returns);
            }

            Dictionary<string, double> betas = new Dictionary<string, double>();
            foreach (var item in returnDict)
            {
                string ticker = item.Key;
                Dictionary<DateTime, double> returnsX = returnDict["SPY"];
                Dictionary<DateTime, double> returnsY = item.Value;

                double beta = Calculation.CalculateBeta(dates, returnsX, returnsY);
                betas.Add(ticker, beta);
            }


            Dictionary<string, Dictionary<DateTime, double>> factorDict = new Dictionary<string, Dictionary<DateTime, double>>();
            foreach (var item in returnDict)
            {
                string ticker = item.Key;
                Dictionary<DateTime, double> returnsX = returnDict["SPY"];
                Dictionary<DateTime, double> returnsY = item.Value;
                double beta = betas[ticker];
                Dictionary<DateTime, double> factors;
                if (ticker != "SPY")
                {
                    factors = Calculation.CalculateFactors(dates, returnsX, returnsY, beta);
                }
                else
                {
                    factors = returnDict["SPY"];
                }
                factorDict.Add(ticker, factors);
            }

            System.Console.WriteLine("Success!");
        }
    }

    class DataPreprocess
    {
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
    }

    class Calculation
    {
        public static Dictionary<DateTime, double> CalculateReturns(SortedSet<DateTime> dates, Dictionary<DateTime, double> prices, Dictionary<DateTime, double> dividends)
        {
            Dictionary<DateTime, double> returns = new Dictionary<DateTime, double>();

            double pricePrevious = Double.NaN;
            foreach (var date in dates)
            {
                double price;
                if (!prices.TryGetValue(date, out price))
                {
                    price = Double.NaN;
                }
                double dividend;
                if (!dividends.TryGetValue(date, out dividend))
                {
                    dividend = 0;
                }

                double ret = (price + dividend - pricePrevious) / pricePrevious;
                if (!Double.IsNaN(ret))
                {
                    returns.Add(date, ret);
                }

                pricePrevious = price;
            }

            return returns;
        }

        public static double CalculateSlope(List<double> xs, List<double> ys)
        {
            var xys = Enumerable.Zip(xs, ys, (x, y) => new { x = x, y = y });
            double xbar = xs.Average();
            double ybar = ys.Average();
            double slope = xys.Sum(xy => (xy.x - xbar) * (xy.y - ybar)) / xs.Sum(x => (x - xbar) * (x - xbar));
            return slope;
        }

        public static double CalculateBeta(SortedSet<DateTime> dates, Dictionary<DateTime, double> returnsX, Dictionary<DateTime, double> returnsY)
        {
            List<double> xs = new List<double>();
            List<double> ys = new List<double>();
            foreach (var date in dates)
            {
                double x;
                double y;
                if (returnsX.TryGetValue(date, out x) && returnsY.TryGetValue(date, out y))
                {
                    xs.Add(x);
                    ys.Add(y);
                }
            }
            double beta = Calculation.CalculateSlope(xs, ys);
            return beta;
        }

        public static Dictionary<DateTime, double> CalculateFactors(SortedSet<DateTime> dates, Dictionary<DateTime, double> returnsX, Dictionary<DateTime, double> returnsY, double beta)
        {
            Dictionary<DateTime, double> factors = new Dictionary<DateTime, double>();
            foreach (var date in dates)
            {
                double x;
                double y;
                if (returnsX.TryGetValue(date, out x) && returnsY.TryGetValue(date, out y))
                {
                    double factor = y - beta * x;
                    factors.Add(date, factor);
                }
            }
            return factors;
        }
    }
}

