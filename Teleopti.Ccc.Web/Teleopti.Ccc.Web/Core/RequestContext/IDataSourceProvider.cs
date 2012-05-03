using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public interface IDataSourceProvider
	{
		IDataSource CurrentDataSource();
	}
}