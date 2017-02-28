using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Models
{
    public class DateDayModel : IDateSupplier
    {
		public DateOnly Date { get; private set; }

		public DateDayModel(DateOnly date)
        {
            Date = date;
        }

		public override string ToString()
        {
            return Date.Day.ToString(CultureInfo.CurrentCulture);
        }
    }
}
