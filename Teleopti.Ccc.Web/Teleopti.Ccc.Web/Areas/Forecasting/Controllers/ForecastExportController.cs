using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using NPOI.HSSF.UserModel;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Forecasting.Export.Web;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public class ForecastExportController : ApiController
	{
		private readonly ForecastExportModelCreator _forecastExportModelCreator;

		public ForecastExportController(ForecastExportModelCreator forecastExportModelCreator)
		{
			_forecastExportModelCreator = forecastExportModelCreator;
		}

		[HttpPost, Route("api/Forecasting/Export"), UnitOfWork]
		public virtual HttpResponseMessage Export(ExportForecastInput input)
		{
			var response = new HttpResponseMessage();
			var exportModel = _forecastExportModelCreator.Load(input.WorkloadId, new DateOnlyPeriod(new DateOnly(input.ForecastStart.Date), new DateOnly(input.ForecastEnd.Date)));
			
			XSSFWorkbook workbook = new XSSFWorkbook();
			CreateDailyForecastSheet(workbook, exportModel);

			using (var exportData = new MemoryStream())
			{
				workbook.Write(exportData);
				response.Content = new ByteArrayContent(exportData.ToArray());
				response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
				return response;
			}
		}

		private void CreateDailyForecastSheet(XSSFWorkbook workbook, ForecastExportModel exportModel)
		{

			var dailySheet = workbook.CreateSheet("ForecastedDays");
			var dateCellType = workbook.CreateCellStyle();
			dateCellType.DataFormat = HSSFDataFormat.GetBuiltinFormat("m/d/yy");

			fillSheetWithMainHeader(dailySheet, dateCellType, exportModel);
			fillSheetWithDailyHeader(dailySheet);
			fillSheetWithDailyVolumes(dailySheet, exportModel.DailyModelForecast, dateCellType);

		}

		private void fillSheetWithDailyVolumes(ISheet dailySheet, IList<ForecastExportDailyModel> dayModels, ICellStyle dateCellType)
		{
			var rowNumber = 10;
			foreach (var dailyForecast in dayModels)
			{
				var rowDailyForecast = dailySheet.CreateRow(rowNumber);
				var cellForecastDayValue = rowDailyForecast.CreateCell(0);
				var cellForecastCallsValue = rowDailyForecast.CreateCell(1);
				var cellForecastAttValue = rowDailyForecast.CreateCell(2);
				var cellForecastAcwValue = rowDailyForecast.CreateCell(3);
				var cellForecastAhtValue = rowDailyForecast.CreateCell(4);
				var cellForecastHoursValue = rowDailyForecast.CreateCell(5);
				var cellForecastHoursWithShrinkageValue = rowDailyForecast.CreateCell(6);

				cellForecastDayValue.CellStyle = dateCellType;
				cellForecastDayValue.SetCellValue(dailyForecast.ForecastDate);
				cellForecastCallsValue.SetCellValue(dailyForecast.Calls);
				cellForecastAttValue.SetCellValue(dailyForecast.AverageTalkTime);
				cellForecastAcwValue.SetCellValue(dailyForecast.AverageAfterCallWork);
				cellForecastAhtValue.SetCellValue(dailyForecast.AverageHandleTime);
				cellForecastHoursValue.SetCellValue(dailyForecast.ForecastedHours);
				cellForecastHoursWithShrinkageValue.SetCellValue(dailyForecast.ForecastedHoursShrinkage);
				rowNumber = rowNumber + 1;
			}
		}

		private void fillSheetWithDailyHeader(ISheet dailySheet)
		{
			var rowDailysHeader = dailySheet.CreateRow(9);
			var cellDailyDateHeader = rowDailysHeader.CreateCell(0);
			var cellDailyCallsHeader = rowDailysHeader.CreateCell(1);
			var cellDailyAttHeader = rowDailysHeader.CreateCell(2);
			var cellDailyAcwHeader = rowDailysHeader.CreateCell(3);
			var cellDailyAhtHeader = rowDailysHeader.CreateCell(4);
			var cellDailyHoursHeader = rowDailysHeader.CreateCell(5);
			var cellDailyHoursShrinkageHeader = rowDailysHeader.CreateCell(6);

			cellDailyDateHeader.SetCellValue("Date");
			cellDailyCallsHeader.SetCellValue("Calls");
			cellDailyAttHeader.SetCellValue("Average talk time(s)");
			cellDailyAcwHeader.SetCellValue("Average ACW (s)");
			cellDailyAhtHeader.SetCellValue("Average AHT (s)");
			cellDailyHoursHeader.SetCellValue("Hours");
			cellDailyHoursShrinkageHeader.SetCellValue("Hours with shrinkage");
		}

		private void fillSheetWithMainHeader(ISheet dailySheet, ICellStyle dateCellType, ForecastExportModel exportModel)
		{
			var rowPeriodStart = dailySheet.CreateRow(0);
			var rowPeriodEnd = dailySheet.CreateRow(1);
			var rowSkillName = dailySheet.CreateRow(2);
			var rowTimeZone = dailySheet.CreateRow(3);
			var rowServiceLevelPercent = dailySheet.CreateRow(4);
			var rowServiceLevelSeconds = dailySheet.CreateRow(5);
			var rowShrinkagePercent = dailySheet.CreateRow(6);
			var rowEmpty1 = dailySheet.CreateRow(7);
			var rowEmpty2 = dailySheet.CreateRow(8);

			var cellPeriodStartHeader = rowPeriodStart.CreateCell(0);
			var cellPeriodStartValue = rowPeriodStart.CreateCell(1);
			var cellPeriodEndHeader = rowPeriodEnd.CreateCell(0);
			var cellPeriodEndValue = rowPeriodEnd.CreateCell(1);
			var cellSkillNameHeader = rowSkillName.CreateCell(0);
			var cellSkillNameValue = rowSkillName.CreateCell(1);
			var cellTimeZoneHeader = rowTimeZone.CreateCell(0);
			var cellTimeZoneValue = rowTimeZone.CreateCell(1);
			var cellServiceLevelPercentHeader = rowServiceLevelPercent.CreateCell(0);
			var cellServiceLevelPercentValue = rowServiceLevelPercent.CreateCell(1);
			var cellServiceLevelSecondsHeader = rowServiceLevelSeconds.CreateCell(0);
			var cellServiceLevelSecondsValue = rowServiceLevelSeconds.CreateCell(1);
			var cellShrinkagePercentHeader = rowShrinkagePercent.CreateCell(0);
			var cellShrinkagePercentValue = rowShrinkagePercent.CreateCell(1);

			cellPeriodStartHeader.SetCellValue("Period start:");
			cellPeriodEndHeader.SetCellValue("Period end:");
			cellSkillNameHeader.SetCellValue("Skill name:");
			cellTimeZoneHeader.SetCellValue("Time zone:");
			cellServiceLevelPercentHeader.SetCellValue("Service level (%):");
			cellServiceLevelSecondsHeader.SetCellValue("Service level (s):");
			cellShrinkagePercentHeader.SetCellValue("Shrinkage (%):");
			cellPeriodStartValue.CellStyle = dateCellType;
			cellPeriodEndValue.CellStyle = dateCellType;
			cellPeriodStartValue.SetCellValue(exportModel.Header.Period.StartDate.Date);
			cellPeriodEndValue.SetCellValue(exportModel.Header.Period.EndDate.Date);
			cellSkillNameValue.SetCellValue(exportModel.Header.SkillName);
			cellTimeZoneValue.SetCellValue(exportModel.Header.SkillTimeZoneName);
			if (exportModel.Header.ServiceLevelPercent.HasValue)
				cellServiceLevelPercentValue.SetCellValue(exportModel.Header.ServiceLevelPercent.Value.Value);
			if (exportModel.Header.ServiceLevelSeconds.HasValue)
				cellServiceLevelSecondsValue.SetCellValue(exportModel.Header.ServiceLevelSeconds.Value);
			if (exportModel.Header.ShrinkagePercent.HasValue)
				cellShrinkagePercentValue.SetCellValue(exportModel.Header.ShrinkagePercent.Value.Value);
		}
	}
}