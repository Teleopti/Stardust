using System.Collections.Generic;
using System.Reflection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Test set factory for workshift period value calculator. Creates test data.
    /// </summary>
    public static class WorkShiftPeriodValueCalculatorFactory
    {
        /// <summary>
        /// Creates skill - skill staff period dictionary for test.
        /// </summary>
        /// <param name="testPeriod">The test period.</param>
        /// <param name="testTraffsAndSkills">The test traffs and skills.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Traffs")]
        public static IDictionary<ISkill, ISkillStaffPeriod> CreateSkillSkillStaffPeriodDictionary(DateTimePeriod testPeriod, IDictionary<ISkill, int> testTraffsAndSkills)
        {

            IDictionary<ISkill, ISkillStaffPeriod> ret = new Dictionary<ISkill, ISkillStaffPeriod>();

            foreach (KeyValuePair<ISkill, int> skillAndTraffPair in testTraffsAndSkills)
            {
                ISkillStaffPeriod skillStaffPeriod =
                    CreateSkillSkillStaffPeriod(testPeriod, skillAndTraffPair.Value);
                ret.Add(skillAndTraffPair.Key, skillStaffPeriod);
            }
            return ret;

        }

        /// <summary>
        /// Creates a skill skill staff period.
        /// </summary>
        /// <param name="testPeriod">The test period.</param>
        /// <param name="traff">The traff.</param>
        /// <returns></returns>
        public static ISkillStaffPeriod CreateSkillSkillStaffPeriod(DateTimePeriod testPeriod, int traff)
        {

            ISkillStaffPeriod period = SkillStaffPeriodFactory.CreateSkillStaffPeriod(testPeriod, new Task(), new ServiceAgreement());
            ISkillStaff tLayer = period.Payload;

            // Note: the traff (relative) value is used here! The absolut value will be traff * period length [min]. 
            typeof(SkillStaff).GetField("_forecastedIncomingDemand", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(tLayer, traff);
            // Assigned resources value: 3 * 60 = 180
            period.SetCalculatedResource65(3);

            return period;
        }

        public static IVisualLayerCollection CreateVisualLayerCollection(IPerson person,IDictionary<IActivity, DateTimePeriod> activitiesAndPeriods)
        {
            IList<IVisualLayer> layerList = new List<IVisualLayer>();
            IVisualLayerFactory visualLayerFactory = new VisualLayerFactory();

            foreach (KeyValuePair<IActivity, DateTimePeriod> activityAndPeriodPair in activitiesAndPeriods)
            {
                IVisualLayer layer = visualLayerFactory.CreateShiftSetupLayer(activityAndPeriodPair.Key, activityAndPeriodPair.Value, person);
                layerList.Add(layer);
            }

            return new VisualLayerCollection(person, layerList, new ProjectionPayloadMerger());
        }

    }
}
