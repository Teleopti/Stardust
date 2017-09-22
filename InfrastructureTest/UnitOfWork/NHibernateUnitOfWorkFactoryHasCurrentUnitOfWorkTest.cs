using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[InfrastructureTest]
	public class NHibernateUnitOfWorkFactoryHasCurrentUnitOfWorkTest
	{
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;

		[Test]
		public void HasCurrentUnitOfWork()
		{
			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				CurrentUnitOfWorkFactory.Current().HasCurrentUnitOfWork().Should().Be.True();
			}
		}

		[Test]
		public void HasNoCurrentUnitOfWork()
		{
			CurrentUnitOfWorkFactory.Current().HasCurrentUnitOfWork().Should().Be.False();
		}
	}
}