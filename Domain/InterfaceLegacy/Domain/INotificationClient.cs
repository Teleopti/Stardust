using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public interface INotificationClient : IDisposable
    {
        string MakeRequest(string queryStringData);
    }
}