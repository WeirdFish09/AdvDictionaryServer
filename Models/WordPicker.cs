using AdvDictionaryServer.DBContext;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AdvDictionaryServer.Models
{
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

        private List<WordPriority> GetWordsByPriority(int count, int lowerPriorityLimit, int upperPriorityLimit)
        {
            List<WordPriority> WordPriorities = dbContext.WordPriorities
                .Where(wp => wp.Language == language)
                .Where(wp => wp.Value >= lowerPriorityLimit && wp.Value < upperPriorityLimit)
                .OrderBy(wp => Guid.NewGuid())
                .Take(count)
                .ToList();
            return WordPriorities;
        }

        private List<WordPriority> GetWordsByPriorityWithUpperMargin(int count, int lowerPriorityLimit, int upperPriorityLimit)
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
            int avgPriority = (highestKnownWordPriority - leastKnownWordPriority)/2;
            int unknownWordsCount = (int)Math.Ceiling(count*0.6);
            int knownWordsCount = count - unknownWordsCount;
            wp.AddRange(GetWordsByPriority(unknownWordsCount,leastKnownWordPriority,avgPriority));
            wp.AddRange(GetWordsByPriority(knownWordsCount,avgPriority,highestKnownWordPriority));
            return wp;
        }

        private List<WordPriority> SelectWithHighVariance(int count, int leastKnownWordPriority, int highestKnownWordPriority)
        {
            List<WordPriority> wp = new List<WordPriority>();
            int leastKnownPriorityUpperLimit = leastKnownWordPriority + (highestKnownWordPriority - leastKnownWordPriority)/3;
            int semiKnownWordPriorityUpperLimit =  leastKnownWordPriority + 2*(highestKnownWordPriority - leastKnownWordPriority)/3;

            int unknownWordsCount = (int)Math.Ceiling((decimal)count*leastKnownWordsPercentage/100);
            int semiknownWordsCount = (int)Math.Ceiling((decimal)count*semiKnownWordsPercentage/100);
            int wellknownWordsCount = (int)Math.Ceiling((decimal)count*wellKnownWordsPercentage/100);

            wp.AddRange(GetWordsByPriority(unknownWordsCount,leastKnownWordPriority,leastKnownPriorityUpperLimit)); 
            if(wp.Count<unknownWordsCount)
            {
                wp.AddRange(GetWordsByPriority(semiknownWordsCount + unknownWordsCount - wp.Count,leastKnownPriorityUpperLimit,semiKnownWordPriorityUpperLimit));
            } else 
            {
                wp.AddRange(GetWordsByPriority(semiknownWordsCount,leastKnownPriorityUpperLimit,semiKnownWordPriorityUpperLimit));
            }
            
            if(wp.Count < unknownWordsCount + semiknownWordsCount)
            {
                wp.AddRange(GetWordsByPriorityWithUpperMargin(count - wp.Count,semiKnownWordPriorityUpperLimit,highestKnownWordPriority));
            }else
            {
                wp.AddRange(GetWordsByPriorityWithUpperMargin(wellknownWordsCount,semiKnownWordPriorityUpperLimit,highestKnownWordPriority));
            }
            return wp;
        }
        public List<WordPriority> GenerateWordsForQuiz(int count)
        {
            int leastKnownWordPriority = dbContext.WordPriorities.Select(wp => wp.Value).Min();
            int highestKnownWordPriority = dbContext.WordPriorities.Select(wp => wp.Value).Max();
            List<WordPriority> wordPriorities = new List<WordPriority>();

            if(highestKnownWordPriority - leastKnownWordPriority < 3)
            {
                wordPriorities.AddRange(SelectWithLowVariance(count,leastKnownWordPriority,highestKnownWordPriority));
            } else 
            {
                wordPriorities.AddRange(SelectWithHighVariance(count,leastKnownWordPriority,highestKnownWordPriority));
            }
            return wordPriorities;
        }

    }
}