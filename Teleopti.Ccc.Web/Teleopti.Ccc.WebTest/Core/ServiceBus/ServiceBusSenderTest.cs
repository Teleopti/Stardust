using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.ServiceBus;

namespace Teleopti.Ccc.WebTest.Core.ServiceBus
{
	[TestFixture]
	public class ServiceBusSenderTest
	{

		[Test]
		public void ShouldEnsureBus()
		{
			using (var sender = new ServiceBusSender())
			{
				sender.EnsureBus();
				sender.FieldValue<IContainer>("_customHost").Should().Not.Be.Null();
				sender.FieldValue<bool>("_isRunning").Should().Be.False(); //due to miss config file
			}
		}
	}
}