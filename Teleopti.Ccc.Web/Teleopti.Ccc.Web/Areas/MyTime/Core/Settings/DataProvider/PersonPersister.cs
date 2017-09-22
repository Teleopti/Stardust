using System.Globalization;
using MbCache.Core;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider
{
	public class PersonPersister : IPersonPersister
	{
		private readonly IMbCacheFactory _cacheFactory;
		private readonly IMakeRegionalFromPerson _makeRegionalFromPerson;

		public PersonPersister(IMbCacheFactory cacheFactory, IMakeRegionalFromPerson makeRegionalFromPerson)
		{
			_cacheFactory = cacheFactory;
			_makeRegionalFromPerson = makeRegionalFromPerson;
		}

		public void UpdateCulture(IPerson person, CultureInfo culture)
		{
			person.PermissionInformation.SetCulture(culture);
			_cacheFactory.Invalidate(_makeRegionalFromPerson, m => m.MakeRegionalFromPerson(person), true);
		}

		public void UpdateUICulture(IPerson person, CultureInfo culture)
		{
			person.PermissionInformation.SetUICulture(culture);
			_cacheFactory.Invalidate(_makeRegionalFromPerson, m => m.MakeRegionalFromPerson(person), true);
		}
	}
}