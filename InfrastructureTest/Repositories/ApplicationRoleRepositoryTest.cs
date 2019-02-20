using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("BucketB")]
	public class ApplicationRoleRepositoryTest : RepositoryTest<IApplicationRole>
	{
		/// <summary>
		/// Runs every test. Implemented by repository's concrete implementation.
		/// </summary>
		protected override void ConcreteSetup()
		{
		}

		/// <summary>
		/// Creates an aggregate using the Bu of logged in user.
		/// Should be a "full detailed" aggregate
		/// </summary>
		/// <returns></returns>
		protected override IApplicationRole CreateAggregateWithCorrectBusinessUnit()
		{
			IApplicationRole ret = new ApplicationRole();
			ret.DescriptionText = "hej och";
			ret.Name = "ha";
			ret.SetBusinessUnit(BusinessUnitUsedInTests.BusinessUnit);
			return ret;
		}

		/// <summary>
		/// Verifies the aggregate graph properties.
		/// </summary>
		/// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
		protected override void VerifyAggregateGraphProperties(IApplicationRole loadedAggregateFromDatabase)
		{
			Assert.AreEqual(CreateAggregateWithCorrectBusinessUnit().DescriptionText,
				loadedAggregateFromDatabase.DescriptionText);
		}

		protected override Repository<IApplicationRole> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new ApplicationRoleRepository(currentUnitOfWork);
		}

		/// <summary>
		/// Verifies Get all application roles works.
		/// </summary>
		/// <remarks>
		/// Created By: kosalanp
		/// Created Date: 08-04-2008
		/// </remarks>
		[Test]
		public void VerifyGetAllApplicationRoles()
		{
			const int roleCount = 5;
			for (var i = 0; i < roleCount; i++)
			{
				var appRole = ApplicationRoleFactory.CreateRole("Role " + i, "Role Description " + i);
				var applicationFunction = new ApplicationFunction("Test");
				var applicationFunction1 = new ApplicationFunction("Test2");
				var availableData = new AvailableData();

				PersistAndRemoveFromUnitOfWork(applicationFunction);
				PersistAndRemoveFromUnitOfWork(applicationFunction1);

				appRole.AddApplicationFunction(applicationFunction);
				appRole.AddApplicationFunction(applicationFunction1);


				PersistAndRemoveFromUnitOfWork(appRole);

				appRole.AvailableData = availableData;
				availableData.ApplicationRole = appRole;

				PersistAndRemoveFromUnitOfWork(availableData);
			}

			var rolesList = new ApplicationRoleRepository(UnitOfWork).LoadAllApplicationRolesSortedByName();
			Assert.AreEqual(roleCount, rolesList.Count);

			Assert.IsTrue(LazyLoadingManager.IsInitialized(rolesList[0].BusinessUnit));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(rolesList[0].ApplicationFunctionCollection));
		}

		[Test]
		public void ShouldNotLoadRolesWithoutAvailableData()
		{
			const int roleCount = 5;
			for (var i = 0; i < roleCount; i++)
			{
				var appRole = ApplicationRoleFactory.CreateRole("Role " + i, "Role Description " + i);
				var applicationFunction = new ApplicationFunction("Test");
				var applicationFunction1 = new ApplicationFunction("Test2");
				var availableData = new AvailableData();

				PersistAndRemoveFromUnitOfWork(applicationFunction);
				PersistAndRemoveFromUnitOfWork(applicationFunction1);

				appRole.AddApplicationFunction(applicationFunction);
				appRole.AddApplicationFunction(applicationFunction1);

				PersistAndRemoveFromUnitOfWork(appRole);

				if (i >= roleCount - 1) continue;
				appRole.AvailableData = availableData;
				availableData.ApplicationRole = appRole;
				PersistAndRemoveFromUnitOfWork(availableData);
			}

			var rolesList = new ApplicationRoleRepository(UnitOfWork).LoadAllApplicationRolesSortedByName();
			Assert.AreEqual(4, rolesList.Count);
		}

		[Test]
		public void ShouldSearchRolesByDescription()
		{
			PersistAndRemoveFromUnitOfWork( ApplicationRoleFactory.CreateRole("Admin Name" , "Role Description " ));
			PersistAndRemoveFromUnitOfWork(  ApplicationRoleFactory.CreateRole("Normal Name" , "Role Description " ));
			
			var rolesList = new ApplicationRoleRepository(UnitOfWork).LoadAllRolesByDescription("Role");
			rolesList.Count.Should().Be.EqualTo(2);
		}
	}
}
