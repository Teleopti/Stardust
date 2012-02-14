using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Database.EtlSchedules
{
    public class EtlJobRelativePeriod : IEtlJobRelativePeriod
    {
        public EtlJobRelativePeriod(MinMax<int> relativePeriod, JobCategoryType jobCategory)
        {
            RelativePeriod = relativePeriod;
            JobCategory = jobCategory;
        }

        public MinMax<int> RelativePeriod { get; private set; }

        public JobCategoryType JobCategory { get; private set; }
        public string JobCategoryName
        {
            get
            {
                string returnString = null;

                switch (JobCategory)
                {
                    case JobCategoryType.Initial:
                        returnString = "Initial";
                        break;
                    case JobCategoryType.AgentStatistics:
                        returnString = "Agent Statistics";
                        break;
                    case JobCategoryType.QueueStatistics:
                        returnString = "Queue Statistics";
                        break;
                    case JobCategoryType.Schedule:
                        returnString = "Schedule";
                        break;
                    case JobCategoryType.Forecast:
                        returnString = "Forecast";
                        break;
                    default:
                        break;
                }

                return returnString;
            }
        }
    }
}