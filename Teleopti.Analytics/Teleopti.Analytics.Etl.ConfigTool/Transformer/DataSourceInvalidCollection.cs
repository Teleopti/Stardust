using System.Collections.Generic;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.ConfigTool.Transformer
{
	public class DataSourceInvalidCollection : List<IDataSourceEtl>
	{
		public DataSourceInvalidCollection(string datamartConnectionString)
		{
			var generalFunc = new GeneralFunctions(datamartConnectionString);

			AddRange(generalFunc.DataSourceInvalidList);
		}
	}
}