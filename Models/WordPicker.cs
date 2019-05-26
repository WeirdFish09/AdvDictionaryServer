using AdvDictionaryServer.DBContext;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

namespace AdvDictionaryServer.Models
{
    enum WordsGroup { WellKnown, SemiKnown, LeastKnown };
    public class WordPicker
    {


        public const int wellKnownWordsPercentage = 10;
        public const int semiKnownWordsPercentage = 30;
        public const int leastKnownWordsPercentage = 60;

        private readonly DictionaryDBContext dbContext;
        private readonly Language language;



        public WordPicker(DictionaryDBContext DictionaryDBContext, Language language)
        {
            dbContext = DictionaryDBContext;
            this.language = language;
        }

        public List<WordPriority> GetWordsByPriority(int count, int lowerPriorityLimit, int upperPriorityLimit)
        {
            List<WordPriority> WordPriorities = dbContext.WordPriorities
                .Where(wp => wp.Language == language)
                .Where(wp => wp.Value >= lowerPriorityLimit && wp.Value < upperPriorityLimit)
                .OrderBy(wp => Guid.NewGuid())
                .Take(count)
                .ToList();
            return WordPriorities;
        }

        public List<WordPriority> GetWordsByPriorityWithUpperMargin(int count, int lowerPriorityLimit, int upperPriorityLimit)
        {
            List<WordPriority> WordPriorities = dbContext.WordPriorities
                .Where(wp => wp.Language == language)
                .Where(wp => wp.Value >= lowerPriorityLimit && wp.Value <= upperPriorityLimit)
                .OrderBy(wp => Guid.NewGuid())
                .Take(count)
                .ToList();
            return WordPriorities;
        }

        private List<WordPriority> SelectWithLowVariance(int count, int leastKnownWordPriority, int highestKnownWordPriority)
        {
            List<WordPriority> wp = new List<WordPriority>();
            int avgPriority = (highestKnownWordPriority - leastKnownWordPriority) / 2;
            int unknownWordsCount = (int)Math.Ceiling(count * 0.6);
            int knownWordsCount = count - unknownWordsCount;
            wp.AddRange(GetWordsByPriority(unknownWordsCount, leastKnownWordPriority, avgPriority));
            if (wp.Count() < unknownWordsCount)
            {
                wp.AddRange(GetWordsByPriority(count - wp.Count, avgPriority, highestKnownWordPriority));
            }
            else
            {
                wp.AddRange(GetWordsByPriority(knownWordsCount, avgPriority, highestKnownWordPriority));
            }
            return wp;
        }

        private List<WordPriority> SelectWithHighVariance(int count, int leastKnownWordPriority, int highestKnownWordPriority)
        {
            List<WordPriority> wp = new List<WordPriority>();
            int leastKnownPriorityUpperLimit = leastKnownWordPriority + (highestKnownWordPriority - leastKnownWordPriority) / 3;
            int semiKnownWordPriorityUpperLimit = leastKnownWordPriority + 2 * (highestKnownWordPriority - leastKnownWordPriority) / 3;

            int unknownWordsCount = (int)Math.Ceiling((decimal)count * leastKnownWordsPercentage / 100);
            int semiknownWordsCount = (int)Math.Ceiling((decimal)count * semiKnownWordsPercentage / 100);
            int wellknownWordsCount = (int)Math.Ceiling((decimal)count * wellKnownWordsPercentage / 100);

            wp.AddRange(GetWordsByPriority(unknownWordsCount, leastKnownWordPriority, leastKnownPriorityUpperLimit));
            if (wp.Count < unknownWordsCount)
            {
                wp.AddRange(GetWordsByPriority(semiknownWordsCount + unknownWordsCount - wp.Count, leastKnownPriorityUpperLimit, semiKnownWordPriorityUpperLimit));
            }
            else
            {
                wp.AddRange(GetWordsByPriority(semiknownWordsCount, leastKnownPriorityUpperLimit, semiKnownWordPriorityUpperLimit));
            }

            if (wp.Count < unknownWordsCount + semiknownWordsCount)
            {
                wp.AddRange(GetWordsByPriorityWithUpperMargin(count - wp.Count, semiKnownWordPriorityUpperLimit, highestKnownWordPriority));
            }
            else
            {
                wp.AddRange(GetWordsByPriorityWithUpperMargin(wellknownWordsCount, semiKnownWordPriorityUpperLimit, highestKnownWordPriority));
            }
            return wp;
        }

        private List<WordPriority> SelectWithHighVarianceNew(int count, int lowestKnownWordPriority, int highestKnownWordPriority)
        {
            WordsSelector wordsSelector;

            var priorities = dbContext.WordPriorities.Select(wordPrioroty => wordPrioroty.Value);

            switch (SelectGroupWithMostElements(priorities, lowestKnownWordPriority, highestKnownWordPriority))
            {
                case WordsGroup.LeastKnown:
                    wordsSelector = new LeastKnownWordsSelector(dbContext, language, count);
                    break;
                case WordsGroup.SemiKnown:
                    wordsSelector = new SemiKnownWordsSelector(dbContext, language, count);
                    break;
                default:
                    wordsSelector = new WellKnownWordsSelector(dbContext, language, count);
                    break;
            }
            
            return wordsSelector.SelectWordPriorities(leastKnownWordsPercentage, semiKnownWordsPercentage, wellKnownWordsPercentage);
        }



        private WordsGroup SelectGroupWithMostElements(IEnumerable<int> priorities, int lowestKnownWordPriority, int highestKnownWordPriority)
        {
            double centralPriority = (highestKnownWordPriority + lowestKnownWordPriority) / 2;
            double mean = StatisticsCalculation.CalculateMean(priorities);
            double standardDeviation = StatisticsCalculation.CalucaleStandardDeviation(priorities);

            int leastKnownPriorityUpperLimit = lowestKnownWordPriority + (highestKnownWordPriority - lowestKnownWordPriority) / 3;
            int semiKnownWordPriorityUpperLimit = highestKnownWordPriority + 2 * (highestKnownWordPriority - highestKnownWordPriority) / 3;

            if (Math.Abs(centralPriority - mean) > standardDeviation)
            {
                if (mean < centralPriority)
                {
                    return WordsGroup.LeastKnown;
                }
                return WordsGroup.WellKnown;
            }
            else
            {
                if (mean > leastKnownPriorityUpperLimit)
                {
                    if (mean > semiKnownWordPriorityUpperLimit)
                    {
                        return WordsGroup.WellKnown;
                    }
                    return WordsGroup.SemiKnown;
                }
                return WordsGroup.LeastKnown;
            }
        }

        //public List<WordPriority> GenerateWordsForQuiz(int count)
        //{
        //    int leastKnownWordPriority = dbContext.WordPriorities.Select(wp => wp.Value).Min();
        //    int highestKnownWordPriority = dbContext.WordPriorities.Select(wp => wp.Value).Max();
        //    List<WordPriority> wordPriorities = new List<WordPriority>();

        //    if (highestKnownWordPriority - leastKnownWordPriority < 3)
        //    {
        //        wordPriorities.AddRange(SelectWithLowVariance(count, leastKnownWordPriority, highestKnownWordPriority));
        //    } else
        //    {
        //        wordPriorities.AddRange(SelectWithHighVarianceNew(count, leastKnownWordPriority, highestKnownWordPriority));
        //    }
        //    return wordPriorities;
        //}

        public List<WordPriority> GenerateWordsForQuiz(int count)
        {
            int leastKnownWordPriority = dbContext.WordPriorities.Select(wp => wp.Value).Min();
            int highestKnownWordPriority = dbContext.WordPriorities.Select(wp => wp.Value).Max();
            List<WordPriority> wordPriorities = new List<WordPriority>();
            wordPriorities.AddRange(SelectWithHighVarianceNew(count, leastKnownWordPriority, highestKnownWordPriority));
            return wordPriorities;
        }

    }


        abstract class WordsSelector
        {
            protected DictionaryDBContext dbContext;
            protected int count;
            Language language;
            public WordsSelector(DictionaryDBContext dictionaryDBContext, Language language, int wordsCount)
            {
                dbContext = dictionaryDBContext;
                this.language = language;
                count = wordsCount;
            }

            protected List<WordPriority> GetWordsByPriority(int count, int lowerPriorityLimit, int upperPriorityLimit)
            {
            Random random = new Random();
            List<WordPriority> WordPriorities = new List<WordPriority>();
            var wpIDs = dbContext.WordPriorities
                                    .Where(wp => wp.Language == language)
                                    .Where(wp => wp.Value >= lowerPriorityLimit && wp.Value < upperPriorityLimit)
                                    .Select(wp => wp.ID);
            var selectedIDs = wpIDs.OrderBy(wp => random.Next()).Take(count);
            foreach (var id in selectedIDs)
            {
                WordPriorities.Add(dbContext.WordPriorities
                                                     .Where(wp => wp.ID == id)
                                                     .Include(wp => wp.ForeignWord)
                                                     .Include(wp => wp.NativePhrase).Single());
            }
            return WordPriorities;
            }

