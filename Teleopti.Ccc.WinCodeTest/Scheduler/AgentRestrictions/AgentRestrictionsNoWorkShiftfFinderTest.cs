using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.AgentRestrictions
{
	[TestFixture]
	public class AgentRestrictionsNoWorkShiftfFinderTest
	{
		private MockRepository _mock;
		private IRestrictionExtractor _restrictionExtractor;
		private IScheduleDay _scheduleDay;
		private SchedulingOptions _schedulingOptions;
		private IEffectiveRestriction _effectiveRestriction;
		private AgentRestrictionsNoWorkShiftfFinder _finder;
		private IPerson _person;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
		private DateOnly _dateOnly;
		private IPersonPeriod _personPeriod;
		private IRuleSetBag _ruleSetBag;
		private IWorkShiftWorkTime _workShiftWorkTime;
		private IExtractedRestrictionResult _extractedRestrictionResult;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_restrictionExtractor = _mock.StrictMock<IRestrictionExtractor>();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_schedulingOptions = new SchedulingOptions();
			_effectiveRestriction = _mock.StrictMock<IEffectiveRestriction>();
			_person = _mock.StrictMock<IPerson>();
			_dateOnlyAsDateTimePeriod = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
			_dateOnly = new DateOnly(2013, 1, 1);
			_personPeriod = _mock.StrictMock<IPersonPeriod>();
			_ruleSetBag = _mock.StrictMock<IRuleSetBag>();
			_workShiftWorkTime = _mock.StrictMock<IWorkShiftWorkTime>();
			_extractedRestrictionResult = _mock.StrictMock<IExtractedRestrictionResult>();
			_finder = new AgentRestrictionsNoWorkShiftfFinder(_restrictionExtractor, _workShiftWorkTime);	
		}

		[Test]
		public void ShouldFind()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.None);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.RuleSetBag).Return(_ruleSetBag);
				Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
				Expect.Call(_extractedRestrictionResult.CombinedRestriction(_schedulingOptions)).Return(_effectiveRestriction);
				Expect.Call(_ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, _dateOnly, _effectiveRestriction)).Return(null);
				Expect.Call(_effectiveRestriction.IsRestriction).Return(true);
				Expect.Call(_effectiveRestriction.DayOffTemplate).Return(null);
				Expect.Call(_effectiveRestriction.Absence).Return(null);
			}

			using (_mock.Playback())
			{
				var result = _finder.Find(_scheduleDay, _schedulingOptions);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldFindWhenNoPersonPeriod()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.None);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
				Expect.Call(_person.Period(_dateOnly)).Return(null);
			}

			using (_mock.Playback())
			{
				var result = _finder.Find(_scheduleDay, _schedulingOptions);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldFindWhenNoEffectiveRestriction()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.None);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.RuleSetBag).Return(_ruleSetBag);
				Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
				Expect.Call(_extractedRestrictionResult.CombinedRestriction(_schedulingOptions)).Return(null);
			}

			using (_mock.Playback())
			{
				var result = _finder.Find(_scheduleDay, _schedulingOptions);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldFindWhenNoShiftBag()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.None);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.RuleSetBag).Return(null);
			}

			using (_mock.Playback())
			{
				var result = _finder.Find(_scheduleDay, _schedulingOptions);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldNotFind()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.None);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.RuleSetBag).Return(_ruleSetBag);
				Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
				Expect.Call(_extractedRestrictionResult.CombinedRestriction(_schedulingOptions)).Return(_effectiveRestriction);
				Expect.Call(_ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, _dateOnly, _effectiveRestriction)).Return(new WorkTimeMinMax());
			}

			using (_mock.Playback())
			{
				var result = _finder.Find(_scheduleDay, _schedulingOptions);
				Assert.IsFalse(result);
			}

		}

		[Test]
		public void ShouldNotFindWhenIsNoRestriction()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.None);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.RuleSetBag).Return(_ruleSetBag);
				Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
				Expect.Call(_extractedRestrictionResult.CombinedRestriction(_schedulingOptions)).Return(_effectiveRestriction);
				Expect.Call(_ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, _dateOnly, _effectiveRestriction)).Return(null);
				Expect.Call(_effectiveRestriction.IsRestriction).Return(false);
			}

			using (_mock.Playback())
			{
				var result = _finder.Find(_scheduleDay, _schedulingOptions);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldNotFindWhenDayOffTemplate()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.None);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.RuleSetBag).Return(_ruleSetBag);
				Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
				Expect.Call(_extractedRestrictionResult.CombinedRestriction(_schedulingOptions)).Return(_effectiveRestriction);
				Expect.Call(_ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, _dateOnly, _effectiveRestriction)).Return(null);
				Expect.Call(_effectiveRestriction.IsRestriction).Return(true);
				Expect.Call(_effectiveRestriction.DayOffTemplate).Return(new DayOffTemplate(new Description("dayOffTemplate")));
			}

			using (_mock.Playback())
			{
				var result = _finder.Find(_scheduleDay, _schedulingOptions);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldNotFindWhenAbsence()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.None);
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.RuleSetBag).Return(_ruleSetBag);
				Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
				Expect.Call(_extractedRestrictionResult.CombinedRestriction(_schedulingOptions)).Return(_effectiveRestriction);
				Expect.Call(_ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, _dateOnly, _effectiveRestriction)).Return(null);
				Expect.Call(_effectiveRestriction.IsRestriction).Return(true);
				Expect.Call(_effectiveRestriction.DayOffTemplate).Return(null);
				Expect.Call(_effectiveRestriction.Absence).Return(new Absence());
			}

			using (_mock.Playback())
			{
				var result = _finder.Find(_scheduleDay, _schedulingOptions);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldNotFindWhenScheduled()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
			}

			using (_mock.Playback())
			{
				var result = _finder.Find(_scheduleDay, _schedulingOptions);
				Assert.IsFalse(result);
			}
		}
	}
}
