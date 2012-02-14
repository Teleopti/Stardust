using System.Linq;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Start.Models.Authentication
{
	public class SignInBusinessUnitViewModel
	 {
		  public IEnumerable<BusinessUnitViewModel> BusinessUnits { get; set; }
		  public SignInBusinessUnitModel SignIn { get; set; }

		  public bool HasBusinessUnits
		  {
				get { return BusinessUnits != null && BusinessUnits.Any(); }
		  }
	 }
}