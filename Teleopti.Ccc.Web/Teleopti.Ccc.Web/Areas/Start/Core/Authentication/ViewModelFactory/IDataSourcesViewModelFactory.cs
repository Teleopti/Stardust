using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory
{
	public interface IDataSourcesViewModelFactory
	{
		IEnumerable<DataSourceViewModel> DataSources();
	}
}