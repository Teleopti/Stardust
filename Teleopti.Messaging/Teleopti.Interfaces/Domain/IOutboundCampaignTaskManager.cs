namespace Teleopti.Interfaces.Domain
{
    public interface IOutboundCampaignTaskManager
    {
        IBacklogTask GetIncomingTaskFromCampaign(IOutboundCampaign campaign);
    }
}
