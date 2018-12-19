using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public interface ICurrentDataSource
	{
		IDataSource Current();
		string CurrentName();
	}
}