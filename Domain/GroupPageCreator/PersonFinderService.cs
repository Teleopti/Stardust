using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    public interface IPersonFinderService
    {
        IList<IPerson> Find(string findText);
        void RebuildIndex();
    }

    public class PersonFinderService : GenericFinderService<IPerson>, IPersonFinderService
    {
        public PersonFinderService(ISearchIndexBuilder<IPerson> searchIndexBuilder) : base(searchIndexBuilder)
        {
        }

        public IList<IPerson> Find(string findText)
        {
            if(string.IsNullOrEmpty(findText))
                return new List<IPerson>();
            var splitted = findText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return Search(splitted).Keys.Distinct().ToList();
        }
    }
}