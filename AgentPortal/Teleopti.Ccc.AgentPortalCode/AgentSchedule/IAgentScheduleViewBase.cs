namespace Teleopti.Ccc.AgentPortalCode.AgentSchedule
{
    public interface IAgentScheduleViewBase
    {
        bool IsNeedToReloadData { get; set; }
        void Refresh(bool reloadData);
        void SetResolution(int resolution);
    }
}
