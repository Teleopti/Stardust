using System.Collections.Generic;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
    public class LoadSchedulesForRequestWithoutResourceCalculationTest
	{
        private LoadSchedulesForRequestWithoutResourceCalculation _target;
		private MockRepository _mocks;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private IScheduleStorage _scheduleStorage;
		private IPersonProvider _personProvider;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingResultStateHolder = new SchedulingResultStateHolder();
			_personAbsenceAccountRepository = _mocks.DynamicMock<IPersonAbsenceAccountRepository>();
			_scheduleStorage = _mocks.DynamicMock<IScheduleStorage>();
			_personProvider = _mocks.DynamicMock<IPersonProvider>();
            _target = new LoadSchedulesForRequestWithoutResourceCalculation(_schedulingResultStateHolder, _personAbsenceAccountRepository, _scheduleStorage, p => _personProvider);
		}

		[Test]
		public void ShouldLoadPersonAccountsOnExecute()
		{
			var accounts = _mocks.Stub<IDictionary<IPerson, IPersonAccountCollection>>();

			Expect.Call(_personAbsenceAccountRepository.FindByUsers(null)).Return(accounts).IgnoreArguments();
			_schedulingResultStateHolder.AllPersonAccounts = accounts;

			_mocks.ReplayAll();

			_target.Execute(null, new DateTimePeriod(2010, 2, 1, 2010, 2, 2), new IPerson[]{});

			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyExecute()
		{
			DateTimePeriod period = new DateTimePeriod(2010,2,1,2010,2,2);
			IScenario scenario = _mocks.StrictMock<IScenario>();
			IPerson person = PersonFactory.CreatePerson();
			IScheduleDictionary scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			IPersonProvider personsInOrganizationProvider = _mocks.StrictMock<IPersonProvider>();
		    IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions = _mocks.StrictMock<IScheduleDictionaryLoadOptions>();

			var requestedPeople = new List<IPerson> {person};

			using (_mocks.Record())
			{
				Expect.Call(_scheduleStorage.FindSchedulesForPersons(null, scenario, personsInOrganizationProvider, scheduleDictionaryLoadOptions, null)).IgnoreArguments
					().Return(scheduleDictionary);
			}
			using (_mocks.Playback())
			{
				_target.Execute(scenario, period, requestedPeople);

			    _schedulingResultStateHolder.Schedules.Should().Not.Be.Null();
			    _schedulingResultStateHolder.SkillDays.Should().Not.Be.Null();
			}
		}
	}
}