            protected List<WordPriority> GetWordsByPriorityWithUpperMargin(int count, int lowerPriorityLimit, int upperPriorityLimit)
            {
            Random random = new Random();
            List<WordPriority> WordPriorities = new List<WordPriority>();
            var wpIDs = dbContext.WordPriorities
                                    .Where(wp => wp.Language == language)
                                    .Where(wp => wp.Value >= lowerPriorityLimit && wp.Value <= upperPriorityLimit)
                                    .Select(wp => wp.ID);
            var selectedIDs = wpIDs.OrderBy(wp => random.Next()).Take(count);
            foreach(var id in selectedIDs)
            {
                WordPriorities.Add(dbContext.WordPriorities
                                                     .Where(wp => wp.ID == id)
                                                     .Include(wp => wp.ForeignWord)
                                                     .Include(wp => wp.NativePhrase).Single());
            };
            return WordPriorities;
            }

            abstract public List<WordPriority> SelectWordPriorities(int leastKnownWordsPercentage, int semiKnownWordsPercentage, int wellKnownWordsPercentage);
        }

        class WellKnownWordsSelector : WordsSelector
        {
            public WellKnownWordsSelector(DictionaryDBContext dictionaryDBContext, Language language, int wordsCount) : 
                base(dictionaryDBContext, language, wordsCount)
            {

            }

            public override List<WordPriority> SelectWordPriorities(int leastKnownWordsPercentage, int semiKnownWordsPercentage, int wellKnownWordsPercentage)
            {
                List<WordPriority> wordPriorities = new List<WordPriority>();

                int leastKnownWordPriority = dbContext.WordPriorities.Select(wp => wp.Value).Min();
                int highestKnownWordPriority = dbContext.WordPriorities.Select(wp => wp.Value).Max();
                int leastKnownPriorityUpperLimit = Convert.ToInt32(leastKnownWordPriority + Math.Ceiling((decimal)(highestKnownWordPriority - leastKnownWordPriority) / 3));
                int semiKnownWordPriorityUpperLimit = Convert.ToInt32(leastKnownWordPriority + Math.Ceiling((decimal)2 * (highestKnownWordPriority - leastKnownWordPriority) / 3));
            //int leastKnownPriorityUpperLimit = leastKnownWordPriority + (highestKnownWordPriority - leastKnownWordPriority) / 3;
            //int semiKnownWordPriorityUpperLimit = leastKnownWordPriority + 2 * (highestKnownWordPriority - leastKnownWordPriority) / 3;

            int unknownWordsCount = (int)Math.Ceiling((decimal)count * leastKnownWordsPercentage / 100);
                int semiknownWordsCount = (int)Math.Ceiling((decimal)count * semiKnownWordsPercentage / 100);
                int wellknownWordsCount = (int)Math.Ceiling((decimal)count * wellKnownWordsPercentage / 100);

                wordPriorities.AddRange(GetWordsByPriority(unknownWordsCount, leastKnownWordPriority, leastKnownPriorityUpperLimit));
                if (wordPriorities.Count < unknownWordsCount)
                {
                    wordPriorities.AddRange(GetWordsByPriority(semiknownWordsCount + unknownWordsCount - wordPriorities.Count, leastKnownPriorityUpperLimit, semiKnownWordPriorityUpperLimit));
                }
                else
                {
                    wordPriorities.AddRange(GetWordsByPriority(semiknownWordsCount, leastKnownPriorityUpperLimit, semiKnownWordPriorityUpperLimit));
                }

                if (wordPriorities.Count < unknownWordsCount + semiknownWordsCount)
                {
                    wordPriorities.AddRange(GetWordsByPriorityWithUpperMargin(count - wordPriorities.Count, semiKnownWordPriorityUpperLimit, highestKnownWordPriority));
                }
                else
                {
                    wordPriorities.AddRange(GetWordsByPriorityWithUpperMargin(wellknownWordsCount, semiKnownWordPriorityUpperLimit, highestKnownWordPriority));
                }

                return wordPriorities;
            }
        }

    class LeastKnownWordsSelector : WordsSelector
    {
        public LeastKnownWordsSelector(DictionaryDBContext dictionaryDBContext, Language language, int wordsCount) :
            base(dictionaryDBContext, language, wordsCount)
        {

        }

        public override List<WordPriority> SelectWordPriorities(int leastKnownWordsPercentage, int semiKnownWordsPercentage, int wellKnownWordsPercentage)
        {
            List<WordPriority> wordPriorities = new List<WordPriority>();

            int leastKnownWordPriority = dbContext.WordPriorities.Select(wp => wp.Value).Min();
            int highestKnownWordPriority = dbContext.WordPriorities.Select(wp => wp.Value).Max();
            int leastKnownPriorityUpperLimit = Convert.ToInt32(leastKnownWordPriority + Math.Ceiling((decimal)(highestKnownWordPriority - leastKnownWordPriority) / 3));
            int semiKnownWordPriorityUpperLimit = Convert.ToInt32(leastKnownWordPriority + Math.Ceiling((decimal)2 * (highestKnownWordPriority - leastKnownWordPriority) / 3));
            //int leastKnownPriorityUpperLimit = leastKnownWordPriority + (highestKnownWordPriority - leastKnownWordPriority) / 3;
            //int semiKnownWordPriorityUpperLimit = leastKnownWordPriority + 2 * (highestKnownWordPriority - leastKnownWordPriority) / 3;

            int unknownWordsCount = (int)Math.Ceiling((decimal)count * leastKnownWordsPercentage / 100);
            int semiknownWordsCount = (int)Math.Ceiling((decimal)count * semiKnownWordsPercentage / 100);
            int wellknownWordsCount = (int)Math.Ceiling((decimal)count * wellKnownWordsPercentage / 100);

            wordPriorities.AddRange(GetWordsByPriorityWithUpperMargin(wellknownWordsCount, semiKnownWordPriorityUpperLimit, highestKnownWordPriority));
            if (wordPriorities.Count < wellknownWordsCount)
            {
                wordPriorities.AddRange(GetWordsByPriority(semiknownWordsCount + unknownWordsCount - wordPriorities.Count, leastKnownPriorityUpperLimit, semiKnownWordPriorityUpperLimit));
            }
            else
            {
                wordPriorities.AddRange(GetWordsByPriority(semiknownWordsCount, leastKnownPriorityUpperLimit, semiKnownWordPriorityUpperLimit));
            }

            if (wordPriorities.Count < wellknownWordsCount + semiknownWordsCount)
            {
                wordPriorities.AddRange(GetWordsByPriority(count - wordPriorities.Count, leastKnownWordPriority, leastKnownPriorityUpperLimit));
            }
            else
            {
                wordPriorities.AddRange(GetWordsByPriority(unknownWordsCount, leastKnownWordPriority, leastKnownPriorityUpperLimit));
            }

            return wordPriorities;
        }
    }

    class SemiKnownWordsSelector : WordsSelector
    {
        public SemiKnownWordsSelector(DictionaryDBContext dictionaryDBContext, Language language, int wordsCount) :
            base(dictionaryDBContext, language, wordsCount)
        {

        }

        public override List<WordPriority> SelectWordPriorities(int leastKnownWordsPercentage, int semiKnownWordsPercentage, int wellKnownWordsPercentage)
        {
            List<WordPriority> wordPriorities = new List<WordPriority>();

            int leastKnownWordPriority = dbContext.WordPriorities.Select(wp => wp.Value).Min();
            int highestKnownWordPriority = dbContext.WordPriorities.Select(wp => wp.Value).Max();
            int leastKnownPriorityUpperLimit = Convert.ToInt32(leastKnownWordPriority + Math.Ceiling((decimal)(highestKnownWordPriority - leastKnownWordPriority) / 3));
            int semiKnownWordPriorityUpperLimit = Convert.ToInt32(leastKnownWordPriority + Math.Ceiling((decimal)2 * (highestKnownWordPriority - leastKnownWordPriority) / 3));

            int unknownWordsCount = (int)Math.Ceiling((decimal)count * leastKnownWordsPercentage / 100);
            int semiknownWordsCount = (int)Math.Ceiling((decimal)count * semiKnownWordsPercentage / 100);
            int wellknownWordsCount = (int)Math.Ceiling((decimal)count * wellKnownWordsPercentage / 100);

            wordPriorities.AddRange(GetWordsByPriorityWithUpperMargin(wellknownWordsCount, semiKnownWordPriorityUpperLimit, highestKnownWordPriority));
            if (wordPriorities.Count < wellknownWordsCount)
            {
                wordPriorities.AddRange(GetWordsByPriority(semiknownWordsCount + unknownWordsCount - wordPriorities.Count, leastKnownPriorityUpperLimit, semiKnownWordPriorityUpperLimit));
            }
            else
            {
                wordPriorities.AddRange(GetWordsByPriority(semiknownWordsCount, leastKnownPriorityUpperLimit, semiKnownWordPriorityUpperLimit));
            }

            if (wordPriorities.Count < wellknownWordsCount + semiknownWordsCount)
            {
                wordPriorities.AddRange(GetWordsByPriority(count - wordPriorities.Count, leastKnownWordPriority, leastKnownPriorityUpperLimit));
            }
            else
            {
                wordPriorities.AddRange(GetWordsByPriority(unknownWordsCount, leastKnownWordPriority, leastKnownPriorityUpperLimit));
            }

            return wordPriorities;
        }
    }
}

