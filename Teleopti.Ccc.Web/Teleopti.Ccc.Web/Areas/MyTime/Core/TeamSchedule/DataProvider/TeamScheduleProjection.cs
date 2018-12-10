using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public interface ITeamScheduleProjection
	{
		IEnumerable<ITeamScheduleLayer> Layers { get; }
		DateTime SortDate { get; }
		DayOff DayOff { get; }
	}

	public class TeamScheduleProjection : ITeamScheduleProjection
	{
		public TeamScheduleProjection() { }

		public TeamScheduleProjection(IEnumerable<ITeamScheduleLayer> layers, DateTime sortDate)
		{
			Layers = layers;
			SortDate = sortDate;
		}

		public IEnumerable<ITeamScheduleLayer> Layers { get; set; }
		public DateTime SortDate { get; set; }
		public DayOff DayOff { get; set; }
	}

	public interface ITeamScheduleLayer
	{
		DateTimePeriod? Period { get; }
		Color DisplayColor { get; }
		string ActivityName { get; }
	}

	public class TeamScheduleLayer : ITeamScheduleLayer
	{
		public DateTimePeriod? Period { get; set; }
		public Color DisplayColor { get; set; }
		public string ActivityName { get; set; }
	}
}