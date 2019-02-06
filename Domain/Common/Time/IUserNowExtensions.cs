using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public static class UserNowExtensions
	{
		public static DateOnly Date(this IUserNow instance)
		{
			return new DateOnly(instance.DateTime());
		}
	}
}