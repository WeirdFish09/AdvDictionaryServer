using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvDictionaryServer.Models
{
    public static class StatisticsCalculation
    {
        public static double CalculateMean(IEnumerable<int> priorities)
        {
            return priorities.Average();
        }

        public static double CalculateVariance(IEnumerable<int> priorities)
        {
            double mean = CalculateMean(priorities);
            double variance = 0;

            foreach(var priority in priorities)
            {
                variance += Math.Pow(priority - mean, 2);
            }
            variance /= priorities.Count();

            return variance;
        }

        public static double CalucaleStandardDeviation(IEnumerable<int> priorities)
        {
            return Math.Sqrt(CalculateVariance(priorities));
        }
    }
}
