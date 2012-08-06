using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence
{
    public class RtaVisualLayer : VisualLayer, IRtaVisualLayer
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public RtaVisualLayer(IRtaState state, DateTimePeriod period, IActivity dummyActivity, IPerson person)
            : base(state.StateGroup, period, dummyActivity, person)
        {
            State = state;
        }

        public bool IsLoggedOut { get; set; }

        public IRtaState State { get; private set; }
    }
}