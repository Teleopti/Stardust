using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
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
	    private IScheduleDictionary _scheduleDictionary;
	    private IScheduleRange _scheduleRange;
	    private IScheduleDay _scheduleDay;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_stateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
			_target = new FilterOvertimeAvailabilityPresenter(_stateHolder);
	        _scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            _scheduleRange = _mocks.StrictMock<IScheduleRange>();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
		}

		
		
        [Test]
        public void ShouldFilterTheAgentsWhenThatDateIsPassed()
        {
            var person = PersonFactory.CreatePerson("test1");
            IDictionary<Guid, IPerson> listOfPerson = new Dictionary<Guid, IPerson>();
            listOfPerson.Add(new Guid() ,person);
            var dateOnlyPeriod = new DateOnlyPeriod(DateOnly.Today,DateOnly.Today);

			var persistableCollection = new List<INonversionedPersistableScheduleData>();
            var overtimeAvailability = new OvertimeAvailability(person, DateOnly.Today, TimeSpan.FromHours(17), TimeSpan.FromHours(18));
            persistableCollection.Add(overtimeAvailability);
            using (_mocks.Record())
            {
                Expect.Call(_stateHolder.FilteredPersonDictionary).Return(listOfPerson);
                Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
                Expect.Call(_scheduleDictionary[person]).Return(_scheduleRange);
                Expect.Call(_scheduleRange.ScheduledDayCollection(dateOnlyPeriod)).Return(new List<IScheduleDay> { _scheduleDay });

                
                Expect.Call(_scheduleDay.PersistableScheduleDataCollection()).Return(persistableCollection);
                Expect.Call(() => _stateHolder.FilterPersonsOvertimeAvailability(new List<IPerson>())).IgnoreArguments();
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
                Expect.Call(_stateHolder.FilteredPersonDictionary).Return(new Dictionary<Guid, IPerson>());
                Expect.Call(() => _stateHolder.FilterPersonsOvertimeAvailability(new List<IPerson>())).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target.Filter(TimeSpan.FromHours(17), TimeSpan.FromHours(18), date);
            }
        }

       
	}
}
