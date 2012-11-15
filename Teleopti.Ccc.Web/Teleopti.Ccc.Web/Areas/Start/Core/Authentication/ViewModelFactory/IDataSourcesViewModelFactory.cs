using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory
{
	public interface IDataSourcesViewModelFactory
	{
		IEnumerable<DataSourceViewModelNew> DataSources();
	}
}