using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
	public interface IUserCulture
	{
		CultureInfo GetCulture();
	}
}