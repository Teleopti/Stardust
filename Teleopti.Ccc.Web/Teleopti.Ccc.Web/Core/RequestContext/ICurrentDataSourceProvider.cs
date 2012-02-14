using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public interface ICurrentDataSourceProvider
	{
		IDataSource CurrentDataSource();
	}
}