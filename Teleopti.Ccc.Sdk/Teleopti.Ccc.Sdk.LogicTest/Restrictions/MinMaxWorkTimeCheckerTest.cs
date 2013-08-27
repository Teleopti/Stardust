using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.Restrictions
{
    [TestFixture]
    public class MinMaxWorkTimeCheckerTest

    {
        private MockRepository _mocks;
        private MinMaxWorkTimeChecker _target;
        private IRuleSetBag _ruleSetBag;
        private IScheduleDay _scheduleDay;
        private IWorkShiftWorkTime _workShiftWorkTime;
        private readonly IEffectiveRestriction _restriction = new EffectiveRestriction(new StartTimeLimitation(null, null),
            new EndTimeLimitation(null, null), new WorkTimeLimitation(null, null), null, null, null, new List<IActivityRestriction>());
        private IPerson _person;
    	private IVirtualSchedulePeriod _schedulePeriod;

    	[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
				_workShiftWorkTime = _mocks.StrictMock<IWorkShiftWorkTime>();
            _target = new MinMaxWorkTimeChecker(_workShiftWorkTime);

            _person = PersonFactory.CreatePersonWithBasicPermissionInfo("mycket", "hemligt");
            //_person.SetId(Guid.NewGuid());
            _person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
            _person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
        }
        
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfRuleSetProjectionServiceIsNull()
        {
            _target = new MinMaxWorkTimeChecker(null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfScheduleDayIsNull()
        {
            _target.MinMaxWorkTime(null, _ruleSetBag, _restriction);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfRuleSetBagIsNull()
        {
            _target.MinMaxWorkTime(_scheduleDay, null, _restriction);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void GetWorkTimeShouldThrowIfScheduleDayIsNull()
        {
            _scheduleDay = null;
            MinMaxWorkTimeChecker.GetWorkTime(_scheduleDay);
        }

        [Test]
        public void ShouldReturnEmptyIfDayOff()
        {
            Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff);
            _mocks.ReplayAll();

            var result = _target.MinMaxWorkTime(_scheduleDay, _ruleSetBag, _restriction);
            Assert.That(result.EndTimeLimitation.StartTime, Is.Null);
            Assert.That(result.EndTimeLimitation.EndTime, Is.Null);
            Assert.That(result.StartTimeLimitation.StartTime, Is.Null);
            Assert.That(result.StartTimeLimitation.EndTime, Is.Null);
            Assert.That(result.WorkTimeLimitation.StartTime, Is.Null);
            Assert.That(result.WorkTimeLimitation.EndTime, Is.Null);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGetMinMaxWorkTimeFromScheduleIfScheduled()
        {
            var dateTime = new DateTime(2010, 12, 16, 8, 0, 0, DateTimeKind.Utc);
            var timeZone = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
            var projectionService = _mocks.StrictMock<IProjectionService>();
            var projection = _mocks.StrictMock<IVisualLayerCollection>();

            Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
            Expect.Call(_scheduleDay.TimeZone).Return(timeZone);
            Expect.Call(_scheduleDay.ProjectionService()).Return(projectionService);
            Expect.Call(projectionService.CreateProjection()).Return(projection);
            Expect.Call(projection.ContractTime()).Return(TimeSpan.FromHours(8));
            Expect.Call(projection.Period()).Return(new DateTimePeriod(dateTime, dateTime.AddHours(9)));
            _mocks.ReplayAll();
            var result = _target.MinMaxWorkTime(_scheduleDay, _ruleSetBag, _restriction);
            Assert.That(result, Is.Not.Null);
            _mocks.VerifyAll();
        }

		[Test]
		public void ShouldGetMinMaxWorkTimeFromAverageWorkTimeOnNotAvailableDay()
		{
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			
			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call(effectiveRestriction.NotAvailable).Return(true);
			}

			using (_mocks.Playback())
			{
				var result = _target.MinMaxWorkTime(_scheduleDay, _ruleSetBag, effectiveRestriction);
				Assert.That(result.WorkTimeLimitation.StartTime, Is.Null);
			}
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldGetMinMaxWorkTimeFromAverageWorkTimeOnAbsencePreferenceOnWorkdays()
        {
            var dateOnlyAsPeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            var absence = new Absence { InContractTime = true };
            var personPeriod = _mocks.StrictMock<IPersonPeriod>();
            var dateOnly = new DateOnly(2011, 1, 1);
            var personContract = _mocks.StrictMock<IPersonContract>();
            var person = _mocks.StrictMock<IPerson>();
            var contractSchedule = _mocks.StrictMock<IContractSchedule>();

            using (_mocks.Record())
            {
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call(effectiveRestriction.Absence).Return(absence).Repeat.Twice();
				Expect.Call(effectiveRestriction.NotAvailable).Return(false);
                Expect.Call(_scheduleDay.Person).Return(person);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsPeriod);
                Expect.Call(dateOnlyAsPeriod.DateOnly).Return(dateOnly);
                Expect.Call(person.Period(dateOnly)).IgnoreArguments().Return(personPeriod);
                Expect.Call(personPeriod.PersonContract).Return(personContract);
                Expect.Call(personContract.ContractSchedule).Return(contractSchedule);
                Expect.Call(contractSchedule.IsWorkday(dateOnly, dateOnly)).IgnoreArguments().Return(true);
                Expect.Call(personContract.PartTimePercentage).Return(new PartTimePercentage("Hej"));
                Expect.Call(person.AverageWorkTimeOfDay(new DateOnly())).IgnoreArguments().Return(new TimeSpan(0, 8, 0, 0));
				Expect.Call(person.VirtualSchedulePeriod(new DateOnly())).IgnoreArguments().Return(_schedulePeriod);
            	Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod());
            }

            using(_mocks.Playback())
            {
                var result = _target.MinMaxWorkTime(_scheduleDay, _ruleSetBag, effectiveRestriction);
                Assert.That(result.WorkTimeLimitation.StartTime, Is.EqualTo(TimeSpan.FromHours(8)));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldGetMinMaxWorkTimeZeroOnAbsenceNotInContractTime()
        {
            var dateOnlyAsPeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            var absence = new Absence {InContractTime = false};
            var personPeriod = _mocks.StrictMock<IPersonPeriod>();
            var dateOnly = new DateOnly(2011, 1, 1);
            var personContract = _mocks.StrictMock<IPersonContract>();
            var person = _mocks.StrictMock<IPerson>();
            var contractSchedule = _mocks.StrictMock<IContractSchedule>();

            using (_mocks.Record())
            {
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call(effectiveRestriction.Absence).Return(absence).Repeat.Twice();
				Expect.Call(effectiveRestriction.NotAvailable).Return(false);
                Expect.Call(_scheduleDay.Person).Return(person);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsPeriod);
                Expect.Call(dateOnlyAsPeriod.DateOnly).Return(dateOnly);
                Expect.Call(person.Period(dateOnly)).IgnoreArguments().Return(personPeriod);
                Expect.Call(personPeriod.PersonContract).Return(personContract);
                Expect.Call(personContract.ContractSchedule).Return(contractSchedule);
                Expect.Call(contractSchedule.IsWorkday(dateOnly, dateOnly)).IgnoreArguments().Return(true);
                Expect.Call(personContract.PartTimePercentage).Return(new PartTimePercentage("Hej"));
                Expect.Call(person.AverageWorkTimeOfDay(new DateOnly())).IgnoreArguments().Return(new TimeSpan(0, 8, 0, 0));
				Expect.Call(person.VirtualSchedulePeriod(new DateOnly())).IgnoreArguments().Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod());
            }

            using (_mocks.Playback())
            {
                var result = _target.MinMaxWorkTime(_scheduleDay, _ruleSetBag, effectiveRestriction);
                Assert.That(result.WorkTimeLimitation.StartTime, Is.EqualTo(TimeSpan.Zero));
            }
        }

        [Test]
        public void ShouldGetMinMaxWorkTimeZeroOnAbsencePreferenceOnNonWorkdays()
        {
            var dateOnlyAsPeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            var absence = new Absence();
            var personPeriod = _mocks.StrictMock<IPersonPeriod>();
            var dateOnly = new DateOnly(2011, 1, 1);
            var personContract = _mocks.StrictMock<IPersonContract>();
            var person = _mocks.StrictMock<IPerson>();
            var contractSchedule = _mocks.StrictMock<IContractSchedule>();

            using (_mocks.Record())
            {
                Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);
                Expect.Call(effectiveRestriction.Absence).Return(absence);
                Expect.Call(effectiveRestriction.NotAvailable).Return(false);
                Expect.Call(_scheduleDay.Person).Return(person);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsPeriod);
                Expect.Call(dateOnlyAsPeriod.DateOnly).Return(dateOnly);
                Expect.Call(person.Period(dateOnly)).IgnoreArguments().Return(personPeriod);
                Expect.Call(personPeriod.PersonContract).Return(personContract);
                Expect.Call(personContract.ContractSchedule).Return(contractSchedule);
                Expect.Call(contractSchedule.IsWorkday(dateOnly, dateOnly)).IgnoreArguments().Return(false);
                Expect.Call(personContract.PartTimePercentage).Return(new PartTimePercentage("Hej"));
                Expect.Call(person.AverageWorkTimeOfDay(new DateOnly())).IgnoreArguments().Return(new TimeSpan(0, 8, 0, 0));
				Expect.Call(person.VirtualSchedulePeriod(new DateOnly())).IgnoreArguments().Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod());
            }

            using (_mocks.Playback())
            {
                var result = _target.MinMaxWorkTime(_scheduleDay, _ruleSetBag, effectiveRestriction);
                Assert.That(result.WorkTimeLimitation.StartTime, Is.EqualTo(TimeSpan.Zero));
            }    
        }

        [Test]
        public void ShouldGetMinMaxWorkTimeFromRuleSetBagIfNoSchedule()
        {
            var onDate = new DateOnly(2010, 12, 16);
            var dateOnlyAsPeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            
            Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);
            Expect.Call(_scheduleDay.PersonMeetingCollection()).Return(
                new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>()));
            Expect.Call(_scheduleDay.PersonAssignment()).Return(null);
            Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsPeriod);
            Expect.Call(dateOnlyAsPeriod.DateOnly).Return(onDate);
            Expect.Call(_ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, onDate, _restriction)).Return(new WorkTimeMinMax());
            _mocks.ReplayAll();

            var result = _target.MinMaxWorkTime(_scheduleDay, _ruleSetBag, _restriction);
            Assert.That(result, Is.Not.Null);
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldConsiderPartTimePercentageIfRestrictionIsAbsence()
        {
            var onDate = new DateOnly(2010, 12, 16);
            var dateOnlyAsPeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                                                  new EndTimeLimitation(),
                                                                                  new WorkTimeLimitation(), null, null, null,
                                                                                  new List<IActivityRestriction>());
            effectiveRestriction.Absence = AbsenceFactory.CreateRequestableAbsence("hej", "hh", Color.Black);
            effectiveRestriction.Absence.InContractTime = true;
            var person = _mocks.StrictMock<IPerson>();
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly());
        	
            IPersonContract personContract = PersonContractFactory.CreatePersonContract();
            IContract contract = ContractFactory.CreateContract("Hej");
            contract.WorkTime= new WorkTime(TimeSpan.FromHours(8));
            IPartTimePercentage partTimePercentage = new PartTimePercentage("Hej");
            partTimePercentage.Percentage = new Percent(0.5);
            personContract.Contract = contract;
            personContract.PartTimePercentage = partTimePercentage;
            personPeriod.PersonContract = personContract;
            using(_mocks.Record())
            {
                Expect.Call(_scheduleDay.Person).Return(person);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsPeriod);
                Expect.Call(dateOnlyAsPeriod.DateOnly).Return(onDate);
                Expect.Call(person.Period(new DateOnly())).IgnoreArguments().Return(personPeriod);
                Expect.Call(person.AverageWorkTimeOfDay(new DateOnly())).IgnoreArguments().Return(new TimeSpan(0,8,0,0) );
				Expect.Call(person.VirtualSchedulePeriod(new DateOnly())).IgnoreArguments().Return(_schedulePeriod);
            	Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod());
            }

            IWorkTimeMinMax result;
            using(_mocks.Playback())
            {
                result = MinMaxWorkTimeChecker.GetWorkTimeAbsencePreference(_scheduleDay, effectiveRestriction);
            }

            Assert.AreEqual(TimeSpan.FromHours(4), result.WorkTimeLimitation.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(4), result.WorkTimeLimitation.EndTime);
        }
    }

    
}