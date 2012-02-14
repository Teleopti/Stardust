using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Teleopti.Analytics.Portal.Utils;

namespace Teleopti.Analytics.Portal.Reports.Ccc
{
	public partial class report_agent_schedule_adherence : MatrixBasePage, ITooltipContainer
	{
		private DataTable _dataTable;
		private int _intervalsPerDay;
		private int _intervalsPerHour;
		private int _intervalLength;
		private int _timeLineStartInterval;
		private int _timeLineEndInterval;
		private int _personCount;
		private Decimal _teamAdherenceTotal = -2;
		private Decimal _teamDeviationTotal = -2;
		private Decimal[] _teamAdherenceArray;
		private Decimal[] _teamDeviationArray;
		private IList<SqlParameter> _sqlParameterList;
		private IList<String> _parameterTextList;

		private IDictionary<int, IList<IntervalToolTip>> _intervalToolTipDictionary =
			new Dictionary<int, IList<IntervalToolTip>>();


		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				CheckParametersCollection();
				CreateReport();
			}
		}

		private IList<SqlParameter> SessionParameters
		{
			get
			{
				if (!String.IsNullOrEmpty((Request.QueryString.Get("PARAMETERSKEY"))))
				{
					return (IList<SqlParameter>)Session["PARAMETERS" + Request.QueryString.Get("PARAMETERSKEY")];
				}

				return new List<SqlParameter>();
			}
			set
			{
				if (!String.IsNullOrEmpty((Request.QueryString.Get("PARAMETERSKEY"))))
				{
					Session["PARAMETERS" + Request.QueryString.Get("PARAMETERSKEY")] = value;
				}
			}
		}

		private IList<string> SessionParameterTexts
		{
			get
			{
				if (!String.IsNullOrEmpty((Request.QueryString.Get("PARAMETERSKEY"))))
				{
					return (IList<string>)Session["PARAMETERTEXTS" + Request.QueryString.Get("PARAMETERSKEY")];
				}

				return new List<string>();
			}
		}

		private void CreateReport()
		{
			SetReportHeaderParmaterLabels();
			SetReportHeaderParmaterTexts();
			HideDynamicParameters();
			
			if (GetReportData())
			{
				HideTimeZoneParameter(false);
				SetIntervalInformation();
				SetEarliestShiftStartAndLatestShiftEnd();
				CreateReportTable();
			}
			else
			{
				// Hide Time Zone parameter
				HideTimeZoneParameter(true);
			}
		}

		private void HideDynamicParameters()
		{
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
			CommonReports commonReports = new CommonReports(ConnectionString, ReportId);

			DataSet dataset = commonReports.GetReportData(ReportId, UserCode, BusinessUnitCode, _sqlParameterList);
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

		private void CreateReportTable()
		{
			//Table aspTable = new Table { BorderStyle = BorderStyle.Solid, BorderWidth = Unit.Pixel(1) };
			Table aspTable = new Table();
			aspTable.CssClass = "ReportTable";
			aspTable.CellPadding = 1;
			aspTable.Rows.Add(GetReportDetailHeaderRowWithTimeLineHour());
			aspTable.Rows.AddRange(GetReportDetailRows());
			aspTable.Rows.AddRange(GetIntervalTotalsRows());
			aspTable.Rows.AddAt(1, GetReportTotalsRow(true));
			aspTable.Rows.Add(GetReportTotalsRow(false));
			divReportTable.Controls.Clear();
			divReportTable.Controls.Add(aspTable);
		}

		private void CheckParametersCollection()
		{
			bool isParameterListsValid = false;
			_sqlParameterList = SessionParameters;
			_parameterTextList = SessionParameterTexts;

			if (_sqlParameterList != null && _parameterTextList != null)
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


			if (!isParameterListsValid)
			{
				Response.Write("xxThe report selection could not be obtained. Please try to make a new selection for the report.");
				Response.End();
			}
		}

		private void SetReportHeaderParmaterTexts()
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

		private void SetReportHeaderParmaterLabels()
		{
			tdReportName.InnerText = ReportTexts.Resources.ResReportAgentScheduleAdherence;

			tdGroupPageLabel.InnerText = ReportTexts.Resources.ResGroupPageColon;
			tdGroupPageGroupLabel.InnerText = ReportTexts.Resources.ResGroupPageGroupColon;
			tdGroupPageAgentLabel.InnerText = ReportTexts.Resources.ResAgentColon;

			tdSiteLabel.InnerText = ReportTexts.Resources.ResSiteNameColon;
			tdTeamLabel.InnerText = ReportTexts.Resources.ResTeamNameColon;
			tdAgentLabel.InnerText = ReportTexts.Resources.ResAgentColon;
			tdAdherenceCalculationLabel.InnerText = ReportTexts.Resources.ResAdherenceCalculationColon;
			tdSortOrderLabel.InnerText = ReportTexts.Resources.ResSortByColon;
			tdTimeZoneLabel.InnerText = ReportTexts.Resources.ResTimeZoneColon;
			tdDateLabel.InnerText = ReportTexts.Resources.ResDateColon;

			tdTodaysDateTime.InnerText = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
			imageButtonPreviousDay.ToolTip = ReportTexts.Resources.ResPrevious;
			imageButtonNextDay.ToolTip = ReportTexts.Resources.ResNext;
		}

		private TableRow[] GetIntervalTotalsRows()
		{
			IList<TableRow> tableRowList = new List<TableRow>();

			TableRow tableRowSpace = new TableRow();
			//tableRowSpace.Cells.Add(GetTableCell("&nbsp;", false, HorizontalAlign.Center, VerticalAlign.Middle, "ReportRowSpace"));
			tableRowSpace.Cells.Add(MakeTableCell("", HorizontalAlign.Center, VerticalAlign.Middle, "ReportRowSpace"));
			tableRowList.Add(tableRowSpace);

			// Team adherence total row
			List<TableCell> tableCellListAdherence = new List<TableCell>();
			tableCellListAdherence.Add(MakeTableCell(ReportTexts.Resources.ResAdherencePerIntervalPercent,
													HorizontalAlign.Left, VerticalAlign.Middle, "ReportTotalAdherence"));
			//tableCellListAdherence.Add(GetTableCell(_teamAdherenceTotal.ToString("0", CultureInfo.CurrentCulture), true,
			//                                        HorizontalAlign.Center,
			//                                        VerticalAlign.Middle, "ReportTotalAdherence"));
			tableCellListAdherence.Add(MakeTableCell("&nbsp;",
													HorizontalAlign.Center,
													VerticalAlign.Middle, "ReportTotalAdherence"));
			tableCellListAdherence.Add(MakeTableCell("&nbsp;", HorizontalAlign.Center, VerticalAlign.Middle, "ReportTotalAdherence"));
			tableCellListAdherence.AddRange(GetIntervalTotalsCells(_teamAdherenceArray, true));
			TableRow tableRowAdherence = new TableRow();
			tableRowAdherence.Cells.AddRange(tableCellListAdherence.ToArray());
			tableRowList.Add(tableRowAdherence);

			// Team deviation total row
			List<TableCell> tableCellListDeviation = new List<TableCell>();
			tableCellListDeviation.Add(MakeTableCell(ReportTexts.Resources.ResDeviationPerIntervalMinute,
													HorizontalAlign.Left, VerticalAlign.Middle, "ReportTotalDeviation"));
			tableCellListDeviation.Add(MakeTableCell("&nbsp;", HorizontalAlign.Center, VerticalAlign.Middle, "ReportTotalDeviation"));
			//tableCellListDeviation.Add(GetTableCell(_teamDeviationTotal.ToString("0", CultureInfo.CurrentCulture), true,
			//                                        HorizontalAlign.Center, VerticalAlign.Middle, "ReportTotalDeviation")); 
			tableCellListDeviation.Add(MakeTableCell("&nbsp;",
													HorizontalAlign.Center, VerticalAlign.Middle, "ReportTotalDeviation"));
			tableCellListDeviation.AddRange(GetIntervalTotalsCells(_teamDeviationArray, false));
			TableRow tableRowDeviation = new TableRow();
			tableRowDeviation.Cells.AddRange(tableCellListDeviation.ToArray());
			tableRowList.Add(tableRowDeviation);

			return tableRowList.ToArray();
		}

		private TableCell[] GetIntervalTotalsCells(Decimal[] teamIntervalArray, bool isAdherence)
		{
			TableCell[] tableCells = new TableCell[teamIntervalArray.GetUpperBound(0) + 1];

			for (int interval = 0; interval <= tableCells.GetUpperBound(0); interval++)
			{
				string text;
				string cssClass = "ReportIntervalTotalDeviationCell";

				if (isAdherence)
				{
					text = (teamIntervalArray[interval] * 100).ToString("0", CultureInfo.CurrentCulture);
					cssClass = "ReportIntervalTotalAdherenceCell";
				}
				else
				{
					text = teamIntervalArray[interval].ToString("0", CultureInfo.CurrentCulture);
				}
				
				TableCell tableCell = MakeTableCell(text,
												   HorizontalAlign.Center, VerticalAlign.Middle, cssClass);
				if (((interval + 1) % _intervalsPerHour == 0) && (interval > 0) && (interval < (_timeLineEndInterval - _timeLineStartInterval) - 1))
				{
					tableCell.Style.Add("border-right", "solid 2px silver");
				}

				tableCells[interval] = tableCell;
			}

			return tableCells;
		}

		private TableRow GetReportTotalsRow(bool isTopTotals)
		{
			TableRow tableRow = new TableRow();
			String cssClass = isTopTotals ? "ReportTotalsTop" : "ReportTotalsBottom";

			tableRow.Cells.Add(MakeTableCell(ReportTexts.Resources.ResTotalsColon, HorizontalAlign.Left,
											VerticalAlign.Bottom, cssClass));
			tableRow.Cells.Add(MakeTableCell((_teamAdherenceTotal * 100).ToString("0.0", CultureInfo.CurrentCulture),
													HorizontalAlign.Center,
													VerticalAlign.Middle, cssClass));
			tableRow.Cells.Add(MakeTableCell(_teamDeviationTotal.ToString("0", CultureInfo.CurrentCulture),
											HorizontalAlign.Center, VerticalAlign.Middle, cssClass));

			if (isTopTotals)
			{
				tableRow.Cells.AddRange(GetTimeLineIntervalCellArray());
			}
			else
			{
				TableCell tableCellColumnSpan = MakeTableCell("&nbsp;", HorizontalAlign.Center, VerticalAlign.Middle, cssClass);
				tableCellColumnSpan.ColumnSpan = _timeLineEndInterval - _timeLineStartInterval;
				tableRow.Cells.Add(tableCellColumnSpan);
			}

			return tableRow;
		}

		private TableRow[] GetReportDetailRows()
		{
			PrepareTeamAdherenceAndDeviantion();
			var tableRowList = MakeTableRowList();
			var dataRowReaders = from r in _dataTable.Rows.Cast<DataRow>() select new DataCellModel(r, this);
			var dataPerPerson = from r in dataRowReaders group r by new PersonModel(r.DataRow);
			dataPerPerson.ForEach((a, b) => ProcessPersonData(a, b, tableRowList));
			return tableRowList.ToArray();
		}

		private void ProcessPersonData(PersonModel personModel, IGrouping<PersonModel, DataCellModel> data, IList<TableRow> tableRowList)
		{
			var tableRow = MakeTableRow(personModel);

			var tableCellList = FillWithBlancCells(_timeLineStartInterval, personModel.FirstIntervalId);
			var previousIntervalId = personModel.FirstIntervalId - 1;

			SetTeamTotals(personModel);

			data.ForEach(m => ProcessCellData(m, ref previousIntervalId, tableCellList));
			
			EndRow(tableRowList, tableRow, tableCellList, previousIntervalId);
		}

		private void ProcessCellData(DataCellModel dataCellModel, ref int previousIntervalId, List<TableCell> tableCellList)
		{
			if ((previousIntervalId + 1) != dataCellModel.IntervalId)
			{
				var tableCellBlancList = FillWithBlancCells(previousIntervalId + 1, dataCellModel.IntervalId);
				tableCellList.AddRange(tableCellBlancList);
			}
			previousIntervalId = dataCellModel.IntervalId;

			var tableCell = MakeTableCell(dataCellModel);
			StyleTableCellBorders(dataCellModel, tableCell);
			tableCellList.Add(tableCell);

			// Get team interval sum for adherence and deviation
			SetTeamAdherenceAndDeviation(dataCellModel);
		}

		private IList<TableRow> MakeTableRowList() {
			IList<TableRow> tableRowList = new List<TableRow>();

			// Add a spacerow
			TableRow tableRowSpace = new TableRow();
			TableCell tableCellSpace = MakeTableCell("", HorizontalAlign.Center, VerticalAlign.Middle, "ReportRowSpace");
			tableCellSpace.Style.Add("border-top", "solid 2pt lightgrey");
			tableCellSpace.ColumnSpan = 3 + (_timeLineEndInterval - _timeLineStartInterval);
			tableRowSpace.Cells.Add(tableCellSpace);
			tableRowList.Add(tableRowSpace);
			return tableRowList;
		}

		private void PrepareTeamAdherenceAndDeviantion() {
			_teamAdherenceArray = new Decimal[_timeLineEndInterval - _timeLineStartInterval];
			_teamDeviationArray = new Decimal[_timeLineEndInterval - _timeLineStartInterval];
		}

		private void SetTeamAdherenceAndDeviation(DataCellModel dataCellModel)
		{
			_teamAdherenceArray[dataCellModel.IntervalId - _timeLineStartInterval] = dataCellModel.TeamAdherence;
			_teamDeviationArray[dataCellModel.IntervalId - _timeLineStartInterval] = dataCellModel.TeamDeviation;
		}

		private void StyleTableCellBorders(DataCellModel dataCellModel, TableCell tableCell)
		{
			if (dataCellModel.IntervalId > _timeLineStartInterval && dataCellModel.IntervalId < (_timeLineEndInterval - 1))
			{
				if ((dataCellModel.IntervalId + 1) % _intervalsPerHour == 0)
					tableCell.Style.Add("border-right", "solid 2px silver");
			}
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

		private void EndRow(IList<TableRow> tableRowList, TableRow tableRow, List<TableCell> tableCellList, int previousIntervalId)
		{
			if (tableRow != null && tableCellList != null)
			{
				tableRow.Cells.AddRange(tableCellList.ToArray());
				// If previous shift ends earlier than _timeLineEndInterval then we need to fill with some blanc cells
				var fillCells = FillWithBlancCells(previousIntervalId + 1, _timeLineEndInterval);
				tableRow.Cells.AddRange(fillCells.ToArray());
				tableRowList.Add(tableRow);
			}
		}

		public IntervalToolTip GetToolTip(int personId, int interval)
		{
			IList<IntervalToolTip> toolTipList = _intervalToolTipDictionary[personId];
			var toolTip = toolTipList.Where(t => interval >= t.StartInterval &&
							   interval <= t.EndInterval).FirstOrDefault();
			return toolTip;
		}

		private List<TableCell> FillWithBlancCells(int startInterval, int endInterval)
		{
			List<TableCell> tableCellList = new List<TableCell>();
			for (int interval = startInterval; interval < endInterval; interval++)
			{
				TableCell tableCell = MakeTableCell("&nbsp;", HorizontalAlign.Center, VerticalAlign.Middle, "");

				if (interval > _timeLineStartInterval && interval < (_timeLineEndInterval - 1))
				{
					if ((interval + 1) % _intervalsPerHour == 0)
						tableCell.Style.Add("border-right", "solid 2px silver");
				}

				tableCellList.Add(tableCell);
			}

			return tableCellList;
		}

		private static TableCell[] GetReportDetailRowHeader(PersonModel row)
		{
			TableCell[] tableCellArray = new TableCell[3];

			tableCellArray[0] = MakeTableCell(row.PersonName, HorizontalAlign.Left,
											 VerticalAlign.Middle, "");
			tableCellArray[0].Wrap = false;
			tableCellArray[1] = MakeTableCell((row.AdherenceTotal * 100).ToString("0.0", CultureInfo.CurrentCulture), HorizontalAlign.Center ,
											 VerticalAlign.Middle, "");
			tableCellArray[2] = MakeTableCell(row.DeviationTotal.ToString("0", CultureInfo.CurrentCulture), HorizontalAlign.Center,
											 VerticalAlign.Middle, "");
			return tableCellArray;
		}

		private static Decimal ParseDecimal(object rowValue)
		{
			if (rowValue == DBNull.Value)
			{
				return 0;
			}
			return (Decimal)rowValue;
		}

		//private static String FormatToPercent(Object toFormat)
		//{
		//    return String.Format("{0:0%}", toFormat);
		//}

		private static TableCell MakeTableCell(string text, HorizontalAlign horizontalAlign, VerticalAlign verticalAlign, string cssClass)
		{
			TableCell tableCell = new TableCell
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
			TableCell tableCell = MakeTableCell(text, horizontalAlign, verticalAlign, cssClass);

			if (toolTip != null) tableCell.ToolTip = toolTip.ToolTip(_intervalsPerHour);
			tableCell.BackColor = Color.FromArgb(backColor);

			return tableCell;
		}

		private TableRow GetReportDetailHeaderRowWithTimeLineHour()
		{
			TableRow tableRow = new TableRow { CssClass = "ReportColumnHeaders" };
			tableRow.Cells.Add(MakeTableCell(ReportTexts.Resources.ResAgentName,
											HorizontalAlign.Left, VerticalAlign.Top, ""));

			tableRow.Cells.Add(MakeTableCell(ReportTexts.Resources.ResAdherencePercent,
											HorizontalAlign.Center, VerticalAlign.Top, ""));

			tableRow.Cells.Add(MakeTableCell(ReportTexts.Resources.ResDeviationMinute,
											HorizontalAlign.Center, VerticalAlign.Top, ""));

			tableRow.Cells.AddRange(GetTimeLineHourCellArray());

			return tableRow;
		}

		//private TableRow GetTimeLineInterval()
		//{
		//    TableRow tableRow = new TableRow();
		//    tableRow.Cells.AddRange(GetTimeLineIntervalCellArray());

		//    return tableRow;
		//}

		private TableCell[] GetTimeLineHourCellArray()
		{
			int hourCount = (_timeLineEndInterval - _timeLineStartInterval) / _intervalsPerHour;
			int counter = 0;

			TableCell[] cellArray = new TableCell[hourCount];

			for (int interval = _timeLineStartInterval; interval < _timeLineEndInterval; interval += _intervalsPerHour)
			{
				bool drawHourVerticalLine = (interval < _timeLineEndInterval - _intervalsPerHour) ? true : false;

				cellArray[counter] = GetTimeLineHourCell(interval / _intervalsPerHour, drawHourVerticalLine);
				counter++;
			}

			return cellArray;
		}

		private TableCell[] GetTimeLineIntervalCellArray()
		{
			TableCell[] cellArray = new TableCell[_timeLineEndInterval - _timeLineStartInterval];

			for (int interval = 0; interval < (_timeLineEndInterval - _timeLineStartInterval); interval++)
			{
				String cssClass;
				if (interval % 2 == 0)
				{
					cssClass = "ReportTimeLineIntervalCellOdd";
				}
				else
				{
					cssClass = "ReportTimeLineIntervalCellEven";
				}
				TableCell tableCell = GetTimeLineIntervalCell((interval % _intervalsPerHour) * _intervalLength, cssClass);

				if (((interval + 1) % _intervalsPerHour == 0) && (interval > 0) && (interval < (_timeLineEndInterval - _timeLineStartInterval) - 1))
				{
					tableCell.Style.Add("border-right", "solid 2px silver");
				}

				cellArray[interval] = tableCell;
			}

			return cellArray;
		}

		private TableCell GetTimeLineHourCell(IFormattable hour, bool drawHourVerticalLine)
		{
			String cssClass = "";
			if (drawHourVerticalLine) cssClass = "ReportTimeLineHourVerticalLine";

			TableCell tableCell = MakeTableCell(GetTimeName(hour, 0),
											   HorizontalAlign.Center, VerticalAlign.Top, cssClass);
			tableCell.ColumnSpan = _intervalsPerHour;

			return tableCell;
		}

		private static TableCell GetTimeLineIntervalCell(IFormattable minutePart, String cssClass)
		{
			TableCell tableCell = MakeTableCell(minutePart.ToString("00", CultureInfo.InvariantCulture),
											   HorizontalAlign.Center, VerticalAlign.Middle, cssClass);

			return tableCell;
		}

		private static String GetTimeName(IFormattable hourPart, IFormattable minutePart)
		{
			return String.Concat(hourPart.ToString("00", CultureInfo.InvariantCulture), ":",
								 minutePart.ToString("00", CultureInfo.InvariantCulture));
		}

		private void SetEarliestShiftStartAndLatestShiftEnd()
		{
			// Also gather information about activity/absence layer periods for Tooltip usage.

			int previousPersonId = -2;
			int previousActivityId = -2;
			int previousAbsenceId = -2;
			int previousIntervalId = -2;
			IntervalToolTip intervalToolTip = null;
			IList<IntervalToolTip> intervalToolTipList = null;//new List<IntervalToolTip>();
			_timeLineStartInterval = _intervalsPerDay;

			foreach (DataRow row in _dataTable.Rows)
			{
				if ((int)row["interval_id"] < _timeLineStartInterval)
				{
					_timeLineStartInterval = (int)row["interval_id"];
				}
				if ((int)row["interval_id"] > _timeLineEndInterval)
				{
					_timeLineEndInterval = (int)row["interval_id"];
				}

				if (previousPersonId != (int)row["person_id"])
				{
					// Count the nr of detail rows we will need
					_personCount++;

					// Gather tooltip for each activity/absence layer
					//if (intervalToolTip != null && intervalToolTipList != null && intervalToolTipList.Count > 0)
					if (intervalToolTip != null && intervalToolTipList != null)
					{
						intervalToolTip.EndInterval = previousIntervalId;
						intervalToolTipList.Add(intervalToolTip);
						_intervalToolTipDictionary.Add(previousPersonId, intervalToolTipList);
					}

					intervalToolTipList = new List<IntervalToolTip>();
					intervalToolTip = new IntervalToolTip
					{
						StartInterval = ((int)row["interval_id"]),
						AbsenceOrActivityName = ((String)row["activity_absence_name"])
					};

					previousActivityId = (int)row["activity_id"];
					previousAbsenceId = (int)row["absence_id"];
				}

				if ((previousActivityId != (int)row["activity_id"]) || (previousAbsenceId != (int)row["absence_id"]))
				{
					// We´re in the start of a new layer. Save the end interval of the previous layer 
					// and the start of the current layer into different tooltip objects.
					if (intervalToolTip != null && intervalToolTipList != null)
					{
						intervalToolTip.EndInterval = previousIntervalId;
						intervalToolTipList.Add(intervalToolTip);
					}

					intervalToolTip = new IntervalToolTip
										  {
											  StartInterval = ((int)row["interval_id"]),
											  AbsenceOrActivityName = ((String)row["activity_absence_name"])
										  };
				}

				previousPersonId = (int)row["person_id"];
				previousActivityId = (int)row["activity_id"];
				previousAbsenceId = (int)row["absence_id"];
				previousIntervalId = (int)row["interval_id"];
			}

			// Catch the end of the last layer. Save the end interval of the last layer into tooltip object.
			if (intervalToolTip != null && intervalToolTipList != null)
			{
				intervalToolTip.EndInterval = previousIntervalId;
				intervalToolTipList.Add(intervalToolTip);
				_intervalToolTipDictionary.Add(previousPersonId, intervalToolTipList);
			}

			_timeLineEndInterval += 1;
			// See to that the start and end variables begins and ends at whole hours
			if (_timeLineStartInterval % _intervalsPerHour != 0)
			{
				_timeLineStartInterval -= _timeLineStartInterval % _intervalsPerHour;
			}
			if (_timeLineEndInterval % _intervalsPerHour != 0)
			{
				_timeLineEndInterval += _intervalsPerHour - (_timeLineEndInterval % _intervalsPerHour);
			}
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

			_sqlParameterList[0].Value = ((DateTime)_sqlParameterList[0].Value).AddDays(dayCount);
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
	}
}
