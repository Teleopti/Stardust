using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestFixture]
	public class IntradayRequestProcessorTest : ISetup
	{
		public IntradayRequestProcessor Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<IntradayRequestProcessor>().For<IntradayRequestProcessor>();
		}


		[Test]
		public void ShouldDenyIfAlreadyAbsent()
		{	

		}

		[Test]
		public void ShouldDenyIfNotEnoughPersonAccountBalance()
		{

		}

		[Test]
		public void ShouldDenyIfUnderstaffedPrimarySkill()
		{

		}

		[Test]
		public void ShouldApproveIfAllChecksOk()
		{

		}
	}
}
