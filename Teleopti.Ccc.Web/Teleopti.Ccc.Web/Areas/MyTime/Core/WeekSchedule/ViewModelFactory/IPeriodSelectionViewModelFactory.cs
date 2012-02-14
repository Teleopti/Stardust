using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
    public interface IPeriodSelectionViewModelFactory
    {
        PeriodSelectionViewModel CreateModel(DateOnly dateOnly);
    }
}