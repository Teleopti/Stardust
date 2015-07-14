using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Ccc.Web.Areas.People.Controllers;

namespace Teleopti.Ccc.Web.Areas.People.Core
{
	public interface IPeoplePersister
	{
		dynamic Persist(IEnumerable<RawUser> users);
	}
}