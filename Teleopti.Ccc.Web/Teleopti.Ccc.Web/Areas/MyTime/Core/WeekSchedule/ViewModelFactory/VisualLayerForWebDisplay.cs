using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
    public class VisualLayerForWebDisplay : VisualLayer
    {
        public VisualLayerForWebDisplay(IPayload payload, DateTimePeriod period, IActivity highestPriorityActivity) : base(payload, period, highestPriorityActivity)
        {
        }

        public DateTimePeriod VisualPeriod { get; set; }
    }
}