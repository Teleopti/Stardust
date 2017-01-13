using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Common.TimeLogger
{
	public sealed class TestLogAttribute : AspectAttribute
	{
		public TestLogAttribute() : base(typeof(TestLogAspect))
		{
		}

		public override int Order => -1000;
	}
}