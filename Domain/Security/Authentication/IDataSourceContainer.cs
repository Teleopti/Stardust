using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public interface IDataSourceContainer
    {
        IDataSource DataSource { get; }
        AuthenticationTypeOption AuthenticationTypeOption { get; }
        IRepositoryFactory RepositoryFactory { get; }
        IPerson User { get; }
        void SetUser(IPerson person);
        AuthenticationResult LogOn(string logOnName, string password);
		AuthenticationResult LogOn(string logOnName, string password, string clientIp);
        AuthenticationResult LogOn(string windowsLogOnName);
		void SaveLogonAttempt(bool successful, string clientIp);
		string LogOnName { get; set; }
    }
}