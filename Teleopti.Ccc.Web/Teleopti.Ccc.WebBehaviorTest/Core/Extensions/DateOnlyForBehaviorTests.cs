

using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Extensions
{
	public static class DateOnlyForBehaviorTests
	{
		private static readonly DateOnly Today = DateOnly.Today;

		public static DateOnly TestToday { get { return Today; } }
	}
}