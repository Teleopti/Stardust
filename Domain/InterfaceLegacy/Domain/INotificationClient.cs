using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public interface INotificationClient
    {
        string MakeRequest(Uri url, string queryStringData);
    }
}