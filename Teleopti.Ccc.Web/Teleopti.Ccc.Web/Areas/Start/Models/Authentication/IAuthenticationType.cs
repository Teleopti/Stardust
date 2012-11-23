using System.Collections.Generic;
using System.Web.Mvc;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public interface IAuthenticationType
	{
		string TypeString { get; }
		IEnumerable<IDataSource> DataSources();
		IAuthenticationModel BindModel(ModelBindingContext bindingContext);
	}
}