using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Analytics.ReportTexts;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping
{
    public class QueueMetricsMapper : IQueueMetricsMapper
    {
        private readonly IUserCulture _userCulture;

        public QueueMetricsMapper(IUserCulture userCulture)
        {
            _userCulture = userCulture;
        }

        public ICollection<QueueMetricsViewModel> Map(ICollection<QueueMetricsForDayResult> dataModels)
        {
            if (dataModels.Count.Equals(0))
            {
                return new List<QueueMetricsViewModel>{new QueueMetricsViewModel{DataAvailable = false}};
            }
            //var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();
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
            var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();
            return (timeSpan.TotalSeconds / maxValue * 100).ToString(culture) + "%";
        }

        private string toRestPercentString(TimeSpan handlingtimeSpan, TimeSpan talktimeSpan,double maxValue)
        {
            var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();
            var handlingP = handlingtimeSpan.TotalSeconds/maxValue*100;
            var talkP = talktimeSpan.TotalSeconds/maxValue*100;
            return (handlingP - talkP).ToString(culture) + "%";
        }

        private static string toTimeString(TimeSpan timeSpan)
        {
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