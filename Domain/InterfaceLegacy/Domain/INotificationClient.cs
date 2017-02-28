using System;
using System.IO;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public interface INotificationClient : IDisposable
    {
        Stream MakeRequest(string queryStringData);
    }
}