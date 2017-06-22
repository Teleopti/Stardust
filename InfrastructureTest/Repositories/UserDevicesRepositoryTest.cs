using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class UserDevicesRepositoryTest : RepositoryTest<IUserDevice>
	{
		private IPerson person;
		protected override void ConcreteSetup()
		{
			person = PersonFactory.CreatePerson();
			PersistAndRemoveFromUnitOfWork(person);
		}
		protected override IUserDevice CreateAggregateWithCorrectBusinessUnit()
		{
			return new UserDevice
			{
				Owner = person,
				Token = "token1"
			};
		}

		protected override void VerifyAggregateGraphProperties(IUserDevice loadedAggregateFromDatabase)
		{
			var newItem = CreateAggregateWithCorrectBusinessUnit();

			Assert.AreEqual(newItem.Owner.Id, person.Id);
			Assert.AreEqual(newItem.Token, "token1");
		}

		protected override Repository<IUserDevice> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new UserDeviceRepository(currentUnitOfWork);
		}

		[Test]
		public void ShouldFindUserDeviceByToken()
		{
			var persisted = new UserDevice
			{
				Owner = person,
				Token = "newToken"
			};
			PersistAndRemoveFromUnitOfWork(persisted);

			var target = new UserDeviceRepository(CurrUnitOfWork);

			var userDevice = target.FindByToken("newToken");

			userDevice.Id.Should().Be.EqualTo(persisted.Id);
		}

		[Test]
		public void ShouldFindUserDeviceByPerson()
		{
			var persisted = new UserDevice
			{
				Owner = person,
				Token = "newToken"
			};
			PersistAndRemoveFromUnitOfWork(persisted);

			var target = new UserDeviceRepository(CurrUnitOfWork);

			var userDevice = target.Find(person).Single();

			userDevice.Id.Should().Be.EqualTo(persisted.Id);
		}

		[Test]
		public void ShouldThrowAExceptionWhenAddDuplicateUserDeviceToken()
		{
			var persisted = new UserDevice
			{
				Owner = person,
				Token = "newToken"
			};
			PersistAndRemoveFromUnitOfWork(persisted);
			var newPerson = PersonFactory.CreatePerson();
			PersistAndRemoveFromUnitOfWork(newPerson);

			Assert.Throws<ConstraintViolationException>(() =>
			{
				var duplicate = new UserDevice
				{
					Owner = newPerson,
					Token = "newToken"
				};
				PersistAndRemoveFromUnitOfWork(duplicate);
			});
		}
	}
}