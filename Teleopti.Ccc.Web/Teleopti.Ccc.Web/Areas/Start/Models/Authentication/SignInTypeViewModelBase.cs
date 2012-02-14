using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public class SignInTypeViewModelBase<TModel>
	{
		public IEnumerable<DataSourceViewModel> DataSources { get; set; }

		public TModel SignIn { get; set; }

		public bool HasDataSource
		{
			get
			{
				return DataSources != null && DataSources.Any();
			}
		}
	}
}