using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class FilterHourlyAvailabilityPresenterTest
	{
		private MockRepository _mocks;
		private ISchedulerStateHolder _stateHolder;
		private FilterHourlyAvailabilityPresenter _target;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _scheduleRange;
		private IScheduleDay _scheduleDay;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_stateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
			_target = new FilterHourlyAvailabilityPresenter(new SchedulingScreenState(null, _stateHolder));
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_scheduleRange = _mocks.StrictMock<IScheduleRange>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
		}



		[Test]
		public void ShouldFilterTheAgentsWhenThatDateIsPassed()
		{
			var person = PersonFactory.CreatePerson("test1");
			IDictionary<Guid, IPerson> listOfPerson = new Dictionary<Guid, IPerson>();
			listOfPerson.Add(Guid.Empty, person);
			var dateOnlyPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);

			var persistableCollection = new List<IPersistableScheduleData>();


			var studentAvailabilityRestriction1 = new StudentAvailabilityRestriction();
			studentAvailabilityRestriction1.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(17), null);
			studentAvailabilityRestriction1.EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(18));

			IPersistableScheduleData studentAvailabilityDay1 = new StudentAvailabilityDay(person, DateOnly.Today, new List<IStudentAvailabilityRestriction> { studentAvailabilityRestriction1 });
			
			persistableCollection.Add(studentAvailabilityDay1);
			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.FilteredCombinedAgentsDictionary).Return(listOfPerson);
				Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDayCollection(dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay });


				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(persistableCollection);
				Expect.Call(() => _stateHolder.FilterPersonsHourlyAvailability(new List<IPerson>())).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				_target.Filter(TimeSpan.FromHours(17), TimeSpan.FromHours(18), DateOnly.Today);
			}
		}

		[Test]
		public void ShouldFilterTheAgentsWhenAnotherDateIsPassed()
		{
			var date = DateOnly.Today.AddDays(1);
			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.FilteredCombinedAgentsDictionary).Return(new Dictionary<Guid, IPerson>());
				Expect.Call(() => _stateHolder.FilterPersonsHourlyAvailability(new List<IPerson>())).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				_target.Filter(TimeSpan.FromHours(17), TimeSpan.FromHours(18), date);
			}
		}
	}
}
