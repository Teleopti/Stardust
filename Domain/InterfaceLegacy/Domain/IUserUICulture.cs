using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
	public interface IUserUiCulture
	{
		CultureInfo GetUiCulture();
	}
}