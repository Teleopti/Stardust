using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer.ViewModels;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer
{
	public class IntradayExportCreator
	{
		public byte[] ExportDataToExcel(IntradayExcelExport intradayExcelExport)
		{
			var excelCreator = new IntradayExcelCreator();
			var export = new IntradayExcelExportData()
			{
				SkillAreaName = intradayExcelExport.SkillAreaName,
				Skills = intradayExcelExport.Skills,
				Date = intradayExcelExport.Date
			};
			
			export.DataTotal = new IntradayExcelExportRowData
			{
				ForecastedVolume = intradayExcelExport.IncomingViewModel.Summary.ForecastedCalls,
				ActualVolume = intradayExcelExport.IncomingViewModel.Summary.CalculatedCalls,
				DifferenceVolume = intradayExcelExport.IncomingViewModel.Summary.ForecastedActualCallsDiff / 100,
				ForecastedAht = intradayExcelExport.IncomingViewModel.Summary.ForecastedAverageHandleTime,
				ActualAht = intradayExcelExport.IncomingViewModel.Summary.AverageHandleTime,
				DifferenceAht = intradayExcelExport.IncomingViewModel.Summary.ForecastedActualHandleTimeDiff / 100,
				ServiceLevel = intradayExcelExport.PerformanceViewModel.Summary.ServiceLevel,
				Esl = intradayExcelExport.PerformanceViewModel.Summary.EstimatedServiceLevel / 100,
				AbandonedRate = intradayExcelExport.PerformanceViewModel.Summary.AbandonRate,
				AverageSpeedOfAnswer = intradayExcelExport.PerformanceViewModel.Summary.AverageSpeedOfAnswer,
				ForecastedAgents = intradayExcelExport.StaffingViewModel.DataSeries.ForecastedStaffing.Sum(),
				RequiredAgents = intradayExcelExport.StaffingViewModel.DataSeries.ActualStaffing.Sum(),
				ScheduledAgents = intradayExcelExport.StaffingViewModel.DataSeries.ScheduledStaffing.Sum(),
				ReforecastedAgents = intradayExcelExport.StaffingViewModel.DataSeries.UpdatedForecastedStaffing.Sum()
			};

			export.RowData = createRows(intradayExcelExport).ToList();
			return excelCreator.CreateExcel(export);
		}

		private IEnumerable<IntradayExcelExportRowData> createRows(IntradayExcelExport export)
		{
			var intervals = export.PerformanceViewModel.DataSeries.Time
				.Union(export.StaffingViewModel.DataSeries.Time)
				.Union(export.IncomingViewModel.DataSeries.Time).OrderBy(x => x);

			foreach (var interval in intervals)
			{
				var indexPerformance = export.PerformanceViewModel.DataSeries.Time.IndexOf(interval);
				var indexStaffing = export.StaffingViewModel.DataSeries.Time.IndexOf(interval);
				var indexIncoming = export.IncomingViewModel.DataSeries.Time.IndexOf(interval);

				var row = new IntradayExcelExportRowData()
				{
					Interval = interval
				};

				if (indexIncoming > -1)
				{
					row.ForecastedVolume = getOrDefault(export.IncomingViewModel.DataSeries.ForecastedCalls, indexIncoming);
					row.ActualVolume = getOrDefault(export.IncomingViewModel.DataSeries.CalculatedCalls, indexIncoming);
					row.ForecastedAht = getOrDefault(export.IncomingViewModel.DataSeries.ForecastedAverageHandleTime, indexIncoming);
					row.ActualAht = getOrDefault(export.IncomingViewModel.DataSeries.AverageHandleTime, indexIncoming);

					row.DifferenceVolume = row.ActualVolume.HasValue && row.ForecastedVolume != 0d
						? (row.ActualVolume.Value - row.ForecastedVolume) / row.ForecastedVolume
						: 0;
					row.DifferenceAht = row.ActualAht.HasValue && row.ForecastedAht != 0d
						? (row.ActualAht.Value - row.ForecastedAht) / row.ForecastedAht
						: 0;
				}

				if (indexPerformance > -1)
				{
					row.ServiceLevel = getOrDefault(export.PerformanceViewModel.DataSeries.ServiceLevel, indexPerformance) / 100;
					row.Esl = getOrDefault(export.PerformanceViewModel.DataSeries.EstimatedServiceLevels, indexPerformance) / 100;
					row.AbandonedRate = getOrDefault(export.PerformanceViewModel.DataSeries.AbandonedRate, indexPerformance) / 100;
					row.AverageSpeedOfAnswer = getOrDefault(export.PerformanceViewModel.DataSeries.AverageSpeedOfAnswer, indexPerformance);
				}

				if (indexStaffing > -1)
				{
					row.ForecastedAgents = getOrDefault(export.StaffingViewModel.DataSeries.ForecastedStaffing, indexStaffing);
					row.RequiredAgents = getOrDefault(export.StaffingViewModel.DataSeries.ActualStaffing, indexStaffing);
					row.ScheduledAgents = getOrDefault(export.StaffingViewModel.DataSeries.ScheduledStaffing, indexStaffing);
					row.ReforecastedAgents = getOrDefault(export.StaffingViewModel.DataSeries.UpdatedForecastedStaffing, indexStaffing);
				}

				yield return row;
			}
		}

		private static T getOrDefault<T>(T[] arr, int index)
		{
			if (arr.Length > index)
			{
				return arr[index];
			}

			return default(T);
		}
	}

	public class IntradayExcelExport
	{
		public DateTime Date { get; set; }
		public string SkillAreaName { get; set; }
		public string[] Skills { get; set; }
		public IntradayPerformanceViewModel PerformanceViewModel { get; set; }
		public ScheduledStaffingViewModel StaffingViewModel { get; set; }
		public IntradayIncomingViewModel IncomingViewModel { get; set; }
	}
	
	public class IntradayExcelExportData
	{
		public DateTime Date { get; set; }
		public string SkillAreaName { get; set; }
		public string[] Skills { get; set; }

		public IntradayExcelExportRowData DataTotal { get; set; }
		public List<IntradayExcelExportRowData> RowData { get; set; }
	}

	public class IntradayExcelExportRowData
	{
		public DateTime Interval { get; set; }
		public double ForecastedVolume { get; set; }
		public double? ActualVolume { get; set; }
		public double DifferenceVolume { get; set; }
		public double ForecastedAht { get; set; }
		public double? ActualAht { get; set; }
		public double DifferenceAht { get; set; }
		public double? ServiceLevel { get; set; }
		public double? Esl { get; set; }
		public double? AbandonedRate { get; set; }
		public double? AverageSpeedOfAnswer { get; set; }
		public double? ForecastedAgents { get; set; }
		public double? RequiredAgents { get; set; }
		public double? ScheduledAgents { get; set; }
		public double? ReforecastedAgents { get; set; }
	}
}
