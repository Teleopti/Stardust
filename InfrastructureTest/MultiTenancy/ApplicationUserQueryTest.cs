using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy
{
	public class ApplicationUserQueryTest : DatabaseTest
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
			var expected_shouldBeChangedLater = UnitOfWorkFactory.Current.Name;
			var result = target.FindUserData(correctUserName);
			result.Tennant.Should().Be.EqualTo(expected_shouldBeChangedLater);
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
			correctUserName = "arna";
			var personInDatabase = PersonFactory.CreatePersonWithBasicPermissionInfo(correctUserName, "something");
			PersistAndRemoveFromUnitOfWork(personInDatabase);
			personId = personInDatabase.Id.Value;
			target= new ApplicationUserQuery(new FixedCurrentUnitOfWork(UnitOfWork), new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal()));
		}
	}
}