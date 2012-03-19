using Teleopti.Ccc.UserTexts;
using System;
using System.ComponentModel.DataAnnotations;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public class SignInBusinessUnitModel
	{
		[Required(ErrorMessageResourceName = "BusinessUnitRequired", ErrorMessageResourceType = typeof(Resources))]
		[Display(Name = "PleaseChooseABusinessUnit", ResourceType = typeof(Resources))]
		public Guid BusinessUnitId { get; set; }
		public string DataSourceName { get; set; }
		public Guid PersonId { get; set; }
		public int AuthenticationType { get; set; }
	}
}