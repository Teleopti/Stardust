using System.Globalization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeUserCulture : IUserCulture
	{
		private CultureInfo _culture;

		public FakeUserCulture(CultureInfo culture)
		{
			_culture = culture;
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
	}
}