using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Support.Tool.DataLayer
{
   public class Nhib
    {
        public string AnalyticDatabase { get; set; }
        public string CCCDatabase { get; set; }
        public string AggregationDatabase { get; set; }
        public string CCCConnectionString { get; set; }
        public string AnalyticConnectionString { get; set; }
        public string CCCDataSource { get; set; }
        public string AnalyticDatasource { get; set; }
        public string Factoryname { get; set; }
        public string FileName { get; set; }
        public string AggVersion { get; set; }
        public string CCCVersion { get; set; }
        public string AnalyticVersion { get; set; }





        public Nhib(string analyticDatabase, string cCCDatabase, string aggregationDatabase, string cccConnectionString, string analyticConnectionString, string cccdataSource, string analyticDatasource, string factoryname,string fileName,string cccVersion,string aggVersion, string analyticVersion)
        {
            this.AnalyticDatabase = analyticDatabase;
            this.CCCDatabase = cCCDatabase;
            this.AggregationDatabase = aggregationDatabase;
            this.CCCConnectionString = cccConnectionString;
            this.AnalyticConnectionString = analyticConnectionString;
            this.CCCDataSource = cccdataSource;
            this.AnalyticDatasource = analyticDatasource;
            this.Factoryname = factoryname;
            this.FileName = fileName;
            this.CCCVersion = cccVersion;
            this.AggVersion = aggVersion;
            this.AnalyticVersion = analyticVersion;   
        }
    }
}
