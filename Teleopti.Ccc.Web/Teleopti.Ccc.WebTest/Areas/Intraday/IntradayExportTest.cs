using System;
using System.IO;
using System.Linq;
using NPOI.XSSF.UserModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer.ViewModels;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.WebTest.Areas.Intraday
{
	[DomainTest]
	public class IntradayExportTest
	{
		[Test]
		public void ShouldExportIntradayDataToExcel()
		{
			var intradayExportDataToExcel = new IntradayExportCreator();
			var dataToExport = createExportData();
			var byteArray = intradayExportDataToExcel.ExportDataToExcel(dataToExport);
			
			var workbook = new XSSFWorkbook(new MemoryStream(byteArray));
			var sheet = workbook.GetSheetAt(0);

			sheet.GetRow(0).Cells[1].DateCellValue.ToString("yyyy-MM-dd").Should().Be.EqualTo(dataToExport.Date.ToString("yyyy-MM-dd"));
			sheet.GetRow(1).Cells[1].StringCellValue.Should().Be.EqualTo(dataToExport.SkillAreaName);
			sheet.GetRow(2).Cells[1].StringCellValue.Should().Be.EqualTo(string.Join("; ", dataToExport.Skills));
			
			//Summary
			var dataRowSummary = sheet.GetRow(5);
			dataRowSummary.Cells[1].NumericCellValue.Should().Be.EqualTo(dataToExport.IncomingViewModel.Summary.ForecastedCalls);
			dataRowSummary.Cells[2].NumericCellValue.Should().Be.EqualTo(dataToExport.IncomingViewModel.Summary.CalculatedCalls);
			dataRowSummary.Cells[3].NumericCellValue.ToString("#.##").Should().Be.EqualTo((dataToExport.IncomingViewModel.Summary.ForecastedActualCallsDiff / 100).ToString("#.##"));
			dataRowSummary.Cells[4].NumericCellValue.Should().Be.EqualTo(dataToExport.IncomingViewModel.Summary.ForecastedAverageHandleTime);
			dataRowSummary.Cells[5].NumericCellValue.Should().Be.EqualTo(dataToExport.IncomingViewModel.Summary.AverageHandleTime);
			dataRowSummary.Cells[6].NumericCellValue.ToString("#.##").Should().Be.EqualTo((dataToExport.IncomingViewModel.Summary.ForecastedActualHandleTimeDiff / 100).ToString("#.##"));
			dataRowSummary.Cells[7].NumericCellValue.Should().Be.EqualTo(dataToExport.PerformanceViewModel.Summary.ServiceLevel);

			dataRowSummary.Cells[8].NumericCellValue.Should().Be.EqualTo(dataToExport.PerformanceViewModel.Summary.EstimatedServiceLevel / 100);
			dataRowSummary.Cells[9].NumericCellValue.Should().Be.EqualTo(dataToExport.PerformanceViewModel.Summary.AbandonRate);
			dataRowSummary.Cells[10].NumericCellValue.Should().Be.EqualTo(dataToExport.PerformanceViewModel.Summary.AverageSpeedOfAnswer);
			dataRowSummary.Cells[11].NumericCellValue.Should().Be.EqualTo(dataToExport.StaffingViewModel.DataSeries.ForecastedStaffing.Sum());
			dataRowSummary.Cells[12].NumericCellValue.Should().Be.EqualTo(dataToExport.StaffingViewModel.DataSeries.ActualStaffing.Sum());
			dataRowSummary.Cells[13].NumericCellValue.Should().Be.EqualTo(dataToExport.StaffingViewModel.DataSeries.ScheduledStaffing.Sum());
			dataRowSummary.Cells[14].NumericCellValue.Should().Be.EqualTo(dataToExport.StaffingViewModel.DataSeries.UpdatedForecastedStaffing.Sum());

			//Row
			var row = sheet.GetRow(6);
			row.Cells[1].NumericCellValue.Should().Be.EqualTo(dataToExport.IncomingViewModel.DataSeries.ForecastedCalls[0]);
			row.Cells[2].NumericCellValue.Should().Be.EqualTo(dataToExport.IncomingViewModel.DataSeries.CalculatedCalls[0]);

			var actualForecastedCallsDiff =
				(dataToExport.IncomingViewModel.DataSeries.CalculatedCalls[0] -
				 dataToExport.IncomingViewModel.DataSeries.ForecastedCalls[0]) /
				dataToExport.IncomingViewModel.DataSeries.ForecastedCalls[0];

			var actualForecastedAhtDiff =
				(dataToExport.IncomingViewModel.DataSeries.AverageHandleTime[0] -
				 dataToExport.IncomingViewModel.DataSeries.ForecastedAverageHandleTime[0]) /
				dataToExport.IncomingViewModel.DataSeries.ForecastedAverageHandleTime[0];

			row.Cells[3].NumericCellValue.ToString("#.##").Should().Be.EqualTo(actualForecastedCallsDiff.Value.ToString("#.##"));
			row.Cells[4].NumericCellValue.Should().Be.EqualTo(dataToExport.IncomingViewModel.DataSeries.ForecastedAverageHandleTime[0]);
			row.Cells[5].NumericCellValue.Should().Be.EqualTo(dataToExport.IncomingViewModel.DataSeries.AverageHandleTime[0]);
			row.Cells[6].NumericCellValue.ToString("#.##").Should().Be.EqualTo(actualForecastedAhtDiff.Value.ToString("#.##"));
			row.Cells[7].NumericCellValue.Should().Be.EqualTo(dataToExport.PerformanceViewModel.DataSeries.ServiceLevel[0] / 100);

			row.Cells[8].NumericCellValue.Should().Be.EqualTo(dataToExport.PerformanceViewModel.DataSeries.EstimatedServiceLevels[0] / 100);
			row.Cells[9].NumericCellValue.Should().Be.EqualTo(dataToExport.PerformanceViewModel.DataSeries.AbandonedRate[0] / 100);
			row.Cells[10].NumericCellValue.Should().Be.EqualTo(dataToExport.PerformanceViewModel.DataSeries.AverageSpeedOfAnswer[0]);
			row.Cells[11].NumericCellValue.Should().Be.EqualTo(dataToExport.StaffingViewModel.DataSeries.ForecastedStaffing[0]);
			row.Cells[12].NumericCellValue.Should().Be.EqualTo(dataToExport.StaffingViewModel.DataSeries.ActualStaffing[0]);
			row.Cells[13].NumericCellValue.Should().Be.EqualTo(dataToExport.StaffingViewModel.DataSeries.ScheduledStaffing[0]);
			row.Cells[14].NumericCellValue.Should().Be.EqualTo(dataToExport.StaffingViewModel.DataSeries.UpdatedForecastedStaffing[0]);


		}

		private IntradayExcelExport createExportData()
		{
			var dataToExport = new IntradayExcelExport();

			//Data setup
			DateTime[] timeList = { DateTime.Parse("2017-08-15 02:00:00"), DateTime.Parse("2017-08-15 02:15:00") };

			//IntradayIncomingDataSeries
			double[] forecastedCalls = { 0.41, 0.37 }; //Forecasted volume
			double?[] calculatedCalls = { 1, 2 }; //Actual volume
			double[] forecastedAverageHandleTime = { 149.6, 138.5 }; //Forecasted AHT
			double?[] averageHandleTime = { 149, 138.5 }; //Actual AHT

			//IntradayPerformanceDataSeries
			double?[] serviceLevel = { 1, 1 };
			double?[] estimatedServiceLevels = { 1, 1 };
			double?[] abandonedRate = { 1, 1 };
			double?[] averageSpeedOfAnswer = { 8, 24 };


			//StaffingDataSeries
			double?[] forecastedStaffing = { 0.18, 0.17 }; //Forecasted agents
			double?[] actualStaffing = { 0.18, 0.34 }; //Required agents
			double?[] scheduledStaffing = { 7, 7 };//Scheduled agents
			double?[] updatedForecastedStaffing = { 22.70, 40.66 };//Reforecasted agents

			dataToExport.Date = DateTime.Now;
			dataToExport.SkillAreaName = "Phone";
			dataToExport.Skills = new[] { "Channel Support", "Direct Support" };
			dataToExport.PerformanceViewModel = new IntradayPerformanceViewModel
			{
				DataSeries = new IntradayPerformanceDataSeries
				{
					Time = timeList,
					AbandonedRate = abandonedRate,
					AverageSpeedOfAnswer = averageSpeedOfAnswer,
					EstimatedServiceLevels = estimatedServiceLevels,
					ServiceLevel = serviceLevel
				},
				Summary = new IntradayPerformanceSummary()
				{
					AbandonRate = abandonedRate.Average(x => x.Value),
					AverageSpeedOfAnswer = averageSpeedOfAnswer.Sum(x => x.Value),
					EstimatedServiceLevel = estimatedServiceLevels.Average(x => x.Value),
					ServiceLevel = serviceLevel.Average(x => x.Value),

				}
			};
			dataToExport.StaffingViewModel = new ScheduledStaffingViewModel
			{
				DataSeries = new StaffingDataSeries
				{
					Time = timeList,
					Date = new DateOnly(timeList.First()),
					ForecastedStaffing = forecastedStaffing,
					ActualStaffing = actualStaffing,
					ScheduledStaffing = scheduledStaffing,
					UpdatedForecastedStaffing = updatedForecastedStaffing,
					AbsoluteDifference = null
				}
			};
			dataToExport.IncomingViewModel = new IntradayIncomingViewModel
			{
				DataSeries = new IntradayIncomingDataSeries
				{
					Time = timeList,
					ForecastedCalls = forecastedCalls,
					CalculatedCalls = calculatedCalls,
					ForecastedAverageHandleTime = forecastedAverageHandleTime,
					AverageHandleTime = averageHandleTime,
					ServiceLevel = null
				},
				Summary = new IntradayIncomingSummary()
				{
					ForecastedCalls = forecastedCalls.Sum(),
					CalculatedCalls = calculatedCalls.Sum(x => x.Value),
					ForecastedAverageHandleTime = forecastedAverageHandleTime.Sum(),
					AverageHandleTime = averageHandleTime.Sum(x => x.Value),
					ForecastedActualCallsDiff = ((calculatedCalls.Sum(x => x.Value) - forecastedCalls.Sum()) / forecastedCalls.Sum()),
					ForecastedActualHandleTimeDiff = (averageHandleTime.Sum(x => x.Value) - forecastedAverageHandleTime.Sum()) / forecastedAverageHandleTime.Sum()
				}
			};

			return dataToExport;
		}
	}
}
