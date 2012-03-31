using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public class IsColumnCountInRowValid : Specification<string[]>
    {
        public override bool IsSatisfiedBy(string[] obj)
        {
            return obj.Length == ForecastsFileConstants.FileColumnsWithoutAgent || obj.Length == ForecastsFileConstants.FileColumnsWithAgent;
        }
    }
}