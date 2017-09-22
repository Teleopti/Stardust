using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.ConfigTool.Transformer
{
	public class DataSourceInvalidCollection : List<IDataSourceEtl>
	{
		public DataSourceInvalidCollection(string datamartConnectionString)
		{
			var generalFunc = new GeneralFunctions(datamartConnectionString, new BaseConfigurationRepository());

			AddRange(generalFunc.DataSourceInvalidList);
		}
	}
}