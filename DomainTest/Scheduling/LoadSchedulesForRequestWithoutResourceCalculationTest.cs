using System.Collections.Generic;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


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

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingResultStateHolder = new SchedulingResultStateHolder();
			_personAbsenceAccountRepository = _mocks.DynamicMock<IPersonAbsenceAccountRepository>();
			_scheduleStorage = _mocks.DynamicMock<IScheduleStorage>();
            _target = new LoadSchedulesForRequestWithoutResourceCalculation( _personAbsenceAccountRepository, _scheduleStorage);
		}

		[Test]
		public void ShouldLoadPersonAccountsOnExecute()
		{
			var accounts = _mocks.Stub<IDictionary<IPerson, IPersonAccountCollection>>();

			Expect.Call(_personAbsenceAccountRepository.FindByUsers(null)).Return(accounts).IgnoreArguments();
			_schedulingResultStateHolder.AllPersonAccounts = accounts;

			_mocks.ReplayAll();

			_target.Execute(null, new DateTimePeriod(2010, 2, 1, 2010, 2, 2), new IPerson[]{}, _schedulingResultStateHolder);

			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyExecute()
		{
			DateTimePeriod period = new DateTimePeriod(2010,2,1,2010,2,2);
			IScenario scenario = _mocks.StrictMock<IScenario>();
			IPerson person = PersonFactory.CreatePerson();
			IScheduleDictionary scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
		    var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(false,false);

			var requestedPeople = new List<IPerson> {person};

			using (_mocks.Record())
			{
				Expect.Call(_scheduleStorage.FindSchedulesForPersons(scenario, _schedulingResultStateHolder.LoadedAgents, scheduleDictionaryLoadOptions, new DateTimePeriod(), null, false)).IgnoreArguments
					().Return(scheduleDictionary);
			}
			using (_mocks.Playback())
			{
				_target.Execute(scenario, period, requestedPeople, _schedulingResultStateHolder);

			    _schedulingResultStateHolder.Schedules.Should().Not.Be.Null();
			    _schedulingResultStateHolder.SkillDays.Should().Not.Be.Null();
			}
		}
	}
}
