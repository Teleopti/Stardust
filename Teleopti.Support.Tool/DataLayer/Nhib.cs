using System.Data.SqlClient;

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
            FactoryName = factoryName;
            FileName = fileName;
            _cccDataSource = new NHibDataSource(FactoryName, cccConnectionString, ApplicationDatabaseTextConstant);
            _analyticsDataSource = new NHibDataSource(FactoryName, analyticConnectionString, AnalyticsDatabaseTextConstant);
            SqlConnectionStringBuilder aggBuilder = new SqlConnectionStringBuilder(analyticConnectionString);
            aggBuilder.InitialCatalog = aggregationDatabase;
            _aggregationDataSource = new NHibDataSource(FactoryName, aggBuilder.ConnectionString, AggregationDatabaseTextConstant);
        }


       internal const string ApplicationDatabaseTextConstant = "Application DB";
       internal const string AnalyticsDatabaseTextConstant = "Analytics DB";
       internal const string AggregationDatabaseTextConstant = "Aggregation DB";
    }
}
