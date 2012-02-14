using System.Collections.Generic;
using System.Configuration;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Analytics.Etl.ConfigTool.Transformer
{
    public class DataSourceValidCollection : List<IDataSourceEtl>
    {
        public DataSourceValidCollection(bool includeOptionAll)
        {
            string datamartConnectionString = ConfigurationManager.AppSettings["datamartConnectionString"];

            var generalFunc = new GeneralFunctions(datamartConnectionString);
            AddRange(includeOptionAll ? generalFunc.DataSourceValidListIncludedOptionAll : generalFunc.DataSourceValidList);
        }
    }
}