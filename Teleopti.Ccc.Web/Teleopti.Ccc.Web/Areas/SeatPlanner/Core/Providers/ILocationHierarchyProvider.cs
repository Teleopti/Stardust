using System;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public interface ILocationHierarchyProvider
	{
		LocationViewModel Get ();
	}
}