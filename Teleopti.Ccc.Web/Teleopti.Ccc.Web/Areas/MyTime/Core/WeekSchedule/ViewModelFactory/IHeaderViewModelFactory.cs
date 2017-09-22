using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
    public interface IHeaderViewModelFactory
    {
        HeaderViewModel CreateModel(IScheduleDay scheduleDay);
    }
}