using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy
{
	public class IdentityUserQueryTest : DatabaseTestWithoutTransaction
	{
		private Guid personId;
		private string correctIdentity;
		private IIdentityUserQuery target;

		[Test]
		public void ShouldSucceed()
		{
			var result = target.FindUserData(correctIdentity);
			result.Success.Should().Be.True();
		}

		[Test]
		public void ShouldFindPersonId()
		{
			var result = target.FindUserData(correctIdentity);
			result.PersonId.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldFindTennant()
		{
			//this will change in the future
			var result = target.FindUserData(correctIdentity);
			result.Tennant.Should().Be.EqualTo("Teleopti WFM");
		}

		
		[Test]
		public void NonExistingUserShouldFail()
		{
			var result = target.FindUserData("incorrectUserName");
			result.Success.Should().Be.False();
		}

		[Test]
		public void TerminatedUserShouldFail()
		{
			var personInDatabase = Session.Get<Person>(personId);
			personInDatabase.TerminatePerson(new DateOnly(DateTime.Now.AddDays(-2)), new PersonAccountUpdaterDummy());
			PersistAndRemoveFromUnitOfWork(personInDatabase);

			target.FindUserData(correctIdentity)
				.Success.Should().Be.False();
		}

		[Test]
		public void DeletedUserShouldFail()
		{
			var personInDatabase = Session.Get<Person>(personId);
			personInDatabase.SetDeleted();
			PersistAndRemoveFromUnitOfWork(personInDatabase);

			target.FindUserData(correctIdentity)
				.Success.Should().Be.False();
		}

		[SetUp]
		public void Setup_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				correctIdentity = "arna";
				var personInDatabase = PersonFactory.CreatePersonWithIdentityPermissionInfo(correctIdentity);
				new PersonRepository(uow).Add(personInDatabase);
				uow.PersistAll();
				personId = personInDatabase.Id.Value;
			}
			target= new IdentityUserQuery(() => new TennantDatabaseConnectionFactory(UnitOfWorkFactory.Current.ConnectionString));
		}

		[TearDown]
		public void Teardown_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonRepository(uow);
				var personInDatabase = rep.Get(personId);
				rep.Remove(personInDatabase);
				uow.FetchSession().CreateQuery("delete from UserDetail").ExecuteUpdate();
				uow.PersistAll();
			}
		}
	}
}