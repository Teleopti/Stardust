using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.People.Controllers;

namespace Teleopti.Ccc.Web.Areas.People.Core.Persisters
{
	public interface IPeoplePersister
	{
		IList<RawUser> Persist(IEnumerable<RawUser> users);
	}
}