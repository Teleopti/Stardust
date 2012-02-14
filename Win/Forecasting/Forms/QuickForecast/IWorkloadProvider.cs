using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    public interface IWorkloadProvider
    {
        IEnumerable<IWorkload> WorkloadCollection { get; }
    }
}