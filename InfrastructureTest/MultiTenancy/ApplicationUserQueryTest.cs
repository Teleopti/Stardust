using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy
{
	public class ApplicationUserQueryTest : DatabaseTestWithoutTransaction
	{
		private Guid personId;
		private string correctUserName;
		private IApplicationUserQuery target;

		[Test]
		public void ShouldSucceed()
		{
			var result = target.FindUserData(correctUserName);
			result.Success.Should().Be.True();
		}

		[Test]
		public void ShouldFindPersonId()
		{
			var result = target.FindUserData(correctUserName);
			result.PersonId.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldFindTennant()
		{
			//this will change in the future
			var result = target.FindUserData(correctUserName);
			result.Tennant.Should().Be.EqualTo("Teleopti WFM");
		}

		[Test]
		public void ShouldFindPassword()
		{
			var result = target.FindUserData(correctUserName);
			result.Password.Should().Not.Be.Null();
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

			target.FindUserData(correctUserName)
				.Success.Should().Be.False();
		}

		[SetUp]
		public void Setup_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				correctUserName = "arna";
				var personInDatabase = PersonFactory.CreatePersonWithBasicPermissionInfo(correctUserName, "something");
				new PersonRepository(uow).Add(personInDatabase);
				uow.PersistAll();
				personId = personInDatabase.Id.Value;
			}
			target= new ApplicationUserQuery(() => new TennantDatabaseConnectionFactory(UnitOfWorkFactory.Current.ConnectionString));
		}

		[TearDown]
		public void Teardown_WillBeChangedWhenMovedAwayFromUnitOfWork()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonRepository(uow);
				var personInDatabase = rep.Get(personId);
				rep.Remove(personInDatabase);
				uow.PersistAll();
			}
		}
	}
}