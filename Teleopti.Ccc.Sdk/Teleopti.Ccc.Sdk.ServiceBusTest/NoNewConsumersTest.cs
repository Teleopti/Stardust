using System.Linq;
using NUnit.Framework;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Sdk.ServiceBus;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
	public class NoNewConsumersTest
	{
		[Test]
		public void ShouldNotAllowNewConsumersForRhino()
		{
			var classes = typeof (BusStartup).Assembly.GetTypes();
			classes.Where(c => c.GetInterfaces().Any(x =>
				x.IsGenericType &&
				x.GetGenericTypeDefinition() == typeof (Consumer<>))).Should().Be.Empty();
		}

		[Test]
		public void ShouldNotAllowNewConsumerOfForRhino()
		{
			var classes = typeof(BusStartup).Assembly.GetTypes();
			classes.Count(c => c.GetInterfaces().Any(x =>
				x.IsGenericType &&
				x.GetGenericTypeDefinition() == typeof(ConsumerOf<>))).Should().Be.LessThanOrEqualTo(12);
		}
	}
}