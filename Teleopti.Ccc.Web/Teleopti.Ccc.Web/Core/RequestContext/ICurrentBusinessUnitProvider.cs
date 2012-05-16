using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public interface ICurrentBusinessUnitProvider
	{
		IBusinessUnit CurrentBusinessUnit();
	}
}