using WatiN.Core;
using WatiN.Core.Constraints;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Robustness
{
	public static class QuicklyFind
	{
		public static Constraint ByClass(string className)
		{
			return Find.BySelector("." + className);
		}
	}
}