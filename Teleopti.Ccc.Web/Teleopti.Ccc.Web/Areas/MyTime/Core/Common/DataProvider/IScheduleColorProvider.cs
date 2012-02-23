using System.Collections.Generic;
using System.Drawing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IScheduleColorProvider
	{
		IEnumerable<Color> GetColors(IEnumerable<IScheduleColorSource> source);
	}

	public interface IScheduleColorSource
	{
		IScheduleDay ScheduleDay { get; }
		IVisualLayerCollection Projection { get; }
	}

}