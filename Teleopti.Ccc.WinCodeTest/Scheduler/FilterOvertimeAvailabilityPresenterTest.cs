using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class FilterOvertimeAvailabilityPresenterTest
	{
		private MockRepository _mocks;
		private ISchedulerStateHolder _stateHolder;
		private FilterOvertimeAvailabilityPresenter _target;
	    private IScheduleDictionary _scheduleDictionary;
	    private IScheduleRange _scheduleRange;
	    private IScheduleDay _scheduleDay;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_stateHolder = _mocks.Stub<ISchedulerStateHolder>();
			_target = new FilterOvertimeAvailabilityPresenter(new SchedulingScreenState(null, _stateHolder));
	        _scheduleDictionary = _mocks.Stub<IScheduleDictionary>();
            _scheduleRange = _mocks.Stub<IScheduleRange>();
            _scheduleDay = _mocks.Stub<IScheduleDay>();
		}

        [Test]
        public void ShouldFilterTheAgentsWhenThatDateIsPassed()
        {
            var person = PersonFactory.CreatePerson("test1");
            IDictionary<Guid, IPerson> listOfPerson = new Dictionary<Guid, IPerson>();
            listOfPerson.Add(Guid.Empty ,person);
            var dateOnlyPeriod = new DateOnlyPeriod(DateOnly.Today,DateOnly.Today);

			var persistableCollection = new List<IPersistableScheduleData>();
            var overtimeAvailability = new OvertimeAvailability(person, DateOnly.Today, TimeSpan.FromHours(17), TimeSpan.FromHours(18));
            persistableCollection.Add(overtimeAvailability);
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.FilteredCombinedAgentsDictionary).Return(listOfPerson);
                Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
                _scheduleDictionary[person] = _scheduleRange;
                Expect.Call(_scheduleRange.ScheduledDayCollection(dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay });
				Expect.Call(_scheduleDay.Person).Return(person);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
				Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(persistableCollection);
                Expect.Call(() => _stateHolder.FilterPersonsOvertimeAvailability(new List<IPerson>())).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target.Filter(new TimePeriod(TimeSpan.FromHours(17), TimeSpan.FromHours(18)), DateOnly.Today, false);
            }
        }

        [Test]
        public void ShouldFilterTheAgentsWhenAnotherDateIsPassed()
        {
            var date = DateOnly.Today.AddDays(1);
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.FilteredCombinedAgentsDictionary).Return(new Dictionary<Guid, IPerson>());
                Expect.Call(() => _stateHolder.FilterPersonsOvertimeAvailability(new List<IPerson>())).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target.Filter(new TimePeriod(TimeSpan.FromHours(17), TimeSpan.FromHours(18)), date, false);
            }
        }

       
	}
}
