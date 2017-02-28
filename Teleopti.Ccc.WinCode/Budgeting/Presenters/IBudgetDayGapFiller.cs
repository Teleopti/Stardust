using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Presenters
{
    public interface IBudgetDayGapFiller
    {
        IList<IBudgetDay> AddMissingDays(IEnumerable<IBudgetDay> existingBudgetDays, DateOnlyPeriod period);
    }
}