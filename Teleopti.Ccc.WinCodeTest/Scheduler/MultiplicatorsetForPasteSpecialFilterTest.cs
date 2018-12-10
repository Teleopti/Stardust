using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class MultiplicatorsetForPasteSpecialFilterTest
	{
		private MultiplicatorsetForPasteSpecialFilter _target;
		private MockRepository _mocks;
		private IEnumerable<IScheduleDay> _scheduleDays;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IScheduleDay _scheduleDay3;
		private IPerson _person1;
		private IPerson _person2;
		private IPerson _person3;
		private DateOnly _dateOnly;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
		private IMultiplicatorDefinitionSet _multiplicatorDefinitionSet1;
		private IMultiplicatorDefinitionSet _multiplicatorDefinitionSet2;
		private IMultiplicatorDefinitionSet _multiplicatorDefinitionSet3;


		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();

			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay3 = _mocks.StrictMock<IScheduleDay>();
			_dateOnlyAsDateTimePeriod = _mocks.Stub<IDateOnlyAsDateTimePeriod>();
			_dateOnly = new DateOnly(2010, 03, 01);
			_multiplicatorDefinitionSet1 = new MultiplicatorDefinitionSet("Set1", MultiplicatorType.Overtime);
			_multiplicatorDefinitionSet2 = new MultiplicatorDefinitionSet("Set2", MultiplicatorType.OBTime);
			_multiplicatorDefinitionSet3 = new MultiplicatorDefinitionSet("Set3", MultiplicatorType.Overtime);
			_person1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			_person1.Period(DateOnly.MinValue).PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(_multiplicatorDefinitionSet1);
			_person1.Period(DateOnly.MinValue).PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(_multiplicatorDefinitionSet2);
			_person2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			_person2.Period(DateOnly.MinValue).PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(_multiplicatorDefinitionSet1);
			_person3 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue);
			_person3.Period(DateOnly.MinValue).PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(_multiplicatorDefinitionSet2);
			_person3.Period(DateOnly.MinValue).PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(_multiplicatorDefinitionSet3);
			
			_target = new MultiplicatorsetForPasteSpecialFilter();
		}

		[Test]
		public void ShouldHaveTheMultiplicatorWhichAllPeopleHave()
		{
			_scheduleDays = new List<IScheduleDay> { _scheduleDay1, _scheduleDay2};

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay1.Person).Return(_person1);
				Expect.Call(_scheduleDay2.Person).Return(_person2);
				Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
			}
			using (_mocks.Playback())
			{
				var result = _target.FilterAvailableMultiplicatorSet(_scheduleDays);
				Assert.AreEqual(1, result.ToList().Count);
			}


		}

		[Test]
		public void ShouldHaveEmptyMultiplicatorListIfNotAllPeopleHaveCommonMultiplicator()
		{
			_scheduleDays = new List<IScheduleDay> { _scheduleDay2, _scheduleDay3 };

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay2.Person).Return(_person2);
				Expect.Call(_scheduleDay3.Person).Return(_person3);
				Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_scheduleDay3.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
			}
			using (_mocks.Playback())
			{
				var result = _target.FilterAvailableMultiplicatorSet(_scheduleDays);
				Assert.AreEqual(0, result.ToList().Count);
			}

		}

		[Test]
		public void ShouldHaveEmptyMultiplicatorListIfNoPersonPeriod()
		{
			_person1.TerminatePerson(_dateOnly.AddDays(-1), new PersonAccountUpdaterDummy());
			_scheduleDays = new List<IScheduleDay> { _scheduleDay1 };

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay1.Person).Return(_person1);
				Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
			}
			using (_mocks.Playback())
			{
				var result = _target.FilterAvailableMultiplicatorSet(_scheduleDays);
				Assert.AreEqual(0, result.ToList().Count);
			}

		}

		[Test]
		public void ShouldFindMultiplicatorWhichIsNotDeleted()
		{
			_scheduleDays = new List<IScheduleDay> { _scheduleDay2 };
			((MultiplicatorDefinitionSet)_multiplicatorDefinitionSet1).SetDeleted();

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay2.Person).Return(_person2);
				Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
			}
			using (_mocks.Playback())
			{
				var result = _target.FilterAvailableMultiplicatorSet(_scheduleDays);
				Assert.AreEqual(0, result.ToList().Count);
			}

		}
	}
}
