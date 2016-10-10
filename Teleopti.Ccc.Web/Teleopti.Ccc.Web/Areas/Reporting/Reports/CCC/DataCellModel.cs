using System;
using System.Data;
using System.Drawing;
using Teleopti.Ccc.Web.Areas.Reporting.Core;

namespace Teleopti.Ccc.Web.Areas.Reporting.Reports.CCC
{

	internal interface ITooltipContainer
	{
		IntervalToolTip GetToolTip(int getPersonId, int getIntervalId);
		IntervalToolTip GetToolTip(DateTime date, int getIntervalId);
	}

	internal class DataCellModel
	{
		private readonly ITooltipContainer _tooltipContainer;
		private readonly bool _perDate;

		public DataCellModel(DataRow row, ITooltipContainer tooltipContainer, bool perDate)
		{
			DataRow = row;
			_tooltipContainer = tooltipContainer;
			_perDate = perDate;
		}

		public DataRow DataRow { get; }

		public decimal TeamDeviation => DataRow["team_deviation_m"] == DBNull.Value ? 0 : (decimal)DataRow["team_deviation_m"];
		public decimal TeamAdherence => DataRow["team_adherence"] == DBNull.Value ? 0 : (decimal)DataRow["team_adherence"];
		public int IntervalId => (int)DataRow["interval_id"];
		public int IntervalCounter => (int)DataRow["date_interval_counter"];
		public int PersonId => (int)DataRow["person_id"];
		public DateTime Date => (DateTime)DataRow["date"];
		public DateTime ShiftStartDate => (DateTime)DataRow["shift_startdate"];
		public bool ShiftOverMidnight => Date.IsLaterThan(ShiftStartDate);
		public bool LoggedInOnTheDayBefore => Date.IsEarlierThan(ShiftStartDate);
		public IntervalToolTip CellToolTip => _perDate ? _tooltipContainer.GetToolTip(ShiftStartDate, IntervalCounter) : _tooltipContainer.GetToolTip(PersonId, IntervalCounter);
		public bool IsLoggedIn => (bool)(DataRow["is_logged_in"]);
		public Color DisplayColor => Color.FromArgb((int)DataRow["display_color"]);
		public decimal ReadyTime => (decimal)DataRow["ready_time_m"];
		public bool HasDisplayColor => DataRow["display_color"] != DBNull.Value;
	}
}