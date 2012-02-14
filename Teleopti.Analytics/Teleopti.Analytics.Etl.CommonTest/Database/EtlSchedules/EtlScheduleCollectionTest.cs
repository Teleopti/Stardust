using System.Diagnostics;
using NUnit.Framework;
using Teleopti.Analytics.Etl.CommonTest.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.DB.EtlScheduleTest
{
    //[TestFixture]
    //public class ScheduleCollectionTest
    //{
    //    #region Setup/Teardown

    //    [SetUp]
    //    public void Setup()
    //    {
    //        var repository = RepositoryFactory.GetScheduleRepository();

    //        var logCollection = new EtlLogCollection((ILogRepository)repository);

    //        etlScheduleCollection = new EtlScheduleCollection(repository, logCollection);
    //    }

    //    #endregion

    //    private EtlScheduleCollection etlScheduleCollection;

    //    [Test]
    //    public void VerifySchedule()
    //    {
    //        foreach (IEtlSchedule schedule in etlScheduleCollection)
    //        {
    //            string s = string.Format("{0} {1}", schedule.ScheduleId, schedule.TimeToRunNextJob);
    //            Trace.WriteLine(s);
    //        }
    //    }
    //}
}