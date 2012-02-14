using System;
using System.Globalization;
using Microsoft.AnalysisServices.AdomdClient;
using Teleopti.Analytics.Portal.AnalyzerProxy;

namespace Teleopti.Analytics.Portal.Utils
{
    /// <summary>
    /// Class for special reports not shown through the standard selection page. 
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-05-11    
    /// /// </remarks>
    public class CccReports : IDisposable
    {
        private readonly string _olapConnectionString;
        private AdomdConnection _olapConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="CccReports"/> class.
        /// </summary>
        /// <param name="olapConnectionString">The olap connection string.</param>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-05-11    
        /// /// </remarks>
        public CccReports(string olapConnectionString)
        {
            _olapConnectionString = olapConnectionString;
        }

        /// <summary>
        /// Get Scorecard data for one Agent.
        /// </summary>
        /// <param name="personCode">The person code.</param>
        /// <param name="scorecardCode">The scorecard code.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-05-11    
        /// /// </remarks>
        public CellSet GetAgentScorecardData(Guid personCode, Guid scorecardCode, int period)
        {
            if (PermissionInformation.IsPmAuthenticationWindows)
                return getScorecardData(personCode, scorecardCode, period);

            return getScorecardDataByImpersonation(personCode, scorecardCode, period);
        }

        private CellSet getScorecardDataByImpersonation(Guid personCode, Guid scorecardCode, int period)
        {
            CellSet cellSet;
            using (new ImpersonatedUser())
            {
                _olapConnection = new AdomdConnection(_olapConnectionString);

                var cmdType = new AdomdCommand
                {
                    CommandText = getMdxString(period, personCode, scorecardCode),
                    Connection = _olapConnection,
                    CommandTimeout = 600
                };

                _olapConnection.Open();
                
                cellSet = cmdType.ExecuteCellSet();
            }

            return cellSet;
        }

        private CellSet getScorecardData(Guid personCode, Guid scorecardCode, int period)
        {
            _olapConnection = new AdomdConnection(_olapConnectionString);

            var cmdType = new AdomdCommand
            {
                CommandText = getMdxString(period, personCode, scorecardCode),
                Connection = _olapConnection,
                CommandTimeout = 600
            };

            _olapConnection.Open();

            return cmdType.ExecuteCellSet();
        }

        private static string getMdxString(int period, Guid personCode, Guid scorecardCode)
        {
            string datePeriodString1 = "";
            string datePeriodString2 = "";
            string datePeriodString3 = "";
            string dateParameter = string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd}",
                                                 new object[] { DateTime.Today.Date });
            dateParameter += "T00:00:00";
            setDatePeriods(dateParameter, period, ref datePeriodString1, ref datePeriodString2, ref datePeriodString3);

            //Build MDX expression
            return "WITH MEMBER [Measures].[Team Actual] AS " +
                   "SUM(ANCESTOR([Person].[Person Site].currentmember,2),[Measures].[Measures].[KPI Actual]) " +
                   "MEMBER [Measures].[Last Period Actual] AS " +
                   "SUM(PARALLELPERIOD( " + datePeriodString1 + ",1," + datePeriodString2 + ".CURRENTMEMBER ),[Measures].[Measures].[KPI Actual]) " +
                   "MEMBER [Measures].[Trend] AS " +
                   "CASE " +
                   "WHEN ISEMPTY([Measures].[KPI Actual]) THEN NULL " +
                   "WHEN ROUND([Measures].[KPI Actual],0) > ROUND([Measures].[Last Period Actual],0) THEN 1 " +
                   "WHEN ROUND([Measures].[KPI Actual],0) <  ROUND([Measures].[Last Period Actual],0) THEN -1 " +
                   "ELSE 0 " +
                   "END " +
                   "MEMBER [Measures].[Color] AS " +
                   "CASE " +
                   "WHEN [Measures].[KPI Actual] > [Measures].[Max Value] THEN [Measures].[Higher Than Max Color] " +
                   "WHEN [Measures].[KPI Actual] < [Measures].[Min Value] THEN [Measures].[Lower Than Min Color] " +
                   "ELSE [Measures].[Between Color] " +
                   "END " +
                   "SELECT { [Measures].[Measures].[Target Value],[Measures].[KPI Actual] ,[Measures].[Team Actual],[Measures].[Color],[Measures].[Last Period Actual],[Measures].[Trend]} " +
                   "ON COLUMNS, NON EMPTY { ([KPI].[KPI].[Resource Key].ALLMEMBERS ) }  " +
                   "DIMENSION PROPERTIES MEMBER_CAPTION, " +
                   "MEMBER_UNIQUE_NAME ON ROWS FROM ( " +
                   "SELECT ( { [Person].[Person Site].[Person Code].&[{" + personCode + "}] } )  " +

                   "ON COLUMNS FROM ( SELECT ( { [Scorecard].[Scorecard].[Scorecard Code].&[{" + scorecardCode + "}] } )  " +
                   "ON COLUMNS FROM [Teleopti Analytics])) " +
                   "WHERE ( [Scorecard].[Scorecard].[Scorecard Code].&[{" + scorecardCode + "}],  " +
                   "[Person].[Person Site].[Person Code].&[{" + personCode + "}], " +
                   "[Scorecard].[Period].&[" + period + "], " + datePeriodString3;
            //  " + DatePeriodString1 + ".&[" + DateParameter + "])";
        }

        private static void setDatePeriods(string dateParameter, int period, ref string datePeriodString1, ref string datePeriodString2, ref string datePeriodString3)
        {
            switch (period)
            {
                case 0: //dag
                    datePeriodString1 = "[Date].[Date Year Month].[Date]";
                    datePeriodString2 = "[Date].[Date Year Month]";
                    datePeriodString3 = "[Date].[Date Year Month].[Date].&[" + dateParameter + "])";
                    break;
                case 1: //vecka
                    datePeriodString1 = "[Date].[Date Year Week].[Year Week]";
                    datePeriodString2 = "[Date].[Date Year Week]";
                    datePeriodString3 = "[Date].[Date Year Week].[Date].&[" + dateParameter + "].PARENT)";
                    //+ DatePeriodString1 + ".&[" + DateParameter + "])
                    break;
                case 2://månad
                    datePeriodString1 = "[Date].[Date Year Month].[Year Month]";
                    datePeriodString2 = "[Date].[Date Year Month]";
                    datePeriodString3 = "[Date].[Date Year Month].[Date].&[" + dateParameter + "].PARENT)";
                    break;
                case 3://kvartal
                    datePeriodString1 = "[Date].[Date Year Month].[Quarter]";
                    datePeriodString2 = "[Date].[Date Year Month]";
                    datePeriodString3 = "ANCESTOR([Date].[Date Year Month].[Date].&[" + dateParameter + "],2))";
                    break;
                case 4://år
                    datePeriodString1 = "[Date].[Date Year Month].[Year]";
                    datePeriodString2 = "[Date].[Date Year Month]";
                    dateParameter = string.Format(CultureInfo.InvariantCulture, "{0:yyyy}",
                                                  new object[] { DateTime.Today });
                    datePeriodString3 = "[Date].[Date Year Month].[Year].&[" + dateParameter + "])";
                    break;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool all)
        {
            _olapConnection.Dispose();
        }
    }
}
