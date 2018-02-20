namespace Teleopti.Ccc.Infrastructure.Util
{
	public interface IApplicationInsights
	{
		void Init();
		void TrackEvent(string description);
	}
}