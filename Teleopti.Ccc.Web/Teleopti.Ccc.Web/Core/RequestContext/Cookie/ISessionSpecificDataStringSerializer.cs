namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	public interface ISessionSpecificDataStringSerializer
	{
		string Serialize(SessionSpecificData data);
		SessionSpecificData Deserialize(string stringData);
	}
}