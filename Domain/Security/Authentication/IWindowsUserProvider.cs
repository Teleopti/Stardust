namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public interface IWindowsUserProvider
    {
        string DomainName { get; }
        string UserName { get; }
    }
}