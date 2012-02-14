using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
    public class RtaVisualLayer : VisualLayer, IRtaVisualLayer
    {
        public RtaVisualLayer(IRtaState state, DateTimePeriod period, IActivity dummyActivity)
            : base(state.StateGroup, period, dummyActivity)
        {
            State = state;
        }

        public bool IsLoggedOut { get; set; }

        public IRtaState State { get; private set; }
    }
}