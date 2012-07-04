using System.Linq;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class PersonPeriodProvider : IPersonPeriodProvider
	{
		private readonly ILoggedOnUser _personProvider;

		public PersonPeriodProvider(ILoggedOnUser personProvider)
		{
			_personProvider = personProvider;
		}

		public bool HasPersonPeriod(DateOnly date)
		{
			var personPeriods = _personProvider.CurrentUser().PersonPeriods(new DateOnlyPeriod(date, date));

			return personPeriods.Any();
		}
	}
}