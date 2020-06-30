using System;
using System.Collections.Generic;

namespace FactorAnalysis
{
    class Program
    {
        private static void Main()
        {
            //read data
            List<string[]> priceString = DataPreprocess.ReadCsvFile(@"/Users/tushilan/Desktop/factoranalysis/MarketDataPrice.csv");
            List<string[]> dividendString = DataPreprocess.ReadCsvFile(@"/Users/tushilan/Desktop/factoranalysis/MarketDataDividend.csv");

            //convert string data to dictionary of dictionary
            Dictionary<string, Dictionary<DateTime, double>> priceDict = DataPreprocess.ConvertDict(priceString);
            Dictionary<string, Dictionary<DateTime, double>> dividendDict = DataPreprocess.ConvertDict(dividendString, "sum");

            //get sorted dates set
            SortedSet<DateTime> dates = DataPreprocess.GetDates(priceDict);

            //get the dictionary of returns
            Dictionary<string, Dictionary<DateTime, double>> returnDict = Calculation.GetReturnDict(dates, priceDict, dividendDict);

            //get the dictionary of returns adding lagged var
            Dictionary<string, Dictionary<DateTime, double>> lagReturnDict = Calculation.GetLagReturnDict(dates, returnDict);

            //get the residual of regression on lagged return
            Dictionary<string, Dictionary<DateTime, double>> resReturnDict = Calculation.GetResidualDict(dates, lagReturnDict, returnDict);

            //get simple factors
            Dictionary<string, Dictionary<DateTime, double>> simpleFactorDict = Calculation.GetFactorDict(dates, returnDict,1);

            //get factors using residual return
            Dictionary<string, Dictionary<DateTime, double>> lagFactorDict = Calculation.GetFactorDict(dates, resReturnDict,1);

            //get factors using residual return with decayfactor 0.99
            Dictionary<string, Dictionary<DateTime, double>> wlsFactorDict = Calculation.GetFactorDict(dates, resReturnDict,0.99);


            System.Console.WriteLine("Success!");
        }
    }


}

