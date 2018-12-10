using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
    public interface IEtlJobRelativePeriod
    {
        MinMax<int> RelativePeriod { get; }
        JobCategoryType JobCategory { get; }
        string JobCategoryName { get; }
    }
}