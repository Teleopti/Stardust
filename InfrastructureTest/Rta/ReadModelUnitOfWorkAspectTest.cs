using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Infrastructure;
using IMessageSender = Teleopti.Interfaces.MessageBroker.Client.IMessageSender;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	public class ReadModelUnitOfWorkAspectTest
	{
		[Test]
		public void ShouldNotifyIfEventLeadsACommit()
		{
			var readModelFactory = MockRepository.GenerateMock<IReadModelUnitOfWorkFactory>();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var identity = MockRepository.GenerateMock<ICurrentIdentity>();
			identity.Stub(x => x.Current()).Return(new TeleoptiIdentity("", null, new BusinessUnit("bu").WithId(), null, null));
			var datasource = MockRepository.GenerateMock<ICurrentDataSource>();
			datasource.Stub(x => x.Current()).Return(new DataSource(uowFactory, null, readModelFactory, null));
			var messageSender = MockRepository.GenerateMock<IMessageSender>();
			var target = new ReadModelUnitOfWorkAspect(datasource, messageSender, identity);

			messageSender.Expect(x => x.Send(null)).IgnoreArguments();

			target.OnAfterInvokation(null);
		}
	}
}
