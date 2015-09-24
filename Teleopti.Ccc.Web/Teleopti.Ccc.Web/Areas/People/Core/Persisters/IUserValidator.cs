using System.Collections.Generic;
using System.Text;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.Persisters
{
	public interface IUserValidator
	{
		bool Validate(RawUser user, IDictionary<string, IApplicationRole> availableRoles, StringBuilder errorMsgBuilder);
	}
}