using Teleopti.Analytics.Etl.CommonTest.Infrastructure;
using Teleopti.Analytics.Etl.Interfaces.Common;


namespace Teleopti.Analytics.Etl.CommonTest.FakeData
{
    sealed class RepositoryFactory
    {
        private RepositoryFactory()
        {
            
        }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        //public static IScheduleRepository GetScheduleRepository()
        //{
        //    var rep = new RepositoryStub();

        //    rep.AddSchedule(1, 25);
        //    rep.AddSchedule(1, 35);
        //    rep.AddSchedule(1, 10, 0, 0);


        //    rep.AddLog(1, 10);
        //    rep.AddLog(2, 20);
        //    rep.AddLog(3, 30);

        //    return rep;
        //}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static ILogRepository GetLogRepository()
        {
            var rep = new RepositoryStub();

            rep.AddSchedule(1, 25);
            rep.AddSchedule(1, 35);
            rep.AddSchedule(1, 10, 0, 0);


            rep.AddLog(1, 10);
            rep.AddLog(2, 20);
            rep.AddLog(3, 30);

            return rep;
        }

    }
}