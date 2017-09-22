using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class ScheduleColorSource : IScheduleColorSource
	{
		public IEnumerable<IScheduleDay> ScheduleDays { get; set; }
		public IEnumerable<IVisualLayerCollection> Projections { get; set; }
		public IEnumerable<IPreferenceDay> PreferenceDays { get; set; }
		public IWorkflowControlSet WorkflowControlSet { get; set; }
	}
}