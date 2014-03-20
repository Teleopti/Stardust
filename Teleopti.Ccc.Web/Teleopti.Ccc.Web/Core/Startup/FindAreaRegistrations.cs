using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class FindAreaRegistrations : IFindAreaRegistrations
	{
		public IEnumerable<AreaRegistration> AreaRegistrations()
		{
			return GetType().Assembly.GetTypes()
				.Where(x => x.IsSubclassOf(typeof (AreaRegistration)))
				.Select(x => (AreaRegistration)Activator.CreateInstance(x));
		}
	}
}