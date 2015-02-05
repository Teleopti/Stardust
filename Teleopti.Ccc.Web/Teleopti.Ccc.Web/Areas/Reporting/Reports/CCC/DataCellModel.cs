using System;
using System.Data;
using System.Drawing;
using Teleopti.Analytics.Portal.Reports.Ccc;
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

		public DataRow DataRow { get; private set; }

		public decimal TeamDeviation
		{
			get
			{
				if (DataRow["team_deviation_m"] == DBNull.Value)
					return 0;
				return (Decimal)DataRow["team_deviation_m"];
			}
		}

		public decimal TeamAdherence
		{
			get
			{
				if (DataRow["team_adherence"] == DBNull.Value)
					return 0;
				return (Decimal)DataRow["team_adherence"];
			}
		}

		public int IntervalId { get { return (int)DataRow["interval_id"]; } }

		public int IntervalCounter { get { return (int)DataRow["date_interval_counter"]; } }

		public int PersonId { get { return (int)DataRow["person_id"]; } }

		public DateTime Date
		{
			get { return (DateTime)DataRow["date"]; }
		}

		public DateTime ShiftStartDate
		{
			get { return (DateTime)DataRow["shift_startdate"]; }
		}

		public bool ShiftOverMidnight
		{
			get { return Date.IsLaterThan(ShiftStartDate); }
		}

		public bool LoggedInOnTheDayBefore
		{
			get { return Date.IsEarlierThan(ShiftStartDate); }
		}

		public IntervalToolTip CellToolTip
		{
			get
			{
				if (_perDate)
				{
					return _tooltipContainer.GetToolTip(ShiftStartDate, IntervalCounter);
				}
				return _tooltipContainer.GetToolTip(PersonId, IntervalCounter);
			}
		}

		public bool IsLoggedIn { get { return (bool)(DataRow["is_logged_in"]); } }

		public Color DisplayColor { get { return Color.FromArgb((int)DataRow["display_color"]); } }

		public decimal ReadyTime { get { return (decimal)DataRow["ready_time_m"]; } }

		public bool HasDisplayColor { get { return DataRow["display_color"] != DBNull.Value; } }
	}
}