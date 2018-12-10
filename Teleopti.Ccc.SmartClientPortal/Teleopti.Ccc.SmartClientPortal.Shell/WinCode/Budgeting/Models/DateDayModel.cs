using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models
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
