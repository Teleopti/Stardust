using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public class CheckingResult
	{
		public Guid PersonId { get; set; }
		public string Name { get; set; }
	}

	public class ActivityLayerOverlapCheckingResult : CheckingResult
	{
		public IList<OverlappedLayer> OverlappedLayers { get; set; }
	}
}