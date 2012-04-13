namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public interface ISessionSpecificDataStringSerializer
	{
		string Serialize(SessionSpecificData data);
		SessionSpecificData Deserialize(string stringData);
	}
}