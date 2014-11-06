using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
	public class ThisCulture : IUserCulture
	{
		private readonly CultureInfo _cultureInfo;

		public ThisCulture(CultureInfo cultureInfo)
		{
			_cultureInfo = cultureInfo;
		}

		public CultureInfo GetCulture() { return _cultureInfo; }
	}
}