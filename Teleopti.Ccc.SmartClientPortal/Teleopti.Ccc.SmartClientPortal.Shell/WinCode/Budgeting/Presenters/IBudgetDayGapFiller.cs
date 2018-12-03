using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
    public interface IBudgetDayGapFiller
    {
        IList<IBudgetDay> AddMissingDays(IEnumerable<IBudgetDay> existingBudgetDays, DateOnlyPeriod period);
    }
}