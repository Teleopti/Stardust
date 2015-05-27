using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
    public interface IEtlJobRelativePeriod
    {
        MinMax<int> RelativePeriod { get; }
        JobCategoryType JobCategory { get; }
        string JobCategoryName { get; }
    }
}