using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    public abstract class GenericFinderService<T> 
    {
        private readonly ISearchIndexBuilder<T> _searchIndexBuilder;
        private IDictionary<T, string> _index;

        protected GenericFinderService(ISearchIndexBuilder<T> searchIndexBuilder)
        {
            _searchIndexBuilder = searchIndexBuilder;
        }

        protected IDictionary<T, string> Search(IList<string> stringsToSearchFor)
        {
            if(_index==null)
                _index = _searchIndexBuilder.BuildIndex();

            return recursiveSearch(_index, stringsToSearchFor);
        }

        public void RebuildIndex()
        {
            _index = null;
        }


        private static IDictionary<T, string> recursiveSearch(IDictionary<T, string> workingSet, IList<string> stringsToSearchFor)
        {
            var info = CultureInfo.CurrentCulture;
            if (stringsToSearchFor.Count != 0)
            {
                var item = stringsToSearchFor.First();
                var result = (from pryl in workingSet
                              where pryl.Value.ToLower(info).Contains(item.ToLower(info))
                              select pryl).ToDictionary(k => k.Key, v => v.Value);

                var newSplit = stringsToSearchFor.ToList();
                newSplit.RemoveAt(stringsToSearchFor.IndexOf(item));
                return recursiveSearch(result, newSplit);
            }
            return workingSet;            
        }
    }
}