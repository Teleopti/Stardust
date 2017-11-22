using System.Linq;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	public class HintsHelper
	{
		public static string BuildErrorMessage(ValidationError ve)
		{
			return string.Format(Resources.ResourceManager.GetString(ve.ErrorResource), ve.ErrorResourceData?.ToArray() ?? new object[]{});
		}
	}
}
