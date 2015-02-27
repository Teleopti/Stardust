using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Sdk.Logic
{
    public interface IDataSourceProvider
    {
        IEnumerable<DataSourceContainer> DataSourceList();
    }
}