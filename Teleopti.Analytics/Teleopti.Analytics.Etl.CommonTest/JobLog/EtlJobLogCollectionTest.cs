using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.JobLog;
using Teleopti.Analytics.Etl.CommonTest.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.JobLog
{
    [TestFixture]
    public class EtlJobLogCollectionTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            IJobLogRepository jobLogRepository = RepositoryFactory.GetLogRepository();
            _etlJobLogCollection = new EtlJobLogCollection(jobLogRepository);
        }

        #endregion

        private IEtlJobLogCollection _etlJobLogCollection;

        [Test]
        public void VerifyLog()
        {
            foreach (IEtlJobLog log in _etlJobLogCollection)
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