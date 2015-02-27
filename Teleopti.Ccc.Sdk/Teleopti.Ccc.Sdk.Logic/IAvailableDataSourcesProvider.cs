using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic
{
    public interface IAvailableDataSourcesProvider
    {
        IEnumerable<IDataSource> AvailableDataSources();
        IEnumerable<IDataSource> UnavailableDataSources();
    }
}