using NUnit.Framework;
using SharpTestsEx;
using System;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class PersonAccessAuditRepositoryTest : RepositoryTest<IPersonAccess>
	{
		private IPerson _personActionOn;
		private IPersonAccess _personAccessBase;
		protected override void ConcreteSetup()
		{
			_personActionOn = PersonFactory.CreatePerson(new Name("Test1", "Test2"));
			PersistAndRemoveFromUnitOfWork(_personActionOn);
			_personAccessBase = new PersonAccess(LoggedOnPerson, _personActionOn, "RevokeRole", "Change", "{ \"Data\"=\"Some Json Data\" }", Guid.NewGuid());
			base.ConcreteSetup();
		}

		protected override IPersonAccess CreateAggregateWithCorrectBusinessUnit()
		{
			return new PersonAccess(
				LoggedOnPerson, 
				_personAccessBase.ActionPerformedOn, 
				_personAccessBase.Action, 
				_personAccessBase.ActionResult, 
				_personAccessBase.Data, 
				_personAccessBase.Correlation);
		}

		protected override Repository<IPersonAccess> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PersonAccessAuditRepository(currentUnitOfWork);
		}

		protected override void VerifyAggregateGraphProperties(IPersonAccess loadedAggregateFromDatabase)
		{
			loadedAggregateFromDatabase.ActionPerformedBy.Should().Be.EqualTo(LoggedOnPerson);
			loadedAggregateFromDatabase.ActionPerformedOn.Should().Be.EqualTo(_personAccessBase.ActionPerformedOn);
			loadedAggregateFromDatabase.Action.Should().Be.EqualTo(_personAccessBase.Action);
			loadedAggregateFromDatabase.ActionResult.Should().Be.EqualTo(_personAccessBase.ActionResult);
			loadedAggregateFromDatabase.Data.Should().Be.EqualTo(_personAccessBase.Data);
			loadedAggregateFromDatabase.Correlation.Should().Be.EqualTo(_personAccessBase.Correlation);
		}
	}
}
