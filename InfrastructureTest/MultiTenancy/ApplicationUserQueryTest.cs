﻿using System;
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
		public void ShouldFindUserDetails()
		{
			var personInDatabase = Session.Get<Person>(personId);
			var userDetails = new UserDetail(personInDatabase)
			{
				LastPasswordChange = DateTime.UtcNow,
				InvalidAttemptsSequenceStart = DateTime.UtcNow.AddHours(-1),
				InvalidAttempts = 73
			};
			PersistAndRemoveFromUnitOfWork(userDetails);


			var result = target.FindUserData(correctUserName);
			result.LastPasswordChange.Value.Should().Be.IncludedIn(userDetails.LastPasswordChange.AddMinutes(-1), userDetails.LastPasswordChange.AddMinutes(1));
			result.InvalidAttemptsSequenceStart.Value.Should().Be.IncludedIn(userDetails.InvalidAttemptsSequenceStart.AddMinutes(-1), userDetails.InvalidAttemptsSequenceStart.AddMinutes(1));
			result.InvalidAttempts.Should().Be.EqualTo(userDetails.InvalidAttempts);
			result.IsLocked.Should().Be.EqualTo(userDetails.IsLocked);
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

		[Test]
		public void DeletedUserShouldFail()
		{
			var personInDatabase = Session.Get<Person>(personId);
			personInDatabase.SetDeleted();
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
				uow.FetchSession().CreateQuery("delete from UserDetail").ExecuteUpdate();
				uow.PersistAll();
			}
		}
	}
}