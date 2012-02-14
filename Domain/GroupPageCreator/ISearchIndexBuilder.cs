using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    public interface ISearchIndexBuilder<T>
    {
        IDictionary<T, string> BuildIndex();
    }
}