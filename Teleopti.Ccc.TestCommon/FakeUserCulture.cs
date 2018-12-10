using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.TestCommon
{
	public class FakeUserCulture : IUserCulture
	{
		private CultureInfo _culture;

		public FakeUserCulture(CultureInfo culture)
		{
			_culture = culture;
		}

		public FakeUserCulture()
		{
			
		}

		public CultureInfo GetCulture()
		{
			return _culture;
		}

		public void Is(CultureInfo culture)
		{
			_culture = culture;
		}

		public void IsCatalan()
		{
			Is(CultureInfoFactory.CreateCatalanCulture());
		}

		public void IsSwedish()
		{
			Is(CultureInfoFactory.CreateSwedishCulture());
		}
	}
}