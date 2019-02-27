using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class PersonRequestRepositoryOvertimeTest : RepositoryTest<IPersonRequest>
	{
		private IPerson _person;
		private MultiplicatorDefinitionSet _multiplicator;

		protected override void ConcreteSetup()
		{
			_person = PersonFactory.CreatePerson("sdfoj");
			_multiplicator = new MultiplicatorDefinitionSet("overtime paid", MultiplicatorType.Overtime);
			PersistAndRemoveFromUnitOfWork(_multiplicator);
			PersistAndRemoveFromUnitOfWork(_person);
		}

		protected override Repository<IPersonRequest> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return PersonRequestRepository.DONT_USE_CTOR(currentUnitOfWork);
		}

		protected override IPersonRequest CreateAggregateWithCorrectBusinessUnit()
		{
			IPersonRequest request = new PersonRequest(_person);

			var period = new DateTimePeriod(2008, 04, 1, 2008, 07, 20);
			request.Request = new OvertimeRequest(_multiplicator, period);
			request.Pending();
			return request;
		}

		protected override void VerifyAggregateGraphProperties(IPersonRequest loadedAggregateFromDatabase)
		{
			var org = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(org.Person, loadedAggregateFromDatabase.Person);
			Assert.AreEqual(((IOvertimeRequest) org.Request).MultiplicatorDefinitionSet,
				((IOvertimeRequest) loadedAggregateFromDatabase.Request).MultiplicatorDefinitionSet);
			Assert.AreEqual(org.Request.Period,
				loadedAggregateFromDatabase.Request.Period);
		}
	}
}