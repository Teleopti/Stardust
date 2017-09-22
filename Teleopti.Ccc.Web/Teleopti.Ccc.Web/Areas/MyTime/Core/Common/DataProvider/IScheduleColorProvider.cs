using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IScheduleColorProvider
	{
		IEnumerable<Color> GetColors(IScheduleColorSource source);
	}

	public interface IScheduleColorSource
	{
		IEnumerable<IScheduleDay> ScheduleDays { get; }
		IEnumerable<IVisualLayerCollection> Projections { get; }
		IEnumerable<IPreferenceDay> PreferenceDays { get; }
		IWorkflowControlSet WorkflowControlSet { get; }
	}

}