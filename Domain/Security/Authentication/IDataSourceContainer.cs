using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public interface IDataSourceContainer
	{
		IDataSource DataSource { get; }
		IPerson User { get; }
	}
}