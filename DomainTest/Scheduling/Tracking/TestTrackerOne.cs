using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Tracking;


namespace Teleopti.Ccc.DomainTest.Scheduling.Tracking
{
    internal class TestTrackerOne : Tracker
    {

        public override Description Description
        {
            get { return new Description("Tracker One"); }
        }

        public override TimeSpan TrackForReset(IAbsence absence, IList<IScheduleDay> scheduleDays)
        {
            throw new NotImplementedException();
        }

        public override IAccount CreatePersonAccount(DateOnly dateTime)
        {
            throw new NotImplementedException();
        }

        protected override void PerformTracking(ITraceable target, IAbsence absence, IList<IScheduleDay> scheduleDays)
        {
            throw new NotImplementedException();
        }
    }
}