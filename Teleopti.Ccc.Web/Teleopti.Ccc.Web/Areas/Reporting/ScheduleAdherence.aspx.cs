using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using Teleopti.Analytics.Parameters;
using Teleopti.Analytics.Portal.Reports.Ccc;
using Teleopti.Analytics.ReportTexts;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.Reporting.Core;
using Teleopti.Ccc.Web.Areas.Reporting.Reports.CCC;

namespace Teleopti.Ccc.Web.Areas.Reporting
{
	public partial class ScheduleAdherence : Page, ITooltipContainer
	{
		protected Guid ReportId;
		protected Guid GroupPageCode;

		private DataTable _dataTable;
		private int _intervalsPerDay;
		private int _intervalsPerHour;
		private int _intervalLength;
		private int _timeLineStartInterval;
		private int _timeLineEndInterval;
		private int _timeLineStartIntervalDayBefore;
		private int _timeLineEndIntervalDayAfter;
		private Decimal? _teamAdherenceTotal = -2;
		private Decimal? _teamDeviationTotal = -2;
		private IList<SqlParameter> _sqlParameterList;
		private IList<String> _parameterTextList;

		private readonly IDictionary<int, IList<IntervalToolTip>> _intervalToolTipDictionary =
			new Dictionary<int, IList<IntervalToolTip>>();
		private readonly IDictionary<DateTime, IList<IntervalToolTip>> _intervalDateToolTipDictionary =
			new Dictionary<DateTime, IList<IntervalToolTip>>();

		readonly SortedDictionary<int, summaryData> _colSummary = new SortedDictionary<int, summaryData>();
		class summaryData
		{
			public decimal Adherence { get; set; }
			public decimal Deviation { get; set; }
			public int Interval { get; set; }
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			if (!string.IsNullOrEmpty(Request.QueryString.Get("REPORTID")))
			{
				if (!Guid.TryParse(Request.QueryString["REPORTID"], out ReportId))
					return;
				ParameterSelector.ReportId = ReportId;

				using (var commonReports = new CommonReports(ParameterSelector.ConnectionString, ParameterSelector.ReportId))
				{
					Guid groupPageComboBoxControlCollectionId = commonReports.GetGroupPageComboBoxControlCollectionId();
					string groupPageComboBoxControlCollectionIdName = string.Format("ParameterSelector$Drop{0}",
						groupPageComboBoxControlCollectionId);

					GroupPageCode = string.IsNullOrEmpty(Request.Form.Get(groupPageComboBoxControlCollectionIdName))
						? Selector.BusinessHierarchyCode
						: new Guid(Request.Form.Get(groupPageComboBoxControlCollectionIdName));
					ParameterSelector.GroupPageCode = GroupPageCode;
					commonReports.LoadReportInfo();
					Page.Header.Title = commonReports.Name;
				}
			}
		}

		protected override void OnLoadComplete(EventArgs e)
		{
			base.OnLoadComplete(e);
			buttonShowReport.Enabled = ParameterSelector.IsValid;
		}

		//private IList<SqlParameter> SessionParameters
		//{
		//	get
		//	{
		//		if (!String.IsNullOrEmpty((Request.QueryString.Get("PARAMETERSKEY"))))
		//		{
		//			return (IList<SqlParameter>)Session["PARAMETERS" + Request.QueryString.Get("PARAMETERSKEY")];
		//		}

		//		return new List<SqlParameter>();
		//	}
		//}

		//private IList<string> SessionParameterTexts
		//{
		//	get
		//	{
		//		if (!String.IsNullOrEmpty((Request.QueryString.Get("PARAMETERSKEY"))))
		//		{
		//			return (IList<string>)Session["PARAMETERTEXTS" + Request.QueryString.Get("PARAMETERSKEY")];
		//		}

		//		return new List<string>();
		//	}
		//}

		private void CreateReport()
		{
			SetReportHeaderParmaterLabels();
			SetReportHeaderParmaterTexts();
			HideDynamicParameters();
			
			if (GetReportData())
			{
				HideTimeZoneParameter(false);
				SetIntervalInformation();
				if (ReportId.Equals(new Guid("6a3eb69b-690e-4605-b80e-46d5710b28af"))) //one agent per day
					setEarliestShiftStartAndLatestShiftEndPerDay();
				else	
					SetEarliestShiftStartAndLatestShiftEnd();
				CreateReportTable();
			}
			else
			{
				// Hide Time Zone parameter
				HideTimeZoneParameter(true);
			}
			reportData.Visible = true;
		}

		private void HideDynamicParameters()
		{
			if (ReportId.Equals(new Guid("6a3eb69b-690e-4605-b80e-46d5710b28af"))) //one agent per day
			{
				if (_sqlParameterList[3].Value == DBNull.Value ) //& _sqlParameterList[4].Value == DBNull.Value)
				{
					// Group page Business Hierarchy picked
					trGroupPageGroup.Visible = false;
					trGroupPageAgent.Visible = false;
				}
				else
				{
					// Other group page than Business Hierarchy picked
					trSite.Visible = false;
					trTeam.Visible = false;
					trAgent.Visible = false;
				}
			}
			else
			{
				trDates.Visible = false;
				if (_sqlParameterList[2].Value == DBNull.Value & _sqlParameterList[3].Value == DBNull.Value)
				{
					// Group page Business Hierarchy picked
					trGroupPageGroup.Visible = false;
					trGroupPageAgent.Visible = false;
				}
				else
				{
					// Other group page than Business Hierarchy picked
					trSite.Visible = false;
					trTeam.Visible = false;
					trAgent.Visible = false;
				}
			}
			
		}

		private void HideTimeZoneParameter(bool hide)
		{
			// Check if time zone will be hidden or not
			if (hide || ((bool)_dataTable.Rows[0]["hide_time_zone"]))
			{
				trTimeZoneParameter.Style.Add("display", "none");
			}
		}

		private bool GetReportData()
		{
			using (var commonReports = new CommonReports(ParameterSelector.ConnectionString, ReportId))
			{
				var dataset = commonReports.GetReportData(ParameterSelector.UserCode, ParameterSelector.BusinessUnitCode, _sqlParameterList);
				if (dataset != null && dataset.Tables.Count > 0)
				{
					_dataTable = dataset.Tables[0];
				}

				if (_dataTable == null || _dataTable.Rows.Count == 0)
				{
					return false;
				}
				return true;
			}
		}

		private void CreateReportTable()
		{
			var aspTable = new Table {CssClass = "ReportTable"};
			divReportTable.Controls.Clear();
			divReportTable.Controls.Add(aspTable);
			aspTable.Rows.Add(GetReportDetailHeaderRowWithTimeLineHour());
			aspTable.Rows.AddRange(GetReportDetailRows());
			aspTable.Rows.AddRange(GetIntervalTotalsRows());
			aspTable.Rows.AddAt(1, GetReportTotalsRow(true));
			aspTable.Rows.Add(GetReportTotalsRow(false));
		}

		private void CheckParametersCollection()
		{
			var isParameterListsValid = false;
			_sqlParameterList = ParameterSelector.Parameters;
			_parameterTextList = ParameterSelector.ParameterTexts;

			if (_sqlParameterList != null && _parameterTextList != null)
			{
				if (ReportId.Equals(new Guid("6a3eb69b-690e-4605-b80e-46d5710b28af"))) //one agent per day
				{
					if (_sqlParameterList.Count == 11 && _parameterTextList.Count == 11)
					{
						if (_sqlParameterList[0].ParameterName == "@date_from"
						    && _sqlParameterList[1].ParameterName == "@date_to"
						    && _sqlParameterList[2].ParameterName == "@group_page_code"
						    && _sqlParameterList[3].ParameterName == "@group_page_group_set"
							&& _sqlParameterList[4].ParameterName == "@group_page_agent_code"
						    && _sqlParameterList[5].ParameterName == "@site_id"
						    && _sqlParameterList[6].ParameterName == "@team_set"
						    && _sqlParameterList[7].ParameterName == "@agent_person_code"
							&& _sqlParameterList[8].ParameterName == "@adherence_id"
						    && _sqlParameterList[9].ParameterName == "@sort_by"
						    && _sqlParameterList[10].ParameterName == "@time_zone_id")
						{
							isParameterListsValid = true;
						}
					}
				}
				else
					{
						if (_sqlParameterList.Count == 10 && _parameterTextList.Count == 10)
						{
							if (_sqlParameterList[0].ParameterName == "@date_from"
							    && _sqlParameterList[1].ParameterName == "@group_page_code"
							    && _sqlParameterList[2].ParameterName == "@group_page_group_set"
							    && _sqlParameterList[3].ParameterName == "@group_page_agent_code"
							    && _sqlParameterList[4].ParameterName == "@site_id"
							    && _sqlParameterList[5].ParameterName == "@team_set"
							    && _sqlParameterList[6].ParameterName == "@agent_person_code"
							    && _sqlParameterList[7].ParameterName == "@adherence_id"
							    && _sqlParameterList[8].ParameterName == "@sort_by")
							{
								isParameterListsValid = true;
							}
						}
					}

			}
			if (!isParameterListsValid)
			{
				Response.Write("xxThe report selection could not be obtained. Please try to make a new selection for the report.");
				Response.End();
			}
		}

		private void SetReportHeaderParmaterTexts()
		{
			if (ReportId.Equals(new Guid("6a3eb69b-690e-4605-b80e-46d5710b28af"))) //one agent per day
			{
				tdDatesText.InnerText = _parameterTextList[0] + " - " + _parameterTextList[1];
				tdGroupPageText.InnerText = _parameterTextList[2];
				tdGroupPageGroupText.InnerText = _parameterTextList[3];
				tdGroupPageAgentText.InnerText = _parameterTextList[4];
				
				tdSiteText.InnerText = _parameterTextList[5];
				tdTeamText.InnerText = _parameterTextList[6];
				tdAgentText.InnerText = _parameterTextList[7];
				
				tdAdherenceCalculationText.InnerText = _parameterTextList[8];
				tdSortOrderText.InnerText = _parameterTextList[9];
				tdTimeZoneText.InnerText = _parameterTextList[10];
				
			}
			else
			{
				tdGroupPageText.InnerText = _parameterTextList[1];
				tdGroupPageGroupText.InnerText = _parameterTextList[2];
				tdGroupPageAgentText.InnerText = _parameterTextList[3];
			
				tdSiteText.InnerText = _parameterTextList[4];
				tdTeamText.InnerText = _parameterTextList[5];
				tdAgentText.InnerText = _parameterTextList[6];
				tdAdherenceCalculationText.InnerText = _parameterTextList[7];
				tdSortOrderText.InnerText = _parameterTextList[8];
				tdTimeZoneText.InnerText = _parameterTextList[9];
				tdDateText.InnerText = _parameterTextList[0];
			}
			
		}

		private void SetReportHeaderParmaterLabels()
		{
			if (ReportId.Equals(new Guid("6a3eb69b-690e-4605-b80e-46d5710b28af"))) //one agent per day
				tdReportName.InnerText = Resources.ResReportAdherencePerAgent;
			else
				tdReportName.InnerText = Resources.ResReportAdherencePerDay;

			tdDatesLabel.InnerText = Resources.ResShiftStartDateColon;
			tdGroupPageLabel.InnerText = Resources.ResGroupPageColon;
			tdGroupPageGroupLabel.InnerText = Resources.ResGroupPageGroupColon;
			tdGroupPageAgentLabel.InnerText = Resources.ResAgentColon;

			tdSiteLabel.InnerText = Resources.ResSiteNameColon;
			tdTeamLabel.InnerText = Resources.ResTeamNameColon;
			tdAgentLabel.InnerText = Resources.ResAgentColon;
			tdAdherenceCalculationLabel.InnerText = Resources.ResAdherenceCalculationColon;
			tdSortOrderLabel.InnerText = Resources.ResSortByColon;
			tdTimeZoneLabel.InnerText = Resources.ResTimeZoneColon;
			tdDateLabel.InnerText = Resources.ResShiftStartDateColon;

			tdTodaysDateTime.InnerText = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
			imageButtonPreviousDay.ToolTip = Resources.ResPrevious;
			imageButtonNextDay.ToolTip = Resources.ResNext;

			// per agent
			if (ReportId.Equals(new Guid("6A3EB69B-690E-4605-B80E-46D5710B28AF")))
				DayButtons.Visible = false;
		}

		private TableRow[] GetIntervalTotalsRows()
		{
			//fill it if there are holes in it
			for (var i = _timeLineStartIntervalDayBefore; i < _intervalsPerDay; i++)
			{
				if (!_colSummary.ContainsKey(i))
					_colSummary.Add(i, new summaryData { Interval = i });
			}
			for (var i = _timeLineStartInterval; i < _timeLineEndInterval; i++)
			{
				var interval = i + 1000;
				if(!_colSummary.ContainsKey(interval))
					_colSummary.Add(interval, new summaryData { Interval = interval });
			}
			for (var i = 0; i < _timeLineEndIntervalDayAfter; i++)
			{
				var interval = i + 2000;
				if (!_colSummary.ContainsKey(interval))
					_colSummary.Add(interval, new summaryData { Interval = interval });
			}
			IList<TableRow> tableRowList = new List<TableRow>();

			var tableRowSpace = new TableRow();
			tableRowSpace.Cells.Add(MakeTableCell("", HorizontalAlign.Center, VerticalAlign.Middle, "ReportRowSpace"));
			tableRowList.Add(tableRowSpace);

			// Team adherence total row
			var tableCellListAdherence = new List<TableCell>
			                             	{
			                             		MakeTableCell(Analytics.ReportTexts.Resources.ResAdherencePerIntervalPercent,
			                             		              HorizontalAlign.Left, VerticalAlign.Middle, "ReportTotalAdherence"),
			                             		MakeTableCell("&nbsp;",
			                             		              HorizontalAlign.Center,
			                             		              VerticalAlign.Middle, "ReportTotalAdherence"),
			                             		MakeTableCell("&nbsp;", HorizontalAlign.Center, VerticalAlign.Middle,
			                             		              "ReportTotalAdherence")
			                             	};

			tableCellListAdherence.AddRange(getIntervalTotalsCells(_colSummary, true));
			var tableRowAdherence = new TableRow();
			tableRowAdherence.Cells.AddRange(tableCellListAdherence.ToArray());
			tableRowList.Add(tableRowAdherence);

			// Team deviation total row
			var tableCellListDeviation = new List<TableCell>
			                             	{
			                             		MakeTableCell(Analytics.ReportTexts.Resources.ResDeviationPerIntervalMinute,
			                             		              HorizontalAlign.Left, VerticalAlign.Middle, "ReportTotalDeviation"),
			                             		MakeTableCell("&nbsp;", HorizontalAlign.Center, VerticalAlign.Middle,
			                             		              "ReportTotalDeviation"),
			                             		MakeTableCell("&nbsp;",
			                             		              HorizontalAlign.Center, VerticalAlign.Middle, "ReportTotalDeviation")
			                             	};

			tableCellListDeviation.AddRange(getIntervalTotalsCells(_colSummary, false));
			var tableRowDeviation = new TableRow();
			tableRowDeviation.Cells.AddRange(tableCellListDeviation.ToArray());
			tableRowList.Add(tableRowDeviation);

			return tableRowList.ToArray();
		}

		private IEnumerable<TableCell> getIntervalTotalsCells(SortedDictionary<int,summaryData> colSummary , bool isAdherence)
		{
			var tableCells = new List<TableCell>();

			foreach (var summaryData in colSummary)
			{
				string text;
				var cssClass = "ReportIntervalTotalDeviationCell";
				if (isAdherence)
				{
					text = (summaryData.Value.Adherence * 100).ToString("0", CultureInfo.CurrentCulture);
					cssClass = "ReportIntervalTotalAdherenceCell";
				}
				else
				{
					text = summaryData.Value.Deviation.ToString("0", CultureInfo.CurrentCulture);
				}
				
				var tableCell = MakeTableCell(text, HorizontalAlign.Center, VerticalAlign.Middle, cssClass);
				var interval = summaryData.Value.Interval;
				//if (((interval + 1) % _intervalsPerHour == 0) && (interval > 0))
				if (((interval + 1) % _intervalsPerHour == 0))
					tableCell.Style.Add("border-right", "solid 2px silver");

				tableCells.Add(tableCell);
			}

			return tableCells;
		}

		private TableRow GetReportTotalsRow(bool isTopTotals)
		{
			var tableRow = new TableRow();
			var cssClass = isTopTotals ? "ReportTotalsTop" : "ReportTotalsBottom";

			tableRow.Cells.Add(MakeTableCell(Analytics.ReportTexts.Resources.ResTotalsColon, HorizontalAlign.Left,
											VerticalAlign.Bottom, cssClass));

			var teamAdherenceTotal = _teamAdherenceTotal.HasValue ? ((decimal)(_teamAdherenceTotal * 100)).ToString("0.0", CultureInfo.CurrentCulture) : string.Empty;
			tableRow.Cells.Add(MakeTableCell(teamAdherenceTotal,HorizontalAlign.Center,VerticalAlign.Middle, cssClass));
			var teamDeviationTotal = _teamDeviationTotal.HasValue ? ((decimal)(_teamDeviationTotal)).ToString("0", CultureInfo.CurrentCulture) : string.Empty;
			tableRow.Cells.Add(MakeTableCell(teamDeviationTotal,HorizontalAlign.Center, VerticalAlign.Middle, cssClass));

			if (isTopTotals)
			{
				tableRow.Cells.AddRange(GetTimeLineIntervalCellArray());
			}
			else
			{
				var tableCellColumnSpan = MakeTableCell("&nbsp;", HorizontalAlign.Center, VerticalAlign.Middle, cssClass);
				tableCellColumnSpan.ColumnSpan = (_intervalsPerDay - _timeLineStartIntervalDayBefore) + (_timeLineEndInterval - _timeLineStartInterval) + _timeLineEndIntervalDayAfter;
				tableRow.Cells.Add(tableCellColumnSpan);
			}

			return tableRow;
		}

		private TableRow[] GetReportDetailRows()
		{
			var perDate = ReportId.Equals(new Guid("6a3eb69b-690e-4605-b80e-46d5710b28af"));
			var tableRowList = MakeTableRowList();
			var dataRowReaders = from r in _dataTable.Rows.Cast<DataRow>() select new DataCellModel(r, this, perDate);
			var dataPerPerson = dataRowReaders.GroupBy(r => new PersonModel(r.DataRow, perDate));
			dataPerPerson.ForEach((a, b) => ProcessPersonData(a, b, tableRowList));
			return tableRowList.ToArray();
		}

		private void ProcessPersonData(PersonModel personModel, IGrouping<PersonModel, DataCellModel> data, IList<TableRow> tableRowList)
		{
			var tableRow = MakeTableRow(personModel);

			
			var tableCellList = new List<TableCell>();
			if (personModel.LoggedInOnTheDayBefore)
			{
				tableCellList.AddRange(fillWithBlankCells(_timeLineStartIntervalDayBefore, personModel.FirstIntervalId));
			}
			else
			{
				tableCellList.AddRange(fillWithBlankCells(_timeLineStartIntervalDayBefore, _intervalsPerDay));
				tableCellList.AddRange(fillWithBlankCells(_timeLineStartInterval, personModel.FirstIntervalId));
			}
			
			var previousIntervalId = personModel.FirstIntervalId;

			SetTeamTotals(personModel);

			data.ForEach(m => ProcessCellData(m, ref previousIntervalId, tableCellList, personModel));

			EndRow(tableRowList, tableRow, tableCellList, previousIntervalId, personModel);
		}

		private void ProcessCellData(DataCellModel dataCellModel, ref int previousIntervalId, List<TableCell> tableCellList, PersonModel personModel)
		{
			if ((previousIntervalId + 1) != dataCellModel.IntervalId)
			{
				var tableCellBlancList = fillWithBlankCells(previousIntervalId + 1, dataCellModel.IntervalId);
				tableCellList.AddRange(tableCellBlancList);
			}
			if (dataCellModel.ShiftOverMidnight )
				personModel.EndsOnNextDate = true;
			previousIntervalId = dataCellModel.IntervalId;

			var tableCell = MakeTableCell(dataCellModel);
			StyleTableCellBorders(dataCellModel, tableCell);
			tableCellList.Add(tableCell);

			// Get team interval sum for adherence and deviation
			addColSummary(dataCellModel);
		}

		private IList<TableRow> MakeTableRowList() {
			IList<TableRow> tableRowList = new List<TableRow>();

			// Add a spacerow
			var tableRowSpace = new TableRow();
			var tableCellSpace = MakeTableCell("", HorizontalAlign.Center, VerticalAlign.Middle, "ReportRowSpace");
			tableCellSpace.Style.Add("border-top", "solid 2pt lightgrey");
			tableCellSpace.ColumnSpan = 3 + (_intervalsPerDay - _timeLineStartIntervalDayBefore) + (_timeLineEndInterval - _timeLineStartInterval) + _timeLineEndIntervalDayAfter;
			tableRowSpace.Cells.Add(tableCellSpace);
			tableRowList.Add(tableRowSpace);
			return tableRowList;
		}

		private void addColSummary(DataCellModel dataCellModel)
		{
			var key = dataCellModel.IntervalId;
			// Since the colSummary is a sorted dictionary with key interval we need a workaround here.
			// We could have data for three days here: 
			// "Day before" - no adding to the intervals
			// "Report date" - adding 1000 to the interval
			// "Day after" - adding 2000 to the interval
			if (dataCellModel.ShiftOverMidnight)
				key += 2000;
			else if (!dataCellModel.LoggedInOnTheDayBefore)
				key += 1000;

			if(!_colSummary.ContainsKey(key))
				_colSummary.Add(key, new summaryData{Adherence = dataCellModel.TeamAdherence, Deviation = dataCellModel.TeamDeviation, Interval = dataCellModel.IntervalId});
		}
		

		private void StyleTableCellBorders(DataCellModel dataCellModel, TableCell tableCell)
		{
			if ((dataCellModel.IntervalId + 1) % _intervalsPerHour == 0)
				tableCell.Style.Add("border-right", "solid 2px silver");
		}

		private TableCell MakeTableCell(DataCellModel dataCellModel) {
			var cssClass = "ReportIntervalCell";
			if (!dataCellModel.HasDisplayColor)
			{
				return MakeTableCell(dataCellModel.ReadyTime.ToString("0", CultureInfo.CurrentCulture),
					             HorizontalAlign.Center,
					             VerticalAlign.Middle, cssClass);
			}

			var color = dataCellModel.DisplayColor;
			if (color.IsDark())
			{
				cssClass = cssClass + " Bright";
			}
			if (dataCellModel.IsLoggedIn) //Only print ready_time_m if logged in. (always 0 in db)
			{
				return MakeTableCell(dataCellModel.ReadyTime.ToString("0", CultureInfo.CurrentCulture),
						            HorizontalAlign.Center,
						            VerticalAlign.Middle, color.ToArgb(), cssClass,
									dataCellModel.CellToolTip);
			}
			return MakeTableCell("",
						        HorizontalAlign.Center,
						        VerticalAlign.Middle, color.ToArgb(), cssClass,
								dataCellModel.CellToolTip);
		}

		private void SetTeamTotals(PersonModel personModel)
		{
			if (_teamDeviationTotal == -2)
			{
				_teamAdherenceTotal = personModel.TeamAdherenceTotal;
				_teamDeviationTotal = personModel.TeamDeviationTotal;
			}
		}

		private TableRow MakeTableRow(PersonModel personModel)
		{
			var tableRow = new TableRow();
			tableRow.Cells.AddRange(GetReportDetailRowHeader(personModel));
			return tableRow;
		}

		private void EndRow(IList<TableRow> tableRowList, TableRow tableRow, List<TableCell> tableCellList, int previousIntervalId, PersonModel personModel)
		{
			if (tableRow != null && tableCellList != null)
			{
				tableRow.Cells.AddRange(tableCellList.ToArray());
				List<TableCell> fillCells;
				// If previous shift ends earlier than _timeLineEndInterval then we need to fill with some blanc cells
				if (personModel.EndsOnNextDate)
					fillCells = fillWithBlankCells(previousIntervalId + 1, _timeLineEndIntervalDayAfter);
				else
				{
					fillCells = fillWithBlankCells(previousIntervalId + 1, _timeLineEndInterval);
					fillCells.AddRange(fillWithBlankCells(0, _timeLineEndIntervalDayAfter));
				}
				tableRow.Cells.AddRange(fillCells.ToArray());
				tableRowList.Add(tableRow);
			}
		}

		public IntervalToolTip GetToolTip(int personId, int interval)
		{
			var toolTipList = _intervalToolTipDictionary[personId];
			var toolTip = toolTipList.FirstOrDefault(t => interval >= t.StartIntervalCounter &&
			                                              interval <= t.EndIntervalCounter);
			return toolTip;
		}

		public IntervalToolTip GetToolTip(DateTime date, int getIntervalId)
		{
			var toolTipList = _intervalDateToolTipDictionary[date];
			var toolTip = toolTipList.FirstOrDefault(t => getIntervalId >= t.StartIntervalCounter &&
			                                              getIntervalId <= t.EndIntervalCounter);
			return toolTip;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "nbsp"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Analytics.Portal.Reports.Ccc.report_agent_schedule_adherence.MakeTableCell(System.String,System.Web.UI.WebControls.HorizontalAlign,System.Web.UI.WebControls.VerticalAlign,System.String)")]
		private List<TableCell> fillWithBlankCells(int startInterval, int endInterval)
		{
			var tableCellList = new List<TableCell>();
			for (var interval = startInterval; interval < endInterval; interval++)
			{
				var tableCell = MakeTableCell("&nbsp;", HorizontalAlign.Center, VerticalAlign.Middle, "");

				if ((interval + 1) % _intervalsPerHour == 0)
					tableCell.Style.Add("border-right", "solid 2px silver");

				tableCellList.Add(tableCell);
			}

			return tableCellList;
		}

		private TableCell[] GetReportDetailRowHeader(PersonModel row)
		{
			var tableCellArray = new TableCell[3];

			var header = row.PersonName;
			if(ReportId.Equals(new Guid("6a3eb69b-690e-4605-b80e-46d5710b28af"))) //one agent per day
				header = row.DateText;

			tableCellArray[0] = MakeTableCell(header, HorizontalAlign.Left,
											 VerticalAlign.Middle, "");
			tableCellArray[0].Wrap = false;
			var totalAdherence = row.AdherenceTotal.HasValue ? ((decimal)(row.AdherenceTotal * 100)).ToString("0.0", CultureInfo.CurrentCulture) : string.Empty;
			tableCellArray[1] = MakeTableCell(totalAdherence, HorizontalAlign.Center ,VerticalAlign.Middle, "");
			var DeviationTotal = row.AdherenceTotal.HasValue ? ((decimal)(row.DeviationTotal)).ToString("0", CultureInfo.CurrentCulture) : string.Empty;
			tableCellArray[2] = MakeTableCell(DeviationTotal, HorizontalAlign.Center, VerticalAlign.Middle, "");
			return tableCellArray;
		}

		private static TableCell MakeTableCell(string text, HorizontalAlign horizontalAlign, VerticalAlign verticalAlign, string cssClass)
		{
			var tableCell = new TableCell
									  {
										  Text = text,
										  HorizontalAlign = horizontalAlign,
										  VerticalAlign = verticalAlign
									  };
			if (!String.IsNullOrEmpty(cssClass))
			{
				tableCell.CssClass = cssClass;
			}

			return tableCell;
		}
		private TableCell MakeTableCell(string text, HorizontalAlign horizontalAlign, VerticalAlign verticalAlign, int backColor, string cssClass, IntervalToolTip toolTip)
		{
			var tableCell = MakeTableCell(text, horizontalAlign, verticalAlign, cssClass);

			if (toolTip != null) tableCell.ToolTip = toolTip.ToolTip(_intervalsPerHour);
			tableCell.BackColor = Color.FromArgb(backColor);

			return tableCell;
		}

		private TableRow GetReportDetailHeaderRowWithTimeLineHour()
		{
			var tableRow = new TableRow { CssClass = "ReportColumnHeaders" };
			tableRow.Cells.Add(ReportId.Equals(new Guid("6a3eb69b-690e-4605-b80e-46d5710b28af"))
								   ? MakeTableCell(Analytics.ReportTexts.Resources.ResDate, HorizontalAlign.Left, VerticalAlign.Top, "") //one agent per days
				                   : MakeTableCell(Analytics.ReportTexts.Resources.ResAgentName, HorizontalAlign.Left, VerticalAlign.Top, "")); // one day per agents

			tableRow.Cells.Add(MakeTableCell(Analytics.ReportTexts.Resources.ResAdherencePercent, HorizontalAlign.Center, VerticalAlign.Top, ""));

			tableRow.Cells.Add(MakeTableCell(Analytics.ReportTexts.Resources.ResDeviationMinute, HorizontalAlign.Center, VerticalAlign.Top, ""));

			tableRow.Cells.AddRange(GetTimeLineHourCellArray(_timeLineStartIntervalDayBefore, _intervalsPerDay));
			tableRow.Cells.AddRange(GetTimeLineHourCellArray(_timeLineStartInterval, _timeLineEndInterval));
			tableRow.Cells.AddRange(GetTimeLineHourCellArray(0, _timeLineEndIntervalDayAfter));
			return tableRow;
		}

		private TableCell[] GetTimeLineHourCellArray(int start, int end)
		{
			var hourCount = (end - start) / _intervalsPerHour;
			var counter = 0;

			var cellArray = new TableCell[hourCount];

			for (var interval = start; interval < end; interval += _intervalsPerHour)
			{
				cellArray[counter] = GetTimeLineHourCell(interval / _intervalsPerHour, true);
				counter++;
			}

			return cellArray;
		}

		private TableCell[] GetTimeLineIntervalCellArray()
		{
			var cells = new List<TableCell>();
			for (var interval = 0; interval < (_intervalsPerDay - _timeLineStartIntervalDayBefore); interval++)
			{
				var cssClass = interval % 2 == 0 ? "ReportTimeLineIntervalCellOdd" : "ReportTimeLineIntervalCellEven";
				var tableCell = GetTimeLineIntervalCell((interval % _intervalsPerHour) * _intervalLength, cssClass);

				if (((interval + 1) % _intervalsPerHour == 0) && (interval > 0))
					tableCell.Style.Add("border-right", "solid 2px silver");

				cells.Add(tableCell);
			}
			for (var interval = 0; interval < (_timeLineEndInterval - _timeLineStartInterval); interval++)
			{
				var cssClass = interval % 2 == 0 ? "ReportTimeLineIntervalCellOdd" : "ReportTimeLineIntervalCellEven";
				var tableCell = GetTimeLineIntervalCell((interval % _intervalsPerHour) * _intervalLength, cssClass);

				if (((interval + 1) % _intervalsPerHour == 0) && (interval > 0) && (interval < (_timeLineEndInterval - _timeLineStartInterval)))
					tableCell.Style.Add("border-right", "solid 2px silver");

				cells.Add(tableCell);
			}
			for (var interval = 0; interval < (_timeLineEndIntervalDayAfter); interval++)
			{
				var cssClass = interval % 2 == 0 ? "ReportTimeLineIntervalCellOdd" : "ReportTimeLineIntervalCellEven";
				var tableCell = GetTimeLineIntervalCell((interval % _intervalsPerHour) * _intervalLength, cssClass);

				if (((interval + 1) % _intervalsPerHour == 0) && (interval > 0) && (interval < (_timeLineEndInterval - _timeLineStartInterval) - 1))
					tableCell.Style.Add("border-right", "solid 2px silver");

				cells.Add(tableCell);
			}
			return cells.ToArray();
		}

		private TableCell GetTimeLineHourCell(IFormattable hour, bool drawHourVerticalLine)
		{
			var cssClass = "";
			if (drawHourVerticalLine) cssClass = "ReportTimeLineHourVerticalLine";

			var tableCell = MakeTableCell(GetTimeName(hour, 0),
											   HorizontalAlign.Center, VerticalAlign.Top, cssClass);
			tableCell.ColumnSpan = _intervalsPerHour;

			return tableCell;
		}

		private static TableCell GetTimeLineIntervalCell(IFormattable minutePart, String cssClass)
		{
			var tableCell = MakeTableCell(minutePart.ToString("00", CultureInfo.InvariantCulture),
											   HorizontalAlign.Center, VerticalAlign.Middle, cssClass);

			return tableCell;
		}

		private static String GetTimeName(IFormattable hourPart, IFormattable minutePart)
		{
			return String.Concat(hourPart.ToString("00", CultureInfo.InvariantCulture), ":",
								 minutePart.ToString("00", CultureInfo.InvariantCulture));
		}

		private void setEarliestShiftStartAndLatestShiftEndPerDay()
		{
			// Also gather information about activity/absence layer periods for Tooltip usage.

			var previousDate = new DateTime();
			var previousActivityId = -2;
			var previousAbsenceId = -2;
			var previousIntervalId = -2;
			var previousIntervalCounter = -2;
			var dataDayAfterExists = false;
			var isNewDate = false;
			IntervalToolTip intervalToolTip = null;
			IList<IntervalToolTip> intervalToolTipList = null;//new List<IntervalToolTip>();
			_timeLineStartInterval = _intervalsPerDay;
			_timeLineStartIntervalDayBefore = _intervalsPerDay;

			foreach (DataRow row in _dataTable.Rows)
			{
				var dateIntervalBelongsTo = (DateTime)row["date"];
				var shiftStartDate = (DateTime)row["shift_startdate"];
				var intervalId = (int)row["interval_id"];
				var activityId = (int)row["activity_id"];
				var absenceId = (int)row["absence_id"];

				if (intervalId < _timeLineStartIntervalDayBefore && dateIntervalBelongsTo.IsEarlierThan(shiftStartDate))
					_timeLineStartIntervalDayBefore = intervalId;

				if (intervalId < _timeLineStartInterval && dateIntervalBelongsTo.Equals(shiftStartDate))
					_timeLineStartInterval = intervalId;

				if (intervalId > _timeLineEndInterval && dateIntervalBelongsTo.Equals(shiftStartDate))
					_timeLineEndInterval = intervalId;

				if (intervalId >= _timeLineEndIntervalDayAfter && dateIntervalBelongsTo.IsLaterThan(shiftStartDate))
				{
					dataDayAfterExists = true;
					_timeLineEndIntervalDayAfter = intervalId;
				}

				if (previousDate != shiftStartDate)
				{
					isNewDate = true;
					// Gather tooltip for each activity/absence layer
					if (intervalToolTip != null && intervalToolTipList != null)
					{
						intervalToolTip.EndInterval = previousIntervalId;
						intervalToolTip.EndIntervalCounter = previousIntervalCounter;
						intervalToolTipList.Add(intervalToolTip);
						if (!_intervalDateToolTipDictionary.ContainsKey(previousDate))
							_intervalDateToolTipDictionary.Add(previousDate, intervalToolTipList);
					}

					intervalToolTipList = new List<IntervalToolTip>();
					intervalToolTip = new IntervalToolTip
					{
						StartInterval = intervalId,
						StartIntervalCounter = ((int)row["date_interval_counter"]),
						AbsenceOrActivityName = ((String)row["activity_absence_name"])
					};
				}

				if (
					(!isNewDate)
					&&
					(
						((activityId == -1 && previousActivityId == activityId) && (absenceId == -1 && previousAbsenceId == absenceId))
						|| 
						((previousActivityId != activityId) || (previousAbsenceId != absenceId))
					)
					)
				{
					// We´re in the start of a new layer. Save the end interval of the previous layer 
					// and the start of the current layer into different tooltip objects.
					if (intervalToolTip != null && intervalToolTipList != null)
					{
						intervalToolTip.EndInterval = previousIntervalId;
						intervalToolTip.EndIntervalCounter = previousIntervalCounter;
						intervalToolTipList.Add(intervalToolTip);
					}

					intervalToolTip = new IntervalToolTip
											{
												StartInterval = intervalId,
												StartIntervalCounter = ((int)row["date_interval_counter"]),
												AbsenceOrActivityName = ((String)row["activity_absence_name"])
											};
				}

				previousDate = shiftStartDate;
				previousActivityId = activityId;
				previousAbsenceId = absenceId;
				previousIntervalId = intervalId;
				previousIntervalCounter = (int)row["date_interval_counter"];
				isNewDate = false;
			}

			// Catch the end of the last layer. Save the end interval of the last layer into tooltip object.
			if (intervalToolTip != null && intervalToolTipList != null)
			{
				intervalToolTip.EndInterval = previousIntervalId;
				intervalToolTip.EndIntervalCounter = previousIntervalCounter;
				intervalToolTipList.Add(intervalToolTip);
				if (!_intervalDateToolTipDictionary.ContainsKey(previousDate))
					_intervalDateToolTipDictionary.Add(previousDate, intervalToolTipList);
			}

			_timeLineEndInterval += 1;
			if (_timeLineStartInterval % _intervalsPerHour != 0)
				_timeLineStartInterval -= _timeLineStartInterval % _intervalsPerHour;

			if (_timeLineEndInterval % _intervalsPerHour != 0)
				_timeLineEndInterval += _intervalsPerHour - (_timeLineEndInterval % _intervalsPerHour);

			if (_timeLineStartIntervalDayBefore % _intervalsPerHour != 0)
				_timeLineStartIntervalDayBefore -= _timeLineStartIntervalDayBefore % _intervalsPerHour;

			if (dataDayAfterExists)
				_timeLineEndIntervalDayAfter += 1;

			if (_timeLineEndIntervalDayAfter % _intervalsPerHour != 0)
				_timeLineEndIntervalDayAfter += _intervalsPerHour - (_timeLineEndIntervalDayAfter % _intervalsPerHour);
		}

		private void SetEarliestShiftStartAndLatestShiftEnd()
		{
			// Also gather information about activity/absence layer periods for Tooltip usage.

			var previousPersonId = -2;
			var previousActivityId = -2;
			var previousAbsenceId = -2;
			var previousIntervalId = -2;
			var previousIntervalCounter = -2;
			var dataDayAfterExists = false;
			var isNewPerson = false;
			IntervalToolTip intervalToolTip = null;
			IList<IntervalToolTip> intervalToolTipList = null;
			_timeLineStartInterval = _intervalsPerDay;
			_timeLineStartIntervalDayBefore = _intervalsPerDay;

			foreach (DataRow row in _dataTable.Rows)
			{
				var dateIntervalBelongsTo = (DateTime) row["date"];
				var shiftStartDate = (DateTime) row["shift_startdate"];
				var intervalId = (int) row["interval_id"];
				var activityId = (int)row["activity_id"];
				var absenceId = (int)row["absence_id"];

				if (intervalId < _timeLineStartIntervalDayBefore && dateIntervalBelongsTo.IsEarlierThan(shiftStartDate))
					_timeLineStartIntervalDayBefore = intervalId;

				if (intervalId < _timeLineStartInterval && dateIntervalBelongsTo.Equals(shiftStartDate))
					_timeLineStartInterval = intervalId;

				if (intervalId > _timeLineEndInterval && dateIntervalBelongsTo.Equals(shiftStartDate))
					_timeLineEndInterval = intervalId;

				if (intervalId >= _timeLineEndIntervalDayAfter && dateIntervalBelongsTo.IsLaterThan(shiftStartDate))
				{
					dataDayAfterExists = true;
					_timeLineEndIntervalDayAfter = intervalId;
				}

				if (previousPersonId != (int)row["person_id"])
				{
					isNewPerson = true;
					// Gather tooltip for each activity/absence layer
					if (intervalToolTip != null && intervalToolTipList != null)
					{
						intervalToolTip.EndInterval = previousIntervalId;
						intervalToolTip.EndIntervalCounter = previousIntervalCounter;
						intervalToolTipList.Add(intervalToolTip);
						
						if (_intervalToolTipDictionary.ContainsKey(previousPersonId))
							foreach (var tooltip in intervalToolTipList)
								_intervalToolTipDictionary[previousPersonId].Add(tooltip);
						else
							_intervalToolTipDictionary.Add(previousPersonId, intervalToolTipList);
					}

					intervalToolTipList = new List<IntervalToolTip>();
					intervalToolTip = new IntervalToolTip
					{
						StartInterval = intervalId,
						StartIntervalCounter = ((int)row["date_interval_counter"]),
						AbsenceOrActivityName = ((String)row["activity_absence_name"])
					};
				}

				if (
					(!isNewPerson)
					&&
					(
						((activityId == -1 && previousActivityId == activityId) && (absenceId == -1 && previousAbsenceId == absenceId))
						||
						((previousActivityId != activityId) || (previousAbsenceId != absenceId))
					)
					)
				{
					// We´re in the start of a new layer. Save the end interval of the previous layer 
					// and the start of the current layer into different tooltip objects.
					if (intervalToolTip != null && intervalToolTipList != null)
					{
						intervalToolTip.EndInterval = previousIntervalId;
						intervalToolTip.EndIntervalCounter = previousIntervalCounter;
						intervalToolTipList.Add(intervalToolTip);
					}

					intervalToolTip = new IntervalToolTip
											{
												StartInterval = intervalId,
												StartIntervalCounter = ((int)row["date_interval_counter"]),
												AbsenceOrActivityName = ((String)row["activity_absence_name"])
											};
				}

				previousPersonId = (int)row["person_id"];
				previousActivityId = activityId;
				previousAbsenceId = absenceId;
				previousIntervalId = intervalId;
				previousIntervalCounter = (int)row["date_interval_counter"];
				isNewPerson = false;
			}

			// Catch the end of the last layer. Save the end interval of the last layer into tooltip object.
			if (intervalToolTip != null && intervalToolTipList != null)
			{
				intervalToolTip.EndInterval = previousIntervalId;
				intervalToolTip.EndIntervalCounter = previousIntervalCounter;
				intervalToolTipList.Add(intervalToolTip);

				if (_intervalToolTipDictionary.ContainsKey(previousPersonId))
					foreach (var tooltip in intervalToolTipList)
						_intervalToolTipDictionary[previousPersonId].Add(tooltip);
				else
					_intervalToolTipDictionary.Add(previousPersonId, intervalToolTipList);
			}

			// See to that the start and end variables begins and ends at whole hours
			_timeLineEndInterval += 1;
			if (_timeLineStartInterval % _intervalsPerHour != 0)
				_timeLineStartInterval -= _timeLineStartInterval%_intervalsPerHour;

			if (_timeLineEndInterval % _intervalsPerHour != 0)
				_timeLineEndInterval += _intervalsPerHour - (_timeLineEndInterval % _intervalsPerHour);

			if (_timeLineStartIntervalDayBefore % _intervalsPerHour != 0)
				_timeLineStartIntervalDayBefore -= _timeLineStartIntervalDayBefore % _intervalsPerHour;

			if (dataDayAfterExists)
				_timeLineEndIntervalDayAfter += 1;

			if (_timeLineEndIntervalDayAfter % _intervalsPerHour != 0)
				_timeLineEndIntervalDayAfter += _intervalsPerHour - (_timeLineEndIntervalDayAfter % _intervalsPerHour);
		}

		private void SetIntervalInformation()
		{
			_intervalsPerDay = (int)_dataTable.Rows[0]["intervals_per_day"];
			_intervalLength = 1440 / _intervalsPerDay;
			_intervalsPerHour = 60 / _intervalLength;
		}

		private void ChangeDateParameter(int dayCount)
		{
			CheckParametersCollection();
			dateOffset.Value = (Convert.ToInt32(dateOffset.Value) + dayCount).ToString();
			_sqlParameterList[0].Value = ((DateTime)_sqlParameterList[0].Value).AddDays(Convert.ToDouble(dateOffset.Value));
			_parameterTextList[0] = ((DateTime)_sqlParameterList[0].Value).ToShortDateString();

			CreateReport();
		}

		protected void imageButtonNextDay_Click(object sender, ImageClickEventArgs e)
		{
			ChangeDateParameter(1);
		}

		protected void imageButtonPreviousDay_Click(object sender, ImageClickEventArgs e)
		{
			ChangeDateParameter(-1);
		}

		protected void buttonShowTheReport(object sender, ImageClickEventArgs e)
		{
			if (!ParameterSelector.IsValid) return;
			CheckParametersCollection();
			try
			{
				dateOffset.Value = "0";
				CreateReport();
			}
			catch (SqlException exception)
			{
				//timeout?
				if (exception.Number == - 2)
				{
					labelError.Text = UserTexts.Resources.ReportTimeoutMessage;
					return;
				}
				labelError.Text = exception.Message;
			}
			
			hideSelection(null,new ImageClickEventArgs(0,0));
		}

		protected void Selector_OnInit(object sender, EventArgs e)
		{
			if (!Request.IsAuthenticated)
			{
				if (!Guid.TryParse(Request.QueryString["REPORTID"], out ReportId))
					return;
				Response.Redirect(string.Format("~/Reporting/Report/{0}#{1}", ReportId, ReportId));
			}
			var princip = Thread.CurrentPrincipal;
			var id = ((TeleoptiPrincipalCacheable)princip).Person.Id;
			var dataSource = ((TeleoptiIdentity)princip.Identity).DataSource;
			var bu = ((TeleoptiIdentity)princip.Identity).BusinessUnit.Id;

			ParameterSelector.ConnectionString = dataSource.Statistic.ConnectionString;
			ParameterSelector.UserCode = id.GetValueOrDefault();
			ParameterSelector.BusinessUnitCode = bu.GetValueOrDefault();
			ParameterSelector.LanguageId = ((TeleoptiPrincipalCacheable)princip).Person.PermissionInformation.UICulture().LCID;
			using (var commonReports = new CommonReports(ParameterSelector.ConnectionString, ParameterSelector.ReportId))
			{
				ParameterSelector.DbTimeout = commonReports.DbTimeout;
			}
		}

		protected void showSelection(object sender, ImageClickEventArgs e)
		{
			selectionPanel.Visible = true;
			buttonShowSelection.Visible = false;
			buttonHideSelection.Visible = true;

		}

		protected void hideSelection(object sender, ImageClickEventArgs e)
		{
			selectionPanel.Visible = false;
			buttonShowSelection.Visible = true;
			buttonHideSelection.Visible = false;

		}
	}
}
