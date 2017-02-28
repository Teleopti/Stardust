using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public interface IDataSourceContainer
	{
		IDataSource DataSource { get; }
		IPerson User { get; }
	}
}