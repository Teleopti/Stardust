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
		private decimal? _teamAdherenceTotal = -2;
		private decimal? _teamDeviationTotal = -2;
		private IList<SqlParameter> _sqlParameterList;
		private IList<string> _parameterTextList;
		private static readonly Guid reportAdherencePerAgentGuid = new Guid("6a3eb69b-690e-4605-b80e-46d5710b28af");

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
					string groupPageComboBoxControlCollectionIdName = $"ParameterSelector$Drop{groupPageComboBoxControlCollectionId}";

					GroupPageCode = string.IsNullOrEmpty(Request.Form.Get(groupPageComboBoxControlCollectionIdName))
						? Selector.BusinessHierarchyCode
						: new Guid(Request.Form.Get(groupPageComboBoxControlCollectionIdName));

					if (!string.IsNullOrEmpty(Request.Form.Get("lastGroupPage")))
						GroupPageCode = new Guid(Request.Form.Get("lastGroupPage"));

					ParameterSelector.GroupPageCode = GroupPageCode;
					commonReports.LoadReportInfo();
					Page.Header.Title = commonReports.Name;
				}
			}
		}

		protected override void OnLoadComplete(EventArgs e)
		{
			base.OnLoadComplete(e);
			if (ReportId.Equals(reportAdherencePerAgentGuid))
			{
				buttonHideSelection.Visible = false;
				buttonShowSelection.Visible = false;
			}
			buttonShowReport.Enabled = ParameterSelector.IsValid;
		}

		private void createReport()
		{
			setReportHeaderParmaterLabels();
			setReportHeaderParmaterTexts();
			hideDynamicParameters();
			lastGroupPage.Value = GroupPageCode.ToString();
			if (getReportData())
			{
				hideTimeZoneParameter(false);
				setIntervalInformation();
				if (ReportId.Equals(reportAdherencePerAgentGuid)) //one agent per day
					setEarliestShiftStartAndLatestShiftEndPerDay();
				else
					setEarliestShiftStartAndLatestShiftEnd();
				createReportTable();
			}
			else
			{
				// Hide Time Zone parameter
				hideTimeZoneParameter(true);
			}
			reportData.Visible = true;
		}

		private void hideDynamicParameters()
		{
			if (ReportId.Equals(reportAdherencePerAgentGuid)) //one agent per day
			{
				if (_sqlParameterList[3].Value == DBNull.Value) //& _sqlParameterList[4].Value == DBNull.Value)
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

		private void hideTimeZoneParameter(bool hide)
		{
			// Check if time zone will be hidden or not
			if (hide || (bool)_dataTable.Rows[0]["hide_time_zone"])
			{
				trTimeZoneParameter.Style.Add("display", "none");
			}
		}

		private bool getReportData()
		{
			using (var commonReports = new CommonReports(ParameterSelector.ConnectionString, ReportId))
			{
				tdTodaysDateTime.InnerText = commonReports.GetReportPullDate(_sqlParameterList, ParameterSelector.UserTimeZone);
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

		private void createReportTable()
		{
			var aspTable = new Table { CssClass = "ReportTable" };
			divReportTable.Controls.Clear();
			divReportTable.Controls.Add(aspTable);
			aspTable.Rows.Add(getReportDetailHeaderRowWithTimeLineHour());
			aspTable.Rows.AddRange(getReportDetailRows());
			aspTable.Rows.AddRange(getIntervalTotalsRows());
			aspTable.Rows.AddAt(1, getReportTotalsRow(true));
			aspTable.Rows.Add(getReportTotalsRow(false));
		}

		private void checkParametersCollection()
		{
			var isParameterListsValid = false;
			_sqlParameterList = ParameterSelector.Parameters;
			_parameterTextList = ParameterSelector.ParameterTexts;

			if (_sqlParameterList != null && _parameterTextList != null)
			{
				if (ReportId.Equals(reportAdherencePerAgentGuid)) //one agent per day
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

		private void setReportHeaderParmaterTexts()
		{
			if (ReportId.Equals(reportAdherencePerAgentGuid)) //one agent per day
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

		private void setReportHeaderParmaterLabels()
		{
			tdReportName.InnerText = ReportId.Equals(reportAdherencePerAgentGuid) ? Resources.ResReportReadyTimeAdherencePerAgent : Resources.ResReportReadyTimeAdherencePerDay;

			tdDatesLabel.InnerText = Resources.ResShiftStartDateColon;
			tdGroupPageLabel.InnerText = Resources.ResGroupPageColon;
			tdGroupPageGroupLabel.InnerText = Resources.ResGroupPageGroupColon;
			tdGroupPageAgentLabel.InnerText = Resources.ResAgentColon;

			tdSiteLabel.InnerText = Resources.ResSiteNameColon;
			tdTeamLabel.InnerText = Resources.ResTeamNameColon;
			tdAgentLabel.InnerText = Resources.ResAgentColon;
			tdAdherenceCalculationLabel.InnerText = Resources.ResReadyTimeAdherenceCalculationColon;
			tdSortOrderLabel.InnerText = Resources.ResSortByColon;
			tdTimeZoneLabel.InnerText = Resources.ResTimeZoneColon;
			tdDateLabel.InnerText = Resources.ResShiftStartDateColon;

			imageButtonPreviousDay.ToolTip = Resources.ResPrevious;
			imageButtonNextDay.ToolTip = Resources.ResNext;

			// per agent
			if (ReportId.Equals(reportAdherencePerAgentGuid))
				DayButtons.Visible = false;
		}

		private TableRow[] getIntervalTotalsRows()
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
				if (!_colSummary.ContainsKey(interval))
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
			tableRowSpace.Cells.Add(makeTableCell("", HorizontalAlign.Center, VerticalAlign.Middle, "ReportRowSpace"));
			tableRowList.Add(tableRowSpace);

			// Team adherence total row
			var tableCellListAdherence = new List<TableCell>
											 {
												 makeTableCell(Resources.ResReadyTimeAdherencePerIntervalPercent,
															   HorizontalAlign.Left, VerticalAlign.Middle, "ReportTotalAdherence"),
												 makeTableCell("&nbsp;",
															   HorizontalAlign.Center,
															   VerticalAlign.Middle, "ReportTotalAdherence"),
												 makeTableCell("&nbsp;", HorizontalAlign.Center, VerticalAlign.Middle,
															   "ReportTotalAdherence")
											 };

			tableCellListAdherence.AddRange(getIntervalTotalsCells(_colSummary, true));
			var tableRowAdherence = new TableRow();
			tableRowAdherence.Cells.AddRange(tableCellListAdherence.ToArray());
			tableRowList.Add(tableRowAdherence);

			// Team deviation total row
			var tableCellListDeviation = new List<TableCell>
										 {
												 makeTableCell(Resources.ResDeviationPerIntervalMinute,
															   HorizontalAlign.Left, VerticalAlign.Middle, "ReportTotalDeviation"),
												 makeTableCell("&nbsp;", HorizontalAlign.Center, VerticalAlign.Middle,
															   "ReportTotalDeviation"),
												 makeTableCell("&nbsp;",
															   HorizontalAlign.Center, VerticalAlign.Middle, "ReportTotalDeviation")
											 };

			tableCellListDeviation.AddRange(getIntervalTotalsCells(_colSummary, false));
			var tableRowDeviation = new TableRow();
			tableRowDeviation.Cells.AddRange(tableCellListDeviation.ToArray());
			tableRowList.Add(tableRowDeviation);

			return tableRowList.ToArray();
		}

		private IEnumerable<TableCell> getIntervalTotalsCells(SortedDictionary<int, summaryData> colSummary, bool isAdherence)
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

				var tableCell = makeTableCell(text, HorizontalAlign.Center, VerticalAlign.Middle, cssClass);
				var interval = summaryData.Value.Interval;
				if ((interval + 1) % _intervalsPerHour == 0)
					tableCell.Style.Add("border-right", "solid 2px silver");

				tableCells.Add(tableCell);
			}

			return tableCells;
		}

		private TableRow getReportTotalsRow(bool isTopTotals)
		{
			var tableRow = new TableRow();
			var cssClass = isTopTotals ? "ReportTotalsTop" : "ReportTotalsBottom";

			tableRow.Cells.Add(makeTableCell(Resources.ResTotalsColon, HorizontalAlign.Left,
											VerticalAlign.Bottom, cssClass));

			var teamAdherenceTotal = (_teamAdherenceTotal * 100)?.ToString("0.0", CultureInfo.CurrentCulture) ?? string.Empty;
			tableRow.Cells.Add(makeTableCell(teamAdherenceTotal, HorizontalAlign.Center, VerticalAlign.Middle, cssClass));
			var teamDeviationTotal = _teamDeviationTotal?.ToString("0", CultureInfo.CurrentCulture) ?? string.Empty;
			tableRow.Cells.Add(makeTableCell(teamDeviationTotal, HorizontalAlign.Center, VerticalAlign.Middle, cssClass));

			if (isTopTotals)
			{
				tableRow.Cells.AddRange(getTimeLineIntervalCellArray());
			}
			else
			{
				var tableCellColumnSpan = makeTableCell("&nbsp;", HorizontalAlign.Center, VerticalAlign.Middle, cssClass);
				tableCellColumnSpan.ColumnSpan = _intervalsPerDay - _timeLineStartIntervalDayBefore + (_timeLineEndInterval - _timeLineStartInterval) + _timeLineEndIntervalDayAfter;
				tableRow.Cells.Add(tableCellColumnSpan);
			}

			return tableRow;
		}

		private TableRow[] getReportDetailRows()
		{
			var perDate = ReportId.Equals(reportAdherencePerAgentGuid);
			var tableRowList = makeTableRowList();
			var dataRowReaders = from r in _dataTable.Rows.Cast<DataRow>() select new DataCellModel(r, this, perDate);
			var dataPerPerson = dataRowReaders.GroupBy(r => new PersonModel(r.DataRow, perDate));
			dataPerPerson.ForEach((a, b) => processPersonData(a, b, tableRowList));
			return tableRowList.ToArray();
		}

		private void processPersonData(PersonModel personModel, IGrouping<PersonModel, DataCellModel> data, IList<TableRow> tableRowList)
		{
			var tableRow = makeTableRow(personModel);

            var previousIntervalId = personModel.FirstIntervalId - 1;
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

			setTeamTotals(personModel);

			data.ForEach(m => processCellData(m, ref previousIntervalId, tableCellList, personModel));

			endRow(tableRowList, tableRow, tableCellList, previousIntervalId, personModel);
		}

		private void processCellData(DataCellModel dataCellModel, ref int previousIntervalId, List<TableCell> tableCellList, PersonModel personModel)
		{
			if ((previousIntervalId + 1) != dataCellModel.IntervalId)
			{
				var tableCellBlancList = fillWithBlankCells(previousIntervalId + 1, dataCellModel.IntervalId);
				tableCellList.AddRange(tableCellBlancList);
			}
			if (dataCellModel.ShiftOverMidnight)
				personModel.EndsOnNextDate = true;
			previousIntervalId = dataCellModel.IntervalId;

			var tableCell = makeTableCell(dataCellModel);
			styleTableCellBorders(dataCellModel, tableCell);
			tableCellList.Add(tableCell);

			// Get team interval sum for adherence and deviation
			addColSummary(dataCellModel);
		}

		private IList<TableRow> makeTableRowList()
		{
			IList<TableRow> tableRowList = new List<TableRow>();

			// Add a spacerow
			var tableRowSpace = new TableRow();
			var tableCellSpace = makeTableCell("", HorizontalAlign.Center, VerticalAlign.Middle, "ReportRowSpace");
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

			if (!_colSummary.ContainsKey(key))
				_colSummary.Add(key, new summaryData { Adherence = dataCellModel.TeamAdherence, Deviation = dataCellModel.TeamDeviation, Interval = dataCellModel.IntervalId });
		}


		private void styleTableCellBorders(DataCellModel dataCellModel, TableCell tableCell)
		{
			if ((dataCellModel.IntervalId + 1) % _intervalsPerHour == 0)
				tableCell.Style.Add("border-right", "solid 2px silver");
		}

		private TableCell makeTableCell(DataCellModel dataCellModel)
		{
			var cssClass = "ReportIntervalCell";
			if (dataCellModel.HasDisplayColor && dataCellModel.DisplayColor.IsDark())
			{
				cssClass = cssClass + " Bright";
			}
			else if (!dataCellModel.HasDisplayColor)
			{
				cssClass = cssClass + " MultipleActivities";
			}

			//Only print ready_time_m if IsLoggedIn. (always 0 in db)
			var text = dataCellModel.IsLoggedIn ? dataCellModel.ReadyTime.ToString("0", CultureInfo.CurrentCulture) : "";
			return makeTableCell(text,
								HorizontalAlign.Center,
								VerticalAlign.Middle, dataCellModel.HasDisplayColor ? dataCellModel.DisplayColor : (Color?) null, cssClass,
								dataCellModel.CellToolTip);
		}

		private void setTeamTotals(PersonModel personModel)
		{
			if (_teamDeviationTotal == -2)
			{
				_teamAdherenceTotal = personModel.TeamAdherenceTotal;
				_teamDeviationTotal = personModel.TeamDeviationTotal;
			}
		}

		private TableRow makeTableRow(PersonModel personModel)
		{
			var tableRow = new TableRow();
			tableRow.Cells.AddRange(getReportDetailRowHeader(personModel));
			return tableRow;
		}

		private void endRow(IList<TableRow> tableRowList, TableRow tableRow, List<TableCell> tableCellList, int previousIntervalId, PersonModel personModel)
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
            if (startInterval > endInterval) { startInterval = startInterval - _intervalsPerDay; }
            var tableCellList = new List<TableCell>();
			for (var interval = startInterval; interval < endInterval; interval++)
			{
				var tableCell = makeTableCell("&nbsp;", HorizontalAlign.Center, VerticalAlign.Middle, "");

				if ((interval + 1) % _intervalsPerHour == 0)
					tableCell.Style.Add("border-right", "solid 2px silver");

				tableCellList.Add(tableCell);
			}

			return tableCellList;
		}

		private TableCell[] getReportDetailRowHeader(PersonModel row)
		{
			var tableCellArray = new TableCell[3];

			var header = row.PersonName;
			if (ReportId.Equals(reportAdherencePerAgentGuid)) //one agent per day
				header = row.DateText;

			tableCellArray[0] = makeTableCell(header, HorizontalAlign.Left,
											 VerticalAlign.Middle, "");
			tableCellArray[0].Wrap = false;
			var totalAdherence = row.AdherenceTotal.HasValue ? ((decimal)(row.AdherenceTotal * 100)).ToString("0.0", CultureInfo.CurrentCulture) : string.Empty;
			tableCellArray[1] = makeTableCell(totalAdherence, HorizontalAlign.Center, VerticalAlign.Middle, "");
			var deviationTotal = row.AdherenceTotal.HasValue && row.DeviationTotal.HasValue ? ((decimal)row.DeviationTotal).ToString("0", CultureInfo.CurrentCulture) : string.Empty;
			tableCellArray[2] = makeTableCell(deviationTotal, HorizontalAlign.Center, VerticalAlign.Middle, "");
			return tableCellArray;
		}

		private static TableCell makeTableCell(string text, HorizontalAlign horizontalAlign, VerticalAlign verticalAlign, string cssClass)
		{
			var tableCell = new TableCell
			{
				Text = text,
				HorizontalAlign = horizontalAlign,
				VerticalAlign = verticalAlign
			};
			if (!string.IsNullOrEmpty(cssClass))
			{
				tableCell.CssClass = cssClass;
			}

			return tableCell;
		}

		private TableCell makeTableCell(string text, HorizontalAlign horizontalAlign, VerticalAlign verticalAlign, Color? backColor, string cssClass, IntervalToolTip toolTip)
		{
			var tableCell = makeTableCell(text, horizontalAlign, verticalAlign, cssClass);

			if (toolTip != null) tableCell.ToolTip = toolTip.ToolTip(_intervalsPerHour);

			if (backColor != null)
				tableCell.BackColor = backColor.Value;

			return tableCell;
		}

		private TableRow getReportDetailHeaderRowWithTimeLineHour()
		{
			var tableRow = new TableRow { CssClass = "ReportColumnHeaders" };
			tableRow.Cells.Add(ReportId.Equals(reportAdherencePerAgentGuid)
								   ? makeTableCell(Resources.ResDate, HorizontalAlign.Left, VerticalAlign.Top, "") //one agent per days
								   : makeTableCell(Resources.ResAgentName, HorizontalAlign.Left, VerticalAlign.Top, "")); // one day per agents

			tableRow.Cells.Add(makeTableCell(Resources.ResReadyTimeAdherencePercent, HorizontalAlign.Center, VerticalAlign.Top, ""));

			tableRow.Cells.Add(makeTableCell(Resources.ResDeviationMinute, HorizontalAlign.Center, VerticalAlign.Top, ""));

			tableRow.Cells.AddRange(getTimeLineHourCellArray(_timeLineStartIntervalDayBefore, _intervalsPerDay));
			tableRow.Cells.AddRange(getTimeLineHourCellArray(_timeLineStartInterval, _timeLineEndInterval));
			tableRow.Cells.AddRange(getTimeLineHourCellArray(0, _timeLineEndIntervalDayAfter));
			return tableRow;
		}

		private TableCell[] getTimeLineHourCellArray(int start, int end)
		{
			var hourCount = (end - start) / _intervalsPerHour;
			var counter = 0;

			var cellArray = new TableCell[hourCount];

			for (var interval = start; interval < end; interval += _intervalsPerHour)
			{
				cellArray[counter] = getTimeLineHourCell(interval / _intervalsPerHour, true);
				counter++;
			}

			return cellArray;
		}

		private TableCell[] getTimeLineIntervalCellArray()
		{
			var cells = new List<TableCell>();
			for (var interval = 0; interval < (_intervalsPerDay - _timeLineStartIntervalDayBefore); interval++)
			{
				var cssClass = interval % 2 == 0 ? "ReportTimeLineIntervalCellOdd" : "ReportTimeLineIntervalCellEven";
				var tableCell = getTimeLineIntervalCell((interval % _intervalsPerHour) * _intervalLength, cssClass);

				if (((interval + 1) % _intervalsPerHour == 0) && (interval > 0))
					tableCell.Style.Add("border-right", "solid 2px silver");

				cells.Add(tableCell);
			}
			for (var interval = 0; interval < (_timeLineEndInterval - _timeLineStartInterval); interval++)
			{
				var cssClass = interval % 2 == 0 ? "ReportTimeLineIntervalCellOdd" : "ReportTimeLineIntervalCellEven";
				var tableCell = getTimeLineIntervalCell((interval % _intervalsPerHour) * _intervalLength, cssClass);

				if (((interval + 1) % _intervalsPerHour == 0) && (interval > 0) && (interval < (_timeLineEndInterval - _timeLineStartInterval)))
					tableCell.Style.Add("border-right", "solid 2px silver");

				cells.Add(tableCell);
			}
			for (var interval = 0; interval < (_timeLineEndIntervalDayAfter); interval++)
			{
				var cssClass = interval % 2 == 0 ? "ReportTimeLineIntervalCellOdd" : "ReportTimeLineIntervalCellEven";
				var tableCell = getTimeLineIntervalCell((interval % _intervalsPerHour) * _intervalLength, cssClass);

				if (((interval + 1) % _intervalsPerHour == 0) && (interval > 0) && (interval < (_timeLineEndInterval - _timeLineStartInterval) - 1))
					tableCell.Style.Add("border-right", "solid 2px silver");

				cells.Add(tableCell);
			}
			return cells.ToArray();
		}

		private TableCell getTimeLineHourCell(IFormattable hour, bool drawHourVerticalLine)
		{
			var cssClass = "";
			if (drawHourVerticalLine) cssClass = "ReportTimeLineHourVerticalLine";

			var tableCell = makeTableCell(getTimeName(hour, 0),
											   HorizontalAlign.Center, VerticalAlign.Top, cssClass);
			tableCell.ColumnSpan = _intervalsPerHour;

			return tableCell;
		}

		private static TableCell getTimeLineIntervalCell(IFormattable minutePart, string cssClass)
		{
			var tableCell = makeTableCell(minutePart.ToString("00", CultureInfo.InvariantCulture),
											   HorizontalAlign.Center, VerticalAlign.Middle, cssClass);

			return tableCell;
		}

		private static string getTimeName(IFormattable hourPart, IFormattable minutePart)
		{
			return string.Concat(hourPart.ToString("00", CultureInfo.InvariantCulture), ":",
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
						StartIntervalCounter = (int)row["date_interval_counter"],
						AbsenceOrActivityName = (string)row["activity_absence_name"]
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
						AbsenceOrActivityName = ((string)row["activity_absence_name"])
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

		private void setEarliestShiftStartAndLatestShiftEnd()
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
				var dateIntervalBelongsTo = (DateTime)row["date"];
				var shiftStartDate = (DateTime)row["shift_startdate"];
				var intervalId = (int)row["interval_id"];
				var activityId = (int)row["activity_id"];
				var absenceId = (int)row["absence_id"];
                var activityAbsenceName = (string) row["activity_absence_name"];
                
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

						if (_intervalToolTipDictionary.TryGetValue(previousPersonId, out var temp))
							foreach (var tooltip in intervalToolTipList)
								temp.Add(tooltip);
						else
							_intervalToolTipDictionary.Add(previousPersonId, intervalToolTipList);
					}

					intervalToolTipList = new List<IntervalToolTip>();
					intervalToolTip = new IntervalToolTip
					{
						StartInterval = intervalId,
						StartIntervalCounter = (int)row["date_interval_counter"],
						AbsenceOrActivityName = activityId == -1 && absenceId == -1 && activityAbsenceName == "Multiple things" ? Resources.MultipleActivitiesOrAbsences : (string)row["activity_absence_name"]
					};
				}

				if (
					!isNewPerson
					&&
					((activityId == -1 && previousActivityId == activityId) && (absenceId == -1 && previousAbsenceId == absenceId) || (previousActivityId != activityId) || (previousAbsenceId != absenceId))
					)
				{
					// We are in the start of a new layer. Save the end interval of the previous layer 
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
						StartIntervalCounter = (int)row["date_interval_counter"],
						AbsenceOrActivityName = activityId == -1 && absenceId == -1 && activityAbsenceName == "Multiple things" ? Resources.MultipleActivitiesOrAbsences : (string)row["activity_absence_name"]
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

		private void setIntervalInformation()
		{
			_intervalsPerDay = (int)_dataTable.Rows[0]["intervals_per_day"];
			_intervalLength = 1440 / _intervalsPerDay;
			_intervalsPerHour = 60 / _intervalLength;
		}

		private void changeDateParameter(int dayCount)
		{
			checkParametersCollection();
			var daysOffset = Convert.ToInt32(dateOffset.Value) + dayCount;
			dateOffset.Value = daysOffset.ToString();
			var dateTime = ((DateTime)_sqlParameterList[0].Value).AddDays(daysOffset);
			_sqlParameterList[0].Value = dateTime;
			_parameterTextList[0] = dateTime.ToShortDateString();

			createReport();
		}

		protected void ImageButtonNextDayClick(object sender, ImageClickEventArgs e)
		{
			changeDateParameter(1);
		}

		protected void ImageButtonPreviousDayClick(object sender, ImageClickEventArgs e)
		{
			changeDateParameter(-1);
		}

		protected void ButtonShowTheReport(object sender, ImageClickEventArgs e)
		{
			if (!ParameterSelector.IsValid) return;
			checkParametersCollection();
			try
			{
				dateOffset.Value = "0";
				createReport();
			}
			catch (SqlException exception)
			{
				//timeout?
				if (exception.Number == -2)
				{
					labelError.Text = UserTexts.Resources.ReportTimeoutMessage;
					return;
				}
				labelError.Text = exception.Message;
			}

			HideSelection(null, new ImageClickEventArgs(0, 0));
		}

		protected void Selector_OnInit(object sender, EventArgs e)
		{
			if (!Request.IsAuthenticated)
			{
				if (!Guid.TryParse(Request.QueryString["REPORTID"], out ReportId))
					return;
				Response.Redirect($"~/Reporting/Report/{ReportId}#{ReportId}");
			}
			var princip = (TeleoptiPrincipal)Thread.CurrentPrincipal;
			var teleoptiIdentity = (TeleoptiIdentity)princip.Identity;
			var uiCulture = princip.Regional.UICulture;
			var timeZone = princip.Regional.TimeZone;
			Guid? id = princip.PersonId;
			var dataSource = teleoptiIdentity.DataSource;
			var bu = teleoptiIdentity.BusinessUnitId;

			ParameterSelector.ConnectionString = dataSource.Analytics.ConnectionString;
			ParameterSelector.UserCode = id.GetValueOrDefault();
			ParameterSelector.BusinessUnitCode = bu.GetValueOrDefault();
			ParameterSelector.LanguageId = uiCulture.LCID;
			ParameterSelector.UserTimeZone = timeZone;
			using (var commonReports = new CommonReports(ParameterSelector.ConnectionString, ParameterSelector.ReportId))
			{
				ParameterSelector.DbTimeout = commonReports.DbTimeout;
			}
		}

		protected void ShowSelection(object sender, ImageClickEventArgs e)
		{
			selectionPanel.Visible = true;
			buttonShowSelection.Visible = false;
			buttonHideSelection.Visible = true;

		}

		protected void HideSelection(object sender, ImageClickEventArgs e)
		{
			selectionPanel.Visible = false;
			buttonShowSelection.Visible = true;
			buttonHideSelection.Visible = false;

		}
	}
}
