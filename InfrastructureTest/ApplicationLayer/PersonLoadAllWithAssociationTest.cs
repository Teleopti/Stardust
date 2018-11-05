using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[DatabaseTest]
	public class PersonLoadAllWithAssociationTest
	{
		public Database Database;
		public IPersonLoadAllWithAssociation Target;
		public WithUnitOfWork UnitOfWork;

		[Test]
		public void ShouldLoad()
		{
			Database.WithAgent();

			UnitOfWork.Get(() => Target.LoadAll())
				.Should().Not.Be.Empty();
		}
		
		[Test]
		public void ShouldLoadDistinctPersonsWithManyPeriods()
		{
			Database
				.WithAgent()
				.WithPersonPeriod("2018-10-01")
				.WithPersonPeriod("2018-11-01");

			UnitOfWork.Get(() => Target.LoadAll()).Where(x => x.Id == Database.CurrentPersonId())
				.Should().Have.Count.EqualTo(1);
		}
		
		[Test]
		public void ShouldLoadDistinctPersonsWithManyExternalLogOns()
		{
			Database
				.WithAgent()
				.WithExternalLogon("firstLogon")
				.WithExternalLogon("secondLogon");

			UnitOfWork.Get(() => Target.LoadAll()).Where(x => x.Id == Database.CurrentPersonId())
				.Should().Have.Count.EqualTo(1);
		}
		
		[Test]
		public void ShouldLoadDistinctPersonsWithAllPersonPeriods()
		{
			Database
				.WithPerson()
				.WithPersonPeriod("2018-10-01")
				.WithPersonPeriod("2018-11-01");

			UnitOfWork.Get(() => Target.LoadAll())
				.Where(x => x.Id == Database.CurrentPersonId())
				.SelectMany(x => x.PersonPeriodCollection)
				.Select(x => x.StartDate)
				.Should().Have.SameSequenceAs("2018-10-01".Date(), "2018-11-01".Date());
		}
		
		[Test]
		public void ShouldLoadDistinctPersonsWithAllExternalLogOns()
		{
			Database
				.WithPerson()
				.WithPersonPeriod("2018-10-01")
				.WithExternalLogon("firstLogon")
				.WithPersonPeriod("2018-11-01")
				.WithExternalLogon("secondLogon")
				.WithExternalLogon("thirdLogon");

			UnitOfWork.Get(() => Target.LoadAll())
				.Where(x => x.Id == Database.CurrentPersonId())
				.SelectMany(x => x.PersonPeriodCollection)
				.SelectMany(x => x.ExternalLogOnCollection)
				.Select(x => x.AcdLogOnOriginalId)
				.Should().Have.SameSequenceAs("firstLogon", "secondLogon", "thirdLogon");
		}
	}
}