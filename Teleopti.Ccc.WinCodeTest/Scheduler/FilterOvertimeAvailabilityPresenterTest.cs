using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class FilterOvertimeAvailabilityPresenterTest
	{
		private MockRepository _mocks;
		private ISchedulerStateHolder _stateHolder;
		private FilterOvertimeAvailabilityPresenter _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_stateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
			_target = new FilterOvertimeAvailabilityPresenter(_stateHolder);
		}

		[Test]
		public void ShouldInitialize()
		{
			var personDict = new Dictionary<Guid, IPerson>();
			var person = PersonFactory.CreatePerson("bill");
			personDict.Add(Guid.NewGuid(), person);
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2013, 6, 24), new DateOnly(2013, 6, 24));
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var scheduleRange = _mocks.StrictMock<IScheduleRange>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.FilteredPersonDictionary).Return(personDict);
				Expect.Call(_stateHolder.RequestedPeriod)
					  .Return(new DateOnlyPeriodAsDateTimePeriod(dateOnlyPeriod, TimeZoneInfo.FindSystemTimeZoneById("UTC")));
				Expect.Call(_stateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(scheduleDictionary[person]).Return(scheduleRange);
				Expect.Call(scheduleRange.ScheduledDayCollection(dateOnlyPeriod)).Return(new List<IScheduleDay> { scheduleDay});
			}
			using (_mocks.Playback())
			{
				_target.Initialize();
			}
		}

		[Test]
		public void ShouldFilterAccordingToLimitations()
		{
			using (_mocks.Record())
			{
				Expect.Call(()=>_stateHolder.FilterPersonsOvertimeAvailability(new List<IPerson>())).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				_target.Filter(TimeSpan.FromHours(17), TimeSpan.FromHours(18));
			}
		}
	}
}
