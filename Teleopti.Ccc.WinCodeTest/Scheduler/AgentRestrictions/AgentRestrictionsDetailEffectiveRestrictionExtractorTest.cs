using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsDetailEffectiveRestrictionExtractorTest
	{
		private AgentRestrictionsDetailEffectiveRestrictionExtractor _effectiveRestrictionExtractor;
		private MockRepository _mocks;
		private PreferenceCellData _preferenceCellData;
		private IWorkShiftWorkTime _workShiftWorkTime;
		private IEffectiveRestriction _effectiveRestriction;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IRestrictionExtractor _restrictionExtractor;
		private RestrictionSchedulingOptions _schedulingOptions;
		private IScheduleDayPro _scheduleDayPro;
		private IVirtualSchedulePeriod _virtualSchedulePeriod;

		private DateTime _dateTime;
		private IScheduleDictionary _scheduleDictionary;
		private IScenario _scenario;
		private IScheduleDateTimePeriod _scheduleDateTimePeriod;
		private DateTimePeriod _dateTimePeriod;
		private IDictionary<IPerson, IScheduleRange> _dictionary;
		private IPerson _person;
		private IScheduleRange _range;
		private TimeSpan _periodTarget;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingOptions = new RestrictionSchedulingOptions(){UseScheduling = true};
			_virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			_restrictionExtractor = _mocks.StrictMock<IRestrictionExtractor>();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_workShiftWorkTime = _mocks.StrictMock<IWorkShiftWorkTime>();
			_effectiveRestrictionExtractor = new AgentRestrictionsDetailEffectiveRestrictionExtractor(_workShiftWorkTime, _restrictionExtractor, _schedulingOptions);
			_preferenceCellData = new PreferenceCellData();
			_effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();

			_range = _mocks.StrictMock<IScheduleRange>();
			_person = PersonFactory.CreatePerson("Jens");
			_dictionary = new Dictionary<IPerson, IScheduleRange> { { _person, _range } };
			_dateTime = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			_dateTimePeriod = new DateTimePeriod(_dateTime, _dateTime.AddDays(30));
			_scenario = ScenarioFactory.CreateScenarioAggregate();
			_scheduleDateTimePeriod = new ScheduleDateTimePeriod(_dateTimePeriod);
			_scheduleDictionary = new ScheduleDictionaryForTest(_scenario, _scheduleDateTimePeriod, _dictionary);
			_periodTarget = TimeSpan.FromHours(40);
			
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldExtractMainShift()
		{
			var dateOnly = new DateOnly(_dateTime);
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(30));
			var mainShift = MainShiftFactory.CreateMainShiftWithThreeActivityLayers();
			var part = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(_dateTime));
			part.AddMainShift(mainShift);
			var period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(_dateTime.AddDays(-10)));
			_person.AddPersonPeriod(period);

			using(_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(dateOnly)).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(part);
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(() => _restrictionExtractor.Extract(_person, dateOnly));
				Expect.Call(_restrictionExtractor.CombinedRestriction(_schedulingOptions)).Return(_effectiveRestriction);
				Expect.Call(_restrictionExtractor.PreferenceList).Return(new List<IPreferenceRestriction>());
			}

			using(_mocks.Playback())
			{
				_effectiveRestrictionExtractor.Extract(_scheduleMatrixPro, _preferenceCellData, dateOnly, _dateTimePeriod, _periodTarget);	
			}

			Assert.IsTrue(_preferenceCellData.HasShift);
			Assert.AreEqual(TimeSpan.FromHours(8), _preferenceCellData.EffectiveRestriction.StartTimeLimitation.StartTime);
			Assert.AreEqual(TimeSpan.FromHours(26), _preferenceCellData.EffectiveRestriction.EndTimeLimitation.EndTime);
			Assert.AreEqual(TimeSpan.FromHours(18), _preferenceCellData.EffectiveRestriction.WorkTimeLimitation.StartTime);

			Assert.AreEqual(dateOnly, _preferenceCellData.TheDate);
			Assert.AreEqual(part, _preferenceCellData.SchedulePart);
			Assert.IsTrue(_preferenceCellData.Enabled);
			Assert.AreEqual(_periodTarget, _preferenceCellData.PeriodTarget);
			Assert.AreEqual(_schedulingOptions, _preferenceCellData.SchedulingOption);
			Assert.IsFalse(_preferenceCellData.MustHavePreference);
			Assert.AreEqual(period.PersonContract.Contract.WorkTimeDirective.NightlyRest, _preferenceCellData.NightlyRest);
			Assert.AreEqual(period.PersonContract.Contract.WorkTimeDirective.MaxTimePerWeek, _preferenceCellData.WeeklyMax);
		}
	}
}
