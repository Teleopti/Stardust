namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public interface INotificationClient
    {
        string MakeRequest(INotificationConfigReader config, string queryStringData);
    }
}