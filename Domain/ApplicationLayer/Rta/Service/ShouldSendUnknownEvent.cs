namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IShouldPublishUnknownEvent
	{
		bool ShouldPublish();
	}

	public class ShouldSendUnknownEvent : IShouldPublishUnknownEvent
	{
		public bool ShouldPublish()
		{
			return true;
		}
	}

	public class DontSendUnknownEvent : IShouldPublishUnknownEvent
	{
		public bool ShouldPublish()
		{
			return false;
		}
	}
}