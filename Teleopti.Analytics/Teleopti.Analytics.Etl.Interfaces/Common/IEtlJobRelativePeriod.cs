using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Interfaces.Common
{
    public interface IEtlJobRelativePeriod
    {
        MinMax<int> RelativePeriod { get; }
        JobCategoryType JobCategory { get; }
        string JobCategoryName { get; }
    }
}