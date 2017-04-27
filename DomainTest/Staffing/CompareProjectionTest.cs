using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	public class CompareProjectionTest
	{
		public CompareProjection Target;

		[Test]
		public void ShouldDoSomething()
		{

			Target.Compare(null, null).Count().Should().Be.EqualTo(0);
		}
	}
}
