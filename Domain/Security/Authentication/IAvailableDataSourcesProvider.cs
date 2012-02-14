using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public interface IAvailableDataSourcesProvider
    {
        IEnumerable<IDataSource> AvailableDataSources();
        IEnumerable<IDataSource> UnavailableDataSources();
    }
}