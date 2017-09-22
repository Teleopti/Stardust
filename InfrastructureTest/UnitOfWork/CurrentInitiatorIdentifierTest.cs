using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	[InfrastructureTest]
	public class CurrentInitiatorIdentifierTest
	{
		public ICurrentUnitOfWorkFactory UnitOfWorkFactory;
		public ICurrentInitiatorIdentifier InitiatorIdentifier;
		public IInitiatorIdentifierScope InitiatorIdentifierScope;
		
		[Test]
		public void ShouldReturnInitiatorFromUnitOfWorkPersistAll()
		{
			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var initiatorIdentifier = new FakeInitiatorIdentifier();
				uow.PersistAll(initiatorIdentifier);
				InitiatorIdentifier.Current().Should().Be(initiatorIdentifier);
			}
		}

		[Test]
		public void ShouldNotReturnInitiatorFromUnitOfWorkAfterPersistAll()
		{
			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var initiatorIdentifier = new FakeInitiatorIdentifier();
				uow.PersistAll(initiatorIdentifier);
			}
			InitiatorIdentifier.Current().Should().Be.Null();
		}

		[Test]
		public void ShouldReturnInitiatorFromScope()
		{
			var initiatorIdentifier = new FakeInitiatorIdentifier();
			using (InitiatorIdentifierScope.OnThisThreadUse(initiatorIdentifier))
			{
				InitiatorIdentifier.Current().Should().Be(initiatorIdentifier);
				using (UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					InitiatorIdentifier.Current().Should().Be(initiatorIdentifier);
				}
				InitiatorIdentifier.Current().Should().Be(initiatorIdentifier);
			}
		}

		[Test]
		public void ShouldNotReturnInitiatorAfterScope()
		{
			var initiatorIdentifier = new FakeInitiatorIdentifier();
			using (InitiatorIdentifierScope.OnThisThreadUse(initiatorIdentifier))
			{
				using (UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
				}
			}
			InitiatorIdentifier.Current().Should().Be.Null();
		}
	}
}