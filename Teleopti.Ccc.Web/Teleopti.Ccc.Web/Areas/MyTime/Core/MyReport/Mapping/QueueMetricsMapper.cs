using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();

            return dataModels.Select(model => new QueueMetricsViewModel
            {
                DataAvailable = true,
                Queue = model.Queue,
                AnsweredCalls = model.AnsweredCalls,
                AverageAfterCallWork = model.AverageAfterCallWorkTime.TotalSeconds.ToString(culture),
                AverageHandlingTime = model.AverageHandlingTime.TotalSeconds.ToString(culture),
                AverageTalkTime = model.AverageTalkTime.TotalSeconds.ToString(culture),
            }).ToList();
        }
    }

    public interface IQueueMetricsMapper
    {
        ICollection<QueueMetricsViewModel> Map(ICollection<QueueMetricsForDayResult> dataModel);
    }
}