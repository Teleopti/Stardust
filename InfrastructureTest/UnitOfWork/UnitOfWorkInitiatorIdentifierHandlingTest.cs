using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class UnitOfWorkInitiatorIdentifierHandlingTest
	{
		[Test]
		public void ShouldKeepInitiatorIdentifier()
		{
			var uowFactory = SetupFixtureForAssembly.DataSource.Application;
			using (var uow = uowFactory.CreateAndOpenUnitOfWork())
			{
				var initiatorIdentifier = new FakeInitiatorIdentifier();
				uow.PersistAll(initiatorIdentifier);
				uow.Initiator().Should().Be(initiatorIdentifier);
			}
		}

		[Test]
		public void ShouldKeepInitiatorIdentifierForCurrentUnitOfWorkCall()
		{
			var uowFactory = SetupFixtureForAssembly.DataSource.Application;
			using (var uow = uowFactory.CreateAndOpenUnitOfWork())
			{
				var initiatorIdentifier = new FakeInitiatorIdentifier();
				uow.PersistAll(initiatorIdentifier);
				CurrentUnitOfWork.Make().Current().Initiator().Should().Be(initiatorIdentifier);
			}
		}
	}
}