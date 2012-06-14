using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Gets the user's culture
	/// </summary>
	public interface IUserCulture
	{
		CultureInfo GetCulture();
	}
}