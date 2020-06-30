using System;
using System.Collections.Generic;
using System.Linq;
using QuantRiskLib;

namespace FactorAnalysis
{
    class Calculation
    {
        //caiculate returns
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

        public static Dictionary<string, Dictionary<DateTime, double>> GetReturnDict(SortedSet<DateTime> dates, Dictionary<string, Dictionary<DateTime, double>> priceDict, Dictionary<string, Dictionary<DateTime, double>> dividendDict)
        {
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
            return returnDict;
        }

        //get previous return
        public static Dictionary<DateTime, double> GetLagReturns(SortedSet<DateTime> dates, Dictionary<DateTime, double> returns)
        {
            Dictionary<DateTime, double> prevReturns = new Dictionary<DateTime, double>();
            DateTime prevDate = dates.First();
            foreach (var date in dates)
            {
                if (date > dates.First())
                {
                    double prevReturn;
                    if (returns.TryGetValue(prevDate, out prevReturn))
                    {
                        prevReturns.Add(date, prevReturn);
                    }
                }
            }
            return prevReturns;
        }

        public static Dictionary<string, Dictionary<DateTime, double>> GetLagReturnDict(SortedSet<DateTime> dates, Dictionary<string, Dictionary<DateTime, double>> returnDict)
        {
            Dictionary<string, Dictionary<DateTime, double>> lagReturnDict = new Dictionary<string, Dictionary<DateTime, double>>();
            foreach (var item in returnDict)
            {
                string ticker = item.Key;
                Dictionary<DateTime, double> returns = item.Value;
                Dictionary<DateTime, double> lagReturns = Calculation.GetLagReturns(dates, returns);
                lagReturnDict.Add(ticker, lagReturns);
            }
            return lagReturnDict;
        }

        //calculate residuals using wls
        public static Dictionary<DateTime, double> CalculateResiduals(SortedSet<DateTime> dates, Dictionary<DateTime, double> returnsX, Dictionary<DateTime, double> returnsY, double weight)
        {
            List<double> xs = new List<double>();
            List<double> ys = new List<double>();
            List<DateTime> commonDates = new List<DateTime>();
            foreach (var date in dates)
            {
                double x;
                double y;
                if (returnsX.TryGetValue(date, out x) && returnsY.TryGetValue(date, out y))
                {
                    xs.Add(x);
                    ys.Add(y);
                    commonDates.Add(date);
                }
            }

            WeightedLeastSquares model = new WeightedLeastSquares(ys.ToArray(), xs.ToArray(), true, weight);
            double[] wlsRes = model.Residuals.ToVectorArray();

            Dictionary<DateTime, double> wlsResiduals = commonDates.Zip(wlsRes, (k, v) => new { k, v })
              .ToDictionary(x => x.k, x => x.v);

            return wlsResiduals;
        }

        //get residual of regression on lagged return
        public static Dictionary<string, Dictionary<DateTime, double>> GetResidualDict(SortedSet<DateTime> dates, Dictionary<string, Dictionary<DateTime, double>> returnDictX, Dictionary<string, Dictionary<DateTime, double>> returnDictY)
        {
            //get the dictionary of betas of lagged return regression
            Dictionary<string, Dictionary<DateTime, double>> residualDict = new Dictionary<string, Dictionary<DateTime, double>>();
            foreach (var item in returnDictY)
            {
                string ticker = item.Key;
                Dictionary<DateTime, double> returnsX = returnDictX[ticker];
                Dictionary<DateTime, double> returnsY = item.Value;
                Dictionary<DateTime, double> resReturns;
                resReturns = Calculation.CalculateResiduals(dates, returnsX, returnsY, 1);
                residualDict.Add(ticker, resReturns);
            }
            return residualDict;
        }

        //get factors
        public static Dictionary<string, Dictionary<DateTime, double>> GetFactorDict(SortedSet<DateTime> dates, Dictionary<string, Dictionary<DateTime, double>> returnDict, double weight, string tickerX = "SPY")
        {
            //get the dictionary of betas of lagged return regression
            Dictionary<string, Dictionary<DateTime, double>> factorDict = new Dictionary<string, Dictionary<DateTime, double>>();
            foreach (var item in returnDict)
            {
                string ticker = item.Key;
                Dictionary<DateTime, double> returnsX = returnDict["SPY"];
                Dictionary<DateTime, double> returnsY = item.Value;
                Dictionary<DateTime, double> factors;
                if (ticker != tickerX)
                {
                    factors = Calculation.CalculateResiduals(dates, returnsX, returnsY,weight);
                }
                else
                {
                    factors = factorDict["SPY"];
                }
                factorDict.Add(ticker, factors);
            }
            return factorDict;
        }


 
    }

}
