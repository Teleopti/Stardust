using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
    public interface IPeriodSelectionViewModelFactory
    {
        PeriodSelectionViewModel CreateModel(DateOnly dateOnly);
    }
}