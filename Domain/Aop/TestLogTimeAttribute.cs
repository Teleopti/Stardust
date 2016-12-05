using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Common.TimeLogger
{
	public sealed class TestLogTimeAttribute : AspectAttribute
	{
		public TestLogTimeAttribute() : base(typeof(TestLogTimeAspect))
		{
		}

		public override int Order => -1000;
	}
}