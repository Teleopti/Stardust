using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Win.Main
{
    public interface IDataSourceHandler
    {
        IAvailableDataSourcesProvider AvailableDataSourcesProvider();
        IEnumerable<IDataSourceProvider> DataSourceProviders();
    }
}