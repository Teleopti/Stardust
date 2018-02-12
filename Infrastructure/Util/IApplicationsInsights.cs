namespace Teleopti.Ccc.Infrastructure.Util
{
	public interface IApplicationsInsights
	{
		void Init();
		void TrackEvent(string description);
	}
}