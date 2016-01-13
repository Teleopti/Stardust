using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Service;

namespace Teleopti.Analytics.Etl.CommonTest.Service
{
	[EtlTest]
	public class ResolveTest
	{
		public EtlService Service;

		[Test]
		public void ShouldResolve()
		{
			Service.Should().Not.Be.Null();
		}

	}
}
