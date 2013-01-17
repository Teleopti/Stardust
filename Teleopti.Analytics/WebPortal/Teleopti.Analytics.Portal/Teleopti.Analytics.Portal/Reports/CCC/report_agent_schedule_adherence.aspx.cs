﻿using System;
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
		private int _timeLineDayTwoEndInterval;
		private int _personCount;
		private Decimal _teamAdherenceTotal = -2;
		private Decimal _teamDeviationTotal = -2;
		//private Decimal[] _teamAdherenceArray;
		//private Decimal[] _teamDeviationArray;
		private IList<SqlParameter> _sqlParameterList;
		private IList<String> _parameterTextList;

		private IDictionary<int, IList<IntervalToolTip>> _intervalToolTipDictionary =
			new Dictionary<int, IList<IntervalToolTip>>();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private IDictionary<DateTime, IList<IntervalToolTip>> _intervalDateToolTipDictionary =
			new Dictionary<DateTime, IList<IntervalToolTip>>();

		SortedDictionary<int, SummaryData> _colSummary = new SortedDictionary<int, SummaryData>();
		class SummaryData
		{
			public decimal Adherence { get; set; }
			public decimal Deviation { get; set; }
			public int Interval { get; set; }
		}

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
			var aspTable = new Table {CssClass = "ReportTable"};
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
			var isParameterListsValid = false;
			_sqlParameterList = SessionParameters;
			_parameterTextList = SessionParameterTexts;

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
				tdReportName.InnerText = ReportTexts.Resources.ResReportAdherencePerAgent;
			else
				tdReportName.InnerText = ReportTexts.Resources.ResReportAdherencePerDay;

			tdDatesLabel.InnerText = ReportTexts.Resources.ResDateColon;
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

			// per agent
			if (ReportId.Equals(new Guid("6A3EB69B-690E-4605-B80E-46D5710B28AF")))
				DayButtons.Visible = false;
		}

		private TableRow[] GetIntervalTotalsRows()
		{
			//fill it if there are holes in it
			for (var i = _timeLineStartInterval; i < _timeLineEndInterval; i++)
			{
				if(!_colSummary.ContainsKey(i))
					_colSummary.Add(i, new SummaryData{Interval = i});
			}
			IList<TableRow> tableRowList = new List<TableRow>();

			var tableRowSpace = new TableRow();
			tableRowSpace.Cells.Add(MakeTableCell("", HorizontalAlign.Center, VerticalAlign.Middle, "ReportRowSpace"));
			tableRowList.Add(tableRowSpace);

			// Team adherence total row
			var tableCellListAdherence = new List<TableCell>
			                             	{
			                             		MakeTableCell(ReportTexts.Resources.ResAdherencePerIntervalPercent,
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
			                             		MakeTableCell(ReportTexts.Resources.ResDeviationPerIntervalMinute,
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

		private IEnumerable<TableCell> getIntervalTotalsCells(SortedDictionary<int,SummaryData> colSummary , bool isAdherence)
		{
			var tableCells = new List<TableCell>();

			foreach (var summaryData in colSummary)
			{
				string text;
				var cssClass = "ReportIntervalTotalDeviationCell";
				var interval = summaryData.Value.Interval;
				if (isAdherence)
				{
					text = (summaryData.Value.Adherence * 100).ToString("0", CultureInfo.CurrentCulture);
					cssClass = "ReportIntervalTotalAdherenceCell";
				}
				else
				{
					text = summaryData.Value.Deviation.ToString("0", CultureInfo.CurrentCulture);
				}
				
				var tableCell = MakeTableCell(text,
												   HorizontalAlign.Center, VerticalAlign.Middle, cssClass);
				if (((interval + 1) % _intervalsPerHour == 0) && (interval > 0) ) 
				{
					tableCell.Style.Add("border-right", "solid 2px silver");
				}

				tableCells.Add(tableCell);
			}

			return tableCells;
		}

		private TableRow GetReportTotalsRow(bool isTopTotals)
		{
			var tableRow = new TableRow();
			var cssClass = isTopTotals ? "ReportTotalsTop" : "ReportTotalsBottom";

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
				var tableCellColumnSpan = MakeTableCell("&nbsp;", HorizontalAlign.Center, VerticalAlign.Middle, cssClass);
				tableCellColumnSpan.ColumnSpan = _timeLineEndInterval - _timeLineStartInterval + _timeLineDayTwoEndInterval;
				tableRow.Cells.Add(tableCellColumnSpan);
			}

			return tableRow;
		}

		private TableRow[] GetReportDetailRows()
		{
			var perDate = ReportId.Equals(new Guid("6a3eb69b-690e-4605-b80e-46d5710b28af"));
			var tableRowList = MakeTableRowList();
			var dataRowReaders = from r in _dataTable.Rows.Cast<DataRow>() select new DataCellModel(r, this, perDate);
			//var dataPerPerson = from r in dataRowReaders group r by new PersonModel(r.DataRow, perDate);
			var dataPerPerson = dataRowReaders.GroupBy(r => new PersonModel(r.DataRow, perDate));
			dataPerPerson.ForEach((a, b) => ProcessPersonData(a, b, tableRowList));
			return tableRowList.ToArray();
		}

		private void ProcessPersonData(PersonModel personModel, IGrouping<PersonModel, DataCellModel> data, IList<TableRow> tableRowList)
		{
			var tableRow = MakeTableRow(personModel);

			var tableCellList = fillWithBlankCells(_timeLineStartInterval, personModel.FirstIntervalId);
			var previousIntervalId = personModel.FirstIntervalId - 1;

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
			//SetTeamAdherenceAndDeviation(dataCellModel);
			addColSummary(dataCellModel);
		}

		private IList<TableRow> MakeTableRowList() {
			IList<TableRow> tableRowList = new List<TableRow>();

			// Add a spacerow
			var tableRowSpace = new TableRow();
			var tableCellSpace = MakeTableCell("", HorizontalAlign.Center, VerticalAlign.Middle, "ReportRowSpace");
			tableCellSpace.Style.Add("border-top", "solid 2pt lightgrey");
			tableCellSpace.ColumnSpan = 3 + (_timeLineEndInterval - _timeLineStartInterval) + _timeLineDayTwoEndInterval;
			tableRowSpace.Cells.Add(tableCellSpace);
			tableRowList.Add(tableRowSpace);
			return tableRowList;
		}

		//private void PrepareTeamAdherenceAndDeviantion() {
		//    _teamAdherenceArray = new Decimal[_timeLineEndInterval - _timeLineStartInterval];
		//    _teamDeviationArray = new Decimal[_timeLineEndInterval - _timeLineStartInterval];
		//}

		//private void SetTeamAdherenceAndDeviation(DataCellModel dataCellModel)
		//{
		//    _teamAdherenceArray[dataCellModel.IntervalId - _timeLineStartInterval] = dataCellModel.TeamAdherence;
		//    _teamDeviationArray[dataCellModel.IntervalId - _timeLineStartInterval] = dataCellModel.TeamDeviation;
		//}

		private void addColSummary(DataCellModel dataCellModel)
		{
			var key = dataCellModel.IntervalId;
			if (dataCellModel.ShiftOverMidnight)
				key += 1000;

			if(!_colSummary.ContainsKey(key))
				_colSummary.Add(key, new SummaryData{Adherence = dataCellModel.TeamAdherence, Deviation = dataCellModel.TeamDeviation, Interval = dataCellModel.IntervalId});
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
				if(!personModel.EndsOnNextDate)
				{
					fillCells = fillWithBlankCells(previousIntervalId + 1, _timeLineEndInterval);
					fillCells.AddRange(fillWithBlankCells(0, _timeLineDayTwoEndInterval));
				}
				else
				{
					fillCells = fillWithBlankCells(previousIntervalId + 1, _timeLineDayTwoEndInterval);
				}
				tableRow.Cells.AddRange(fillCells.ToArray());
				tableRowList.Add(tableRow);
			}
		}

		public IntervalToolTip GetToolTip(int personId, int interval)
		{
			var toolTipList = _intervalToolTipDictionary[personId];
			var toolTip = toolTipList.Where(t => interval >= t.StartIntervalCounter &&
							   interval <= t.EndIntervalCounter).FirstOrDefault();
			return toolTip;
		}

		public IntervalToolTip GetToolTip(DateTime date, int getIntervalId)
		{
			var toolTipList = _intervalDateToolTipDictionary[date];
			var toolTip = toolTipList.Where(t => getIntervalId >= t.StartIntervalCounter &&
							   getIntervalId <= t.EndIntervalCounter).FirstOrDefault();
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
				header = row.Date;

			tableCellArray[0] = MakeTableCell(header, HorizontalAlign.Left,
											 VerticalAlign.Middle, "");
			tableCellArray[0].Wrap = false;
			tableCellArray[1] = MakeTableCell((row.AdherenceTotal * 100).ToString("0.0", CultureInfo.CurrentCulture), HorizontalAlign.Center ,
											 VerticalAlign.Middle, "");
			tableCellArray[2] = MakeTableCell(row.DeviationTotal.ToString("0", CultureInfo.CurrentCulture), HorizontalAlign.Center,
											 VerticalAlign.Middle, "");
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
			if(ReportId.Equals(new Guid("6a3eb69b-690e-4605-b80e-46d5710b28af"))) //one agent per day
				tableRow.Cells.Add(MakeTableCell(ReportTexts.Resources.ResDate,
											HorizontalAlign.Left, VerticalAlign.Top, ""));
			else
				tableRow.Cells.Add(MakeTableCell(ReportTexts.Resources.ResAgentName,
											HorizontalAlign.Left, VerticalAlign.Top, ""));	

			tableRow.Cells.Add(MakeTableCell(ReportTexts.Resources.ResAdherencePercent,
											HorizontalAlign.Center, VerticalAlign.Top, ""));

			tableRow.Cells.Add(MakeTableCell(ReportTexts.Resources.ResDeviationMinute,
											HorizontalAlign.Center, VerticalAlign.Top, ""));

			tableRow.Cells.AddRange(GetTimeLineHourCellArray(_timeLineStartInterval, _timeLineEndInterval));
			tableRow.Cells.AddRange(GetTimeLineHourCellArray(0, _timeLineDayTwoEndInterval));
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
			for (var interval = 0; interval < (_timeLineEndInterval - _timeLineStartInterval); interval++)
			{
				var cssClass = interval % 2 == 0 ? "ReportTimeLineIntervalCellOdd" : "ReportTimeLineIntervalCellEven";
				var tableCell = GetTimeLineIntervalCell((interval % _intervalsPerHour) * _intervalLength, cssClass);

				if (((interval + 1) % _intervalsPerHour == 0) && (interval > 0) && (interval < (_timeLineEndInterval - _timeLineStartInterval) - 1))
				{
					tableCell.Style.Add("border-right", "solid 2px silver");
				}
				cells.Add(tableCell);
			}
			for (var interval = 0; interval < (_timeLineDayTwoEndInterval - 0); interval++)
			{
				var cssClass = interval % 2 == 0 ? "ReportTimeLineIntervalCellOdd" : "ReportTimeLineIntervalCellEven";
				var tableCell = GetTimeLineIntervalCell((interval % _intervalsPerHour) * _intervalLength, cssClass);

				if (((interval + 1) % _intervalsPerHour == 0) && (interval > 0) && (interval < (_timeLineEndInterval - _timeLineStartInterval) - 1))
				{
					tableCell.Style.Add("border-right", "solid 2px silver");
				}
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
			IntervalToolTip intervalToolTip = null;
			IList<IntervalToolTip> intervalToolTipList = null;//new List<IntervalToolTip>();
			_timeLineStartInterval = _intervalsPerDay;

			foreach (DataRow row in _dataTable.Rows)
			{
				if ((int)row["interval_id"] < _timeLineStartInterval && ((DateTime)row["date"]).Equals((DateTime)row["shift_startdate"]))
				{
					_timeLineStartInterval = (int)row["interval_id"];
				}
				if ((int)row["interval_id"] > _timeLineEndInterval)
				{
					_timeLineEndInterval = (int)row["interval_id"];
				}
				if ((int)row["interval_id"] > _timeLineDayTwoEndInterval && !((DateTime)row["date"]).Equals((DateTime)row["shift_startdate"]))
				{
					_timeLineDayTwoEndInterval = (int)row["interval_id"];
				}

				if (previousDate != (DateTime)row["shift_startdate"])
				{
					// Count the nr of detail rows we will need
					_personCount++;

					// Gather tooltip for each activity/absence layer
					//if (intervalToolTip != null && intervalToolTipList != null && intervalToolTipList.Count > 0)
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
						StartInterval = ((int)row["interval_id"]),
						StartIntervalCounter = ((int)row["date_interval_counter"]),
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
						StartIntervalCounter = ((int)row["date_interval_counter"]),

						AbsenceOrActivityName = ((String)row["activity_absence_name"])
					};
				}

				previousDate = (DateTime)row["shift_startdate"];
				previousActivityId = (int)row["activity_id"];
				previousAbsenceId = (int)row["absence_id"];
				previousIntervalId = (int)row["interval_id"];
				previousIntervalCounter = (int)row["date_interval_counter"];
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
			// See to that the start and end variables begins and ends at whole hours
			if (_timeLineStartInterval % _intervalsPerHour != 0)
			{
				_timeLineStartInterval -= _timeLineStartInterval % _intervalsPerHour;
			}
			if (_timeLineEndInterval % _intervalsPerHour != 0)
			{
				_timeLineEndInterval += _intervalsPerHour - (_timeLineEndInterval % _intervalsPerHour);
			}
			if (_timeLineDayTwoEndInterval > 0)
				_timeLineDayTwoEndInterval += 1;
			if (_timeLineDayTwoEndInterval % _intervalsPerHour != 0)
			{
				_timeLineDayTwoEndInterval += _intervalsPerHour - (_timeLineDayTwoEndInterval % _intervalsPerHour);
			}
		}

		private void SetEarliestShiftStartAndLatestShiftEnd()
		{
			// Also gather information about activity/absence layer periods for Tooltip usage.

			var previousPersonId = -2;
			var previousActivityId = -2;
			var previousAbsenceId = -2;
			var previousIntervalId = -2;
			var previousIntervalCounter = -2;
			IntervalToolTip intervalToolTip = null;
			IList<IntervalToolTip> intervalToolTipList = null;//new List<IntervalToolTip>();
			_timeLineStartInterval = _intervalsPerDay;

			foreach (DataRow row in _dataTable.Rows)
			{
				if ((int)row["interval_id"] < _timeLineStartInterval && ((DateTime)row["date"]).Equals((DateTime)row["shift_startdate"]))
				{
					_timeLineStartInterval = (int)row["interval_id"];
				}
				if ((int)row["interval_id"] > _timeLineEndInterval)
				{
					_timeLineEndInterval = (int)row["interval_id"];
				}
				if ((int)row["interval_id"] > _timeLineDayTwoEndInterval && !((DateTime)row["date"]).Equals((DateTime)row["shift_startdate"]))
				{
					_timeLineDayTwoEndInterval = (int)row["interval_id"];
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
						intervalToolTip.EndIntervalCounter = previousIntervalCounter;
						intervalToolTipList.Add(intervalToolTip);
						_intervalToolTipDictionary.Add(previousPersonId, intervalToolTipList);
					}

					intervalToolTipList = new List<IntervalToolTip>();
					intervalToolTip = new IntervalToolTip
					{
						StartInterval = ((int)row["interval_id"]),
						StartIntervalCounter = ((int)row["date_interval_counter"]),
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
						intervalToolTip.EndIntervalCounter = previousIntervalCounter;
						intervalToolTipList.Add(intervalToolTip);
					}

					intervalToolTip = new IntervalToolTip
										  {
											  StartInterval = ((int)row["interval_id"]),
											  StartIntervalCounter = ((int)row["date_interval_counter"]),
											  AbsenceOrActivityName = ((String)row["activity_absence_name"])
										  };
				}

				previousPersonId = (int)row["person_id"];
				previousActivityId = (int)row["activity_id"];
				previousAbsenceId = (int)row["absence_id"];
				previousIntervalId = (int)row["interval_id"];
				previousIntervalCounter = (int)row["date_interval_counter"];
			}

			// Catch the end of the last layer. Save the end interval of the last layer into tooltip object.
			if (intervalToolTip != null && intervalToolTipList != null)
			{
				intervalToolTip.EndInterval = previousIntervalId;
				intervalToolTip.EndIntervalCounter = previousIntervalCounter;
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
			if(_timeLineDayTwoEndInterval > 0)
				_timeLineDayTwoEndInterval += 1;
			if (_timeLineDayTwoEndInterval % _intervalsPerHour != 0)
			{
				_timeLineDayTwoEndInterval += _intervalsPerHour - (_timeLineDayTwoEndInterval % _intervalsPerHour);
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
