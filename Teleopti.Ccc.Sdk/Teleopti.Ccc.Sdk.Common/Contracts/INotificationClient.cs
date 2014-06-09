using System;
using System.IO;

namespace Teleopti.Ccc.Sdk.Common.Contracts
{
    public interface INotificationClient : IDisposable
    {
        Stream MakeRequest(string queryStringData);
    }
}