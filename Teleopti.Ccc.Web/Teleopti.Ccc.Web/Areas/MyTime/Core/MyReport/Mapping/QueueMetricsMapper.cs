using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping
{
	public class QueueMetricsMapper : IQueueMetricsMapper
	{
		public ICollection<QueueMetricsViewModel> Map(ICollection<QueueMetricsForDayResult> dataModels)
		{
			if (dataModels.Count.Equals(0))
			{
				return new List<QueueMetricsViewModel>{new QueueMetricsViewModel{DataAvailable = false}};
			}
			var maxTime = (from c in dataModels select c.AverageHandlingTime.TotalSeconds).Max();

			var models= dataModels.Select(model => new QueueMetricsViewModel
			{
				DataAvailable = true,
				Queue = model.Queue,
				AnsweredCalls = model.AnsweredCalls,
				HandlingPercent = toPercentString(model.AverageHandlingTime, maxTime),
				TalkTimePercent = toPercentString(model.AverageTalkTime, maxTime),
				AfterCallWorkPercent = toRestPercentString(model.AverageHandlingTime,model.AverageTalkTime, maxTime),
				AverageAfterCallWork = toTimeString(model.AverageAfterCallWorkTime),
				AverageHandlingTime = toTimeString(model.AverageHandlingTime),
				AverageTalkTime = toTimeString(model.AverageTalkTime),
			}).ToList();

			return models;
		}

		private string toPercentString(TimeSpan timeSpan, double maxValue)
		{
			return (timeSpan.TotalSeconds / maxValue * 100).ToString(CultureInfo.InvariantCulture) + "%";
		}

		private string toRestPercentString(TimeSpan handlingtimeSpan, TimeSpan talktimeSpan,double maxValue)
		{
			var handlingP = handlingtimeSpan.TotalSeconds/maxValue*100;
			var talkP = talktimeSpan.TotalSeconds/maxValue*100;
			return (handlingP - talkP).ToString(CultureInfo.InvariantCulture) + "%";
		}

		private static string toTimeString(TimeSpan timeSpan)
		{
            // to get 320 when it is 319.61 for example
		    timeSpan = TimeSpan.FromSeconds(Math.Round(timeSpan.TotalSeconds, 0));
			var temp = timeSpan.ToString(@"hh\:mm\:ss");
			if (temp.StartsWith("00:"))
				temp = temp.Substring(3);

			if (temp.StartsWith("00:"))
				temp = temp.Substring(3) + UserTexts.Resources.SecondShort;
			return temp;
		}
	}

	public interface IQueueMetricsMapper
	{
		ICollection<QueueMetricsViewModel> Map(ICollection<QueueMetricsForDayResult> dataModel);
	}
}