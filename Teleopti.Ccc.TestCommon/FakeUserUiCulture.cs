using System.Globalization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeUserUiCulture : IUserUiCulture
	{
		private CultureInfo _culture;

		public FakeUserUiCulture(CultureInfo culture)
		{
			_culture = culture;
		}

		public CultureInfo GetUiCulture()
		{
			return _culture;
		}

		public void Is(CultureInfo culture)
		{
			_culture = culture;
		}
		
		public void IsSwedish()
		{
			Is(CultureInfoFactory.CreateSwedishCulture());
		}
	}
}