using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeUserUiCulture : IUserUiCulture
	{
		private CultureInfo _culture;

		public FakeUserUiCulture()
		{
			Is(CultureInfoFactory.CreateSwedishCulture());
		}
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