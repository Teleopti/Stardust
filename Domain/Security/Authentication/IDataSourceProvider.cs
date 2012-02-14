using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public interface IDataSourceProvider
    {
        IEnumerable<DataSourceContainer> DataSourceList();
    }
}