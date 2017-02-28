using System;
using System.IO;

namespace Teleopti.Interfaces.Domain
{
    public interface INotificationClient : IDisposable
    {
        Stream MakeRequest(string queryStringData);
    }
}