using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.ConfigTool.Transformer
{
	public class DataSourceValidCollection : List<IDataSourceEtl>
	{
		public DataSourceValidCollection(bool includeOptionAll, string datamartConnectionString)
		{
			var generalFunc = new GeneralFunctions(new GeneralInfrastructure(new BaseConfigurationRepository()));
			generalFunc.SetConnectionString(datamartConnectionString);
			AddRange(includeOptionAll ? generalFunc.DataSourceValidListIncludedOptionAll : generalFunc.DataSourceValidList);
		}
	}
}