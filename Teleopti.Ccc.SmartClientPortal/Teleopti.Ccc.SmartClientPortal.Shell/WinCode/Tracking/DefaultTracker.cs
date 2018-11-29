using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Tracking;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Tracking
{
    public class DefaultTracker : Tracker
    {
        private readonly Description _description;

        public override Description Description
        {
            get { return _description; }
        }

        internal DefaultTracker()
        {
            _description = new Description(UserTexts.Resources.DefaultTrackerDefaultDescription);
        }

        public override TimeSpan TrackForReset(IAbsence absence, IList<IScheduleDay> scheduleDays)
        {
            return TimeSpan.Zero;
        }

        public override IAccount CreatePersonAccount(DateOnly dateTime)
        {
            throw new NotImplementedException();
        }

        protected override void PerformTracking(ITraceable target, IAbsence absence, IList<IScheduleDay> scheduleDays)
        {
            target.Track(Calculator.CalculateNumberOfDaysOnScheduleDays(absence, scheduleDays));
        }

    }

}
