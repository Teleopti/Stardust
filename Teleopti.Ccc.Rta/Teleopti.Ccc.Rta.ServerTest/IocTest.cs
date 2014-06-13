using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Ccc.Rta.Server.Adherence;

namespace Teleopti.Ccc.Rta.ServerTest
{
	public class IocTest
	{
		[Test]
		public void ShouldResolveRtaDataHandler()
		{
			using (var container = RtaContainerBuilder.CreateBuilder().Build())
			{
				container.Resolve<IRtaDataHandler>()
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldResolveAdherenceAggregator()
		{
			using (var container = RtaContainerBuilder.CreateBuilder().Build())
			{
				container.Resolve<IEnumerable<IActualAgentStateHasBeenSent>>()
					.Single().GetType().Should().Be<AdherenceAggregator>();
			}
		}

		[Test]
		public void ShouldResolveAdherenceAggregatorInitializor()
		{
			using (var container = RtaContainerBuilder.CreateBuilder().Build())
			{
				container.Resolve<AdherenceAggregatorInitializor>()
					.Should().Not.Be.Null();
			}
		}
	}
}
