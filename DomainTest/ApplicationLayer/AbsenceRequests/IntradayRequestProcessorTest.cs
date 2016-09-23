using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestFixture]
	class IntradayRequestProcessorTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<IntradayRequestProcessor>().For<IntradayRequestProcessor>();
		}
	}
}
