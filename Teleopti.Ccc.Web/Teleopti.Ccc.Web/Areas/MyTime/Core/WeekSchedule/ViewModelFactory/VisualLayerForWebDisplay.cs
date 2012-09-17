using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
    public class VisualLayerForWebDisplay : VisualLayer
    {
        public VisualLayerForWebDisplay(IPayload payload, DateTimePeriod period, IActivity highestPriorityActivity, IPerson person) : base(payload, period, highestPriorityActivity, person)
        {
        }

        public DateTimePeriod VisualPeriod { get; set; }
    }
}