

using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public static class DateOnlyExtensions
	{
		private const string FixedClientDateOnlyFormat = "yyyy-MM-dd";

		public static string ToFixedClientDateOnlyFormat(this DateOnly instance)
		{
			return instance.Date.ToString(FixedClientDateOnlyFormat);
		}
	}
}