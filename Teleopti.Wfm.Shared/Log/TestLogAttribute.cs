using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public sealed class TestLogAttribute : AspectAttribute
	{
		public TestLogAttribute() : base(typeof(TestLogAspect))
		{
		}

		public override int Order => -1000;
	}
}