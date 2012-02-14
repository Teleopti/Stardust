using System.Collections.Generic;
using System.Configuration;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Transformer;


namespace Teleopti.Analytics.Etl.ConfigTool.RaptorDataSource
{
    public class DataSourceCollection : List<IDataSource>
    {
        public DataSourceCollection()
        {
            string stageConnectionString = ConfigurationManager.AppSettings["stageConnectionString"];
            string datamartConnectionString = ConfigurationManager.AppSettings["datamartConnectionString"];

            var generalFunc = new GeneralFunctions(stageConnectionString, datamartConnectionString);
            //_dataSourceList = generalFunc.DataSourceList;

            AddRange(generalFunc.DataSourceList);
        }



    }
}