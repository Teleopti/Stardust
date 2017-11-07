using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[DomainTest]
	public class SchedulerStateHolderLoadRequestsTest:ISetup
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public IFindSchedulesForPersons ScheduleStorage;
		public IRepositoryFactory RepositoryFactory;
		public IUnitOfWork UnitOfWork;
		public FakePersonAssignmentRepository PersonAssignmentRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeUnitOfWork>().For<IUnitOfWork>();
		}

		[Test]
		public void ShouldNotLoadOvertimeRequests()
		{
			var userTimeZone = TimeZoneInfoFactory.MountainTimeZoneInfo();
			var date = new DateOnly(2000,1,1);

			var personRequestRepository = RepositoryFactory.CreatePersonRequestRepository(UnitOfWork);
			var repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			repositoryFactory.Stub(s => s.CreatePersonRequestRepository(UnitOfWork)).Return(personRequestRepository);

			var personRequest = new PersonRequestFactory().CreatePersonRequest();
			var datetimePeriod = DateOnly.Today.ToDateTimePeriod(new TimePeriod(8, 9), TimeZoneInfoFactory.SingaporeTimeZoneInfo());
			var absenceRequest =
				new PersonRequestFactory().CreateAbsenceRequest(new Absence(), datetimePeriod);
			personRequest.Request = absenceRequest;
			personRequestRepository.Add(personRequest);

			var personRequestForOvertime = new PersonRequestFactory().CreatePersonRequest();
			var overtimeRequest =
				new OvertimeRequest(
					MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("test", MultiplicatorType.Overtime),
					datetimePeriod);
			personRequestForOvertime.Request = overtimeRequest;
			personRequestRepository.Add(personRequestForOvertime);

			var scenario = new Scenario("_")
			{
				DefaultScenario = true
			};
			var agent = new Person().WithId().InTimeZone(TimeZoneInfoFactory.SingaporeTimeZoneInfo());
			var assignment = new PersonAssignment(agent, scenario, date).WithId();
			assignment.AddActivity(new Activity("_"), new TimePeriod(1,0,2,0));
			PersonAssignmentRepository.Has(assignment);
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), new[] {agent}, new[] {assignment}, Enumerable.Empty<ISkillDay>(), userTimeZone);
			stateHolder.SetRequestedScenario(scenario);
			stateHolder.LoadPersonRequests(UnitOfWork, repositoryFactory,
				new PersonRequestAuthorizationCheckerConfigurable(), 14);

			stateHolder.PersonRequests.Count.Should().Be(1);
			(stateHolder.PersonRequests[0].Request is IAbsenceRequest).Should().Be(true);
		}
	}
}