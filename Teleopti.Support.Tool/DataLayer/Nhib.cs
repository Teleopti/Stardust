using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Support.Tool.DataLayer
{
   public class Nhib
    {
        public string AnalyticDatabase { get; set; }
        public string CccDatabase { get; set; }
        public string AggregationDatabase { get; set; }
        public string CccConnectionString { get; set; }
        public string AnalyticConnectionString { get; set; }
        public string CccDataSource { get; set; }
        public string AnalyticDataSource { get; set; }
        public string FactoryName { get; set; }
        public string FileName { get; set; }
        public string AggVersion { get; set; }
        public string CccVersion { get; set; }
        public string AnalyticVersion { get; set; }





        public Nhib(string analyticDatabase, string cccDatabase, string aggregationDatabase, string cccConnectionString, string analyticConnectionString, string cccDataSource, string analyticDataSource, string factoryName,string fileName,string cccVersion,string aggVersion, string analyticVersion)
        {
            this.AnalyticDatabase = analyticDatabase;
            this.CccDatabase = cccDatabase;
            this.AggregationDatabase = aggregationDatabase;
            this.CccConnectionString = cccConnectionString;
            this.AnalyticConnectionString = analyticConnectionString;
            this.CccDataSource = cccDataSource;
            this.AnalyticDataSource = analyticDataSource;
            this.FactoryName = factoryName;
            this.FileName = fileName;
            this.CccVersion = cccVersion;
            this.AggVersion = aggVersion;
            this.AnalyticVersion = analyticVersion;   
        }
    }
}
