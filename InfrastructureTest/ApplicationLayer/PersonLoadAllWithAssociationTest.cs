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
	}
}