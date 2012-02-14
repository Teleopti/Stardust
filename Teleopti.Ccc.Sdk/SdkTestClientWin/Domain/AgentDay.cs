using SdkTestClientWin.Sdk;

namespace SdkTestClientWin.Domain
{
    public class AgentDay
    {
        private readonly Agent _agent;
        private readonly SchedulePartDto _dto;
        
        //temp
        private MainShift _mainShift;
        private OvertimeShift _overtimeShift;

        public AgentDay(Agent agent, SchedulePartDto schedulePartDto)
        {
            _agent = agent;
            _dto = schedulePartDto;
        }

        public SchedulePartDto Dto
        {
            get { return _dto; }
        }

        public Agent Agent
        {
            get { return _agent; }
        }

        public MainShift MainShift
        {
            get { return _mainShift; }
            set { _mainShift = value; }
        }

        public OvertimeShift OvertimeShift
        {
            get {
                return _overtimeShift;
            }
            set {
                _overtimeShift = value;
            }
        }

        public string PublicNote { get; set; }
    }
}
