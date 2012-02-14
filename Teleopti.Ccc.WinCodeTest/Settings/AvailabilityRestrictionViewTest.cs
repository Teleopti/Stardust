#region Imports

using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Interfaces.Domain;
using System.Globalization;
#endregion

namespace Teleopti.Ccc.WinCodeTest.Settings
{

    /// <summary>
    /// Represents a .
    /// </summary>
	[TestFixture]
    public class AvailabilityRestrictionViewTest
    {

        #region Fields - Instance Member

        private AvailabilityRestrictionView _targetView;
        //private ActivityNormalExtender _base;
        //private Activity _activity;
        //private TimePeriodWithSegment _activityLengthWithSegment;
        //private TimePeriodWithSegment _activityPositionWithSegment;
        //private WorkShiftRuleSet _workShiftRuleSet;
        //private IList<IActivity> _activities;

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - AvailabilityRestrictionViewTest Members

        #endregion

        #endregion

        #region Events - Instance Member

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - AvailabilityRestrictionViewTest Members

        IAvailabilityRestriction _containedEntity;
        private EndTimeLimitation _endTimeLimit = new EndTimeLimitation();
        private StartTimeLimitation _startTimeLimit = new StartTimeLimitation();
        private TimeSpan _minTimeWork = new TimeSpan(7, 0, 0);
        private TimeSpan _maxTimeWork = new TimeSpan(8, 30, 0);
        private WorkTimeLimitation _workTimePeriod;
        private bool _available = true;
        private readonly int _dayCount = 2;

        [SetUp]
        public void Init()
        {
            _workTimePeriod = new WorkTimeLimitation(_minTimeWork, _maxTimeWork);
            _containedEntity = new AvailabilityRestriction();
            _containedEntity.EndTimeLimitation = _endTimeLimit;
            _containedEntity.StartTimeLimitation = _startTimeLimit;
            _containedEntity.WorkTimeLimitation = _workTimePeriod;
            _containedEntity.NotAvailable = !_available;

            //int day = ScheduleRestrictionBaseView.GetDayOfWeek(_dayCount);
            //IList<DayOfWeek> daysOfWeek = DateHelper.GetDaysOfWeek(CultureInfo.CurrentUICulture);
            //DayOfWeek dayOfWeek = daysOfWeek[day];

            int weekNumber = ScheduleRestrictionBaseView.GetWeek(_dayCount);

            _targetView = new AvailabilityRestrictionView(_containedEntity, weekNumber, _dayCount);
        }

        [Test]
        public void VerifyPropertyValues()
        {
            Assert.IsNotNull(_targetView);

            Assert.AreEqual(_targetView.IsAvailable, _available);
            Assert.AreEqual(_targetView.EarlyStartTime, _startTimeLimit.StartTimeString);
            Assert.AreEqual(_targetView.LateEndTime, _endTimeLimit.EndTimeString);
            Assert.AreEqual(_targetView.MinimumWorkTime, _workTimePeriod.StartTimeString);
            Assert.AreEqual(_targetView.MaximumWorkTime, _workTimePeriod.EndTimeString);
            Assert.AreEqual(_targetView.Week, ScheduleRestrictionBaseView.GetWeek(_dayCount));

            //int day = ScheduleRestrictionBaseView.GetDayOfWeek(_dayCount);
            //IList<DayOfWeek> daysOfWeek = DateHelper.GetDaysOfWeek(CultureInfo.CurrentUICulture);
            //DayOfWeek dayOfWeek = daysOfWeek[day];

            string dayName = UserTexts.Resources.Day + " " + _dayCount.ToString(CultureInfo.CurrentUICulture);
            Assert.AreEqual(_targetView.Day, dayName);
        }

        [Test]
        public void VerifyAvailableChange()
        {
            Assert.IsNotNull(_targetView);

            Assert.AreEqual(_targetView.IsAvailable, _available);

            bool notAvailable = false;
            _targetView.IsAvailable = notAvailable;

            Assert.AreEqual(_targetView.IsAvailable, notAvailable);
            Assert.AreNotEqual(_targetView.IsAvailable, _available);

            _targetView.IsAvailable = _available;
            Assert.AreEqual(_targetView.IsAvailable, _available);
            Assert.AreNotEqual(_targetView.IsAvailable, notAvailable);
        }

    	[Test]
    	public void ShouldAssignValuesToDomainEntity()
    	{
    		var availabilityRestrictionView = new AvailabilityRestrictionView(_containedEntity, 4, 1)
    		                                  	{
    		                                  		EarlyStartTime = "08:00",
    		                                  		LateEndTime = "21:00",
    		                                  		MinimumWorkTime = "04:00",
    		                                  		MaximumWorkTime = "07:00",
    		                                  		IsAvailable = true
    		                                  	};

    		availabilityRestrictionView.AssignValuesToDomainObject();

			Assert.IsFalse(_containedEntity.NotAvailable);
			
			Assert.AreEqual(_containedEntity.StartTimeLimitation.StartTime, TimeSpan.FromHours(8));
			Assert.AreEqual(_containedEntity.EndTimeLimitation.EndTime, TimeSpan.FromHours(21));
			Assert.AreEqual(_containedEntity.WorkTimeLimitation.StartTime, TimeSpan.FromHours(4));
			Assert.AreEqual(_containedEntity.WorkTimeLimitation.EndTime, TimeSpan.FromHours(7));
    	}

        #endregion

        #endregion

    }

}
