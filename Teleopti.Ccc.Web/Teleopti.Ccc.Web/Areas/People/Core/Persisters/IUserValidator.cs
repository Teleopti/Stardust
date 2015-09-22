using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.Persisters
{
	public interface IUserValidator
	{
		bool Validate(RawUser user, Dictionary<string, IApplicationRole> availableRoles);
		string ErrorMessage { get; }
		IList<IApplicationRole> ValidRoles { get; }
	}
}