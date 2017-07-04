using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class QueuedOvertimeRequestRepositoryTest : RepositoryTest<IQueuedOvertimeRequest>
	{
		private IPerson _person;
		private IMultiplicatorDefinitionSet _overtimeType;
		private IScenario _defaultScenario;
		private IPersonRequest _personRequest;
		private IOvertimeRequest _overtimeRequest;
		protected override void ConcreteSetup()
		{
			_defaultScenario = ScenarioFactory.CreateScenarioAggregate("Default", true);
			_person = PersonFactory.CreatePerson("sdfoj");
			_overtimeType = new MultiplicatorDefinitionSet("Overtime time", MultiplicatorType.Overtime);

			var period = new DateTimePeriod(
				new DateTime(2008, 7, 10, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2008, 7, 11, 0, 0, 0, DateTimeKind.Utc));
			_personRequest = new PersonRequest(_person);
			_overtimeRequest = new OvertimeRequest(_overtimeType ?? _overtimeType, period);

			_personRequest.Request = _overtimeRequest;
			_personRequest.Pending();
			
			PersistAndRemoveFromUnitOfWork(_defaultScenario);
			PersistAndRemoveFromUnitOfWork(_person);
			PersistAndRemoveFromUnitOfWork(_overtimeType);
			PersistAndRemoveFromUnitOfWork(_personRequest);
		}

		protected override IQueuedOvertimeRequest CreateAggregateWithCorrectBusinessUnit()
		{
			return createOvertimeRequestAndBusinessUnit();
		}

		private IQueuedOvertimeRequest createOvertimeRequestAndBusinessUnit()
		{
			return new QueuedOvertimeRequest
			{
				PersonRequest = _personRequest.Id.GetValueOrDefault(),
				StartDateTime = _overtimeRequest.Period.StartDateTime,
				EndDateTime = _overtimeRequest.Period.EndDateTime,
				Created = _personRequest.CreatedOn.GetValueOrDefault()

			};
		}
		
		protected override void VerifyAggregateGraphProperties(IQueuedOvertimeRequest loadedAggregateFromDatabase)
		{
			var org = CreateAggregateWithCorrectBusinessUnit();
			org.PersonRequest.Should().Be.EqualTo(loadedAggregateFromDatabase.PersonRequest);
			org.StartDateTime.Should().Be.EqualTo(loadedAggregateFromDatabase.StartDateTime);
		}

		protected override Repository<IQueuedOvertimeRequest> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new QueuedOvertimeRequestRepository(currentUnitOfWork);
		}
	}
}
