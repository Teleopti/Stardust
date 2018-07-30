using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
    public interface IHeaderViewModelFactory
    {
        HeaderViewModel CreateModel(IScheduleDay scheduleDay);
    }
}