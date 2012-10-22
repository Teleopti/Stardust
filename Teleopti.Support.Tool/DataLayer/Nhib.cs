using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Teleopti.Support.Tool.DataLayer
{
   public class Nhib
    {
        public string FactoryName { get; set; }
        public string FileName { get; set; }
        private NHibDataSource _cccDataSource;
        private NHibDataSource _analyticsDataSource;
        private NHibDataSource _aggregationDataSource;

        public NHibDataSource AggregationDataSource
       {
           get { return _aggregationDataSource; }
       }

       public NHibDataSource AnalyticsDataSource
       {
           get { return _analyticsDataSource; }
       }

       public NHibDataSource CccDataSource
       {
           get { return _cccDataSource; }
       }

       public Nhib(string cccConnectionString, string analyticConnectionString, string aggregationDatabase, string factoryName, string fileName)
        {
            _cccDataSource = new NHibDataSource(cccConnectionString, ApplicationDatabaseTextConstant);
            _analyticsDataSource = new NHibDataSource(analyticConnectionString, AnalyticsDatabaseTextConstant);
            SqlConnectionStringBuilder aggBuilder = new SqlConnectionStringBuilder(analyticConnectionString);
            aggBuilder.InitialCatalog = aggregationDatabase;
            _aggregationDataSource = new NHibDataSource(aggBuilder.ConnectionString, AggregationDatabaseTextConstant);
            FactoryName = factoryName;
            FileName = fileName;
        }


       internal const string ApplicationDatabaseTextConstant = "Application DB";
       internal const string AnalyticsDatabaseTextConstant = "Analytics DB";
       internal const string AggregationDatabaseTextConstant = "Aggregation DB";
    }
}
