using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Gets the user's culture
	/// </summary>
	public interface IUserCulture
	{
		/// <summary>
		/// get the culture
		/// </summary>
		/// <returns>the culture</returns>
		CultureInfo GetCulture();
	}
}