using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	/// <summary>
	/// System logon interface
	/// </summary>
	public interface ILogOnOff
	{
		/// <summary>
		/// Logs on the system.
		/// </summary>
		/// <param name="dataSource">The uow factory.</param>
		/// <param name="loggedOnUser">The logged on user.</param>
		/// <param name="businessUnit">The business unit.</param>
		void LogOn(IDataSource dataSource,
						IPerson loggedOnUser,
						IBusinessUnit businessUnit);
	}
}