using System.Globalization;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class CultureInfoFactory
	{
		public static CultureInfo CreateEnglishCulture()
		{
			return CultureInfo.GetCultureInfo("en-GB");
		}

		public static CultureInfo CreateSwedishCulture()
		{
            return CultureInfo.GetCultureInfo("sv-SE");
		}

		public static CultureInfo CreateFinnishCulture()
		{
			return CultureInfo.GetCultureInfo("fi-Fi");
		}

		public static CultureInfo CreateCatalanCulture()
		{
			return CultureInfo.GetCultureInfo("ca-ES");
		}
	}
}
