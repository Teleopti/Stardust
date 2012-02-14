using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Database.EtlLogs;
using Teleopti.Analytics.Etl.CommonTest.FakeData;
using Teleopti.Analytics.Etl.Interfaces.Common;

namespace Teleopti.Analytics.Etl.CommonTest.Database.EtlLogs
{
    [TestFixture]
    public class EtlLogCollectionTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            ILogRepository logRepository = RepositoryFactory.GetLogRepository();
            _etlLogCollection = new EtlLogCollection(logRepository);
        }

        #endregion

        private IEtlLogCollection _etlLogCollection;

        [Test]
        public void VerifyLog()
        {
            foreach (IEtlLog log in _etlLogCollection)
            {
                object[] parameters = new object[3];
                parameters[0] = log.ScheduleId;
                parameters[1] = log.StartTime;
                parameters[2] = log.EndTime;
                //string s = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", log.ScheduleId, log.StartTime, log.EndTime);
                string s = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", parameters);
                Trace.WriteLine(s);
            }
        }
    }
}