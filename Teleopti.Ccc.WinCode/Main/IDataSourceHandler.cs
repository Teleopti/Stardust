using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.WinCode.Main
{
    public interface IDataSourceHandler
    {
        IAvailableDataSourcesProvider AvailableDataSourcesProvider();
        IEnumerable<IDataSourceProvider> DataSourceProviders();
    }
}