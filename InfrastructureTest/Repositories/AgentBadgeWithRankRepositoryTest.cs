using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("BucketB")]
	public class AgentBadgeWithRankRepositoryTest : DatabaseTest
	{
		private IAgentBadgeWithRankRepository _target;
		private static readonly Guid personId = Guid.NewGuid();
		private readonly IPerson person = PersonFactory.CreatePersonWithId(personId);
		private readonly DateOnly calculateDate = new DateOnly(2018, 04, 03);

		[SetUp]
		public void SetUp()
		{
			_target = new AgentBadgeWithRankRepository(UnitOfWorkFactory.CurrentUnitOfWork());
		}

		protected override void SetupForRepositoryTest()
		{
			createAndSaveTable();
			UnitOfWork.PersistAll();
			CleanUpAfterTest();
		}

		private void createAndSaveTable()
		{
			Session.CreateSQLQuery("Insert into [Dbo].[Person] (Id, Version, UpdatedBy, UpdatedOn, Email, Note, EmploymentNumber, FirstName, LastName, DefaultTimeZone, IsDeleted)" +
									"Values(:personId, 1, :personId, '2018-04-03', '', '', 9527, 'a', 'a', 'UTC', 0)")
				.SetGuid("personId", personId)
				.ExecuteUpdate();

			Session.CreateSQLQuery(
					"Insert into [Dbo].[AgentBadgeWithRankTransaction] (Id, Person,BadgeType,BronzeBadgeAmount,SilverBadgeAmount, GoldBadgeAmount, CalculatedDate,[Description], InsertedOn, IsExternal)" +
					" Values (NEWID(),:personId,2,3,5, 8,:calculatedDate,'','2018-04-03',1)")
				.SetGuid("personId", person.Id.Value)
				.SetDateOnly("calculatedDate", calculateDate)
				.ExecuteUpdate();
		}

		[Test]
		public void ShouldFindAgentBadgeWithinPeriod()
		{
			var period = new DateOnlyPeriod(DateOnly.MinValue, calculateDate);

			var result = _target.Find(person, 2, true, period);
			result.Person.Should().Be.EqualTo(personId);
			result.BronzeBadgeAmount.Should().Be.EqualTo(3);
			result.SilverBadgeAmount.Should().Be.EqualTo(5);
			result.GoldBadgeAmount.Should().Be.EqualTo(8);
		}

		[Test]
		public void ShouldNotFindAgentBadgeWithoutPeriod()
		{
			var period = new DateOnlyPeriod(DateOnly.MinValue, calculateDate.AddDays(-1));

			var result = _target.Find(person, 2, true, period);
			result.Should().Be.Null();
		}
	}
}
