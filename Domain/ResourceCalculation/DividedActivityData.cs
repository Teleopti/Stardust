using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Takes the input domain classes and a activity and creates a structure that 
    /// can be fed to FurnessData object.
    /// </summary>
    /// <remarks>
    /// For more information on the properties see the ResourceOptimizerTestSet.xls Business logic sheet.
    /// </remarks>
    public class DividedActivityData
    {
        private readonly KeyedSkillResourceDictionary _keyedSkillResourceEfficiencies;
        private readonly KeyedSkillResourceDictionary _weightedRelativeKeyedSkillResourceResources;
        private readonly KeyedSkillResourceDictionary _relativeKeyedSkillResourceResources;
        private readonly Dictionary<ISkill, double> _targetDemands;
		private readonly Dictionary<DoubleGuidCombinationKey, double> _personResources;
		private readonly Dictionary<DoubleGuidCombinationKey, double> _relativePersonResources;
        private readonly Dictionary<ISkill, double> _weightedRelativePersonSkillResourcesSum;
        private readonly Dictionary<ISkill, double> _relativePersonSkillResourcesSum;
		
        /// <summary>
        /// Initializes a new instance of the <see cref="DividedActivityData"/> class.
        /// </summary>
        public DividedActivityData()
        {
            _keyedSkillResourceEfficiencies = new KeyedSkillResourceDictionary();
            _weightedRelativeKeyedSkillResourceResources = new KeyedSkillResourceDictionary();
            _relativeKeyedSkillResourceResources = new KeyedSkillResourceDictionary();
            _targetDemands = new Dictionary<ISkill, double>();
            _personResources = new Dictionary<DoubleGuidCombinationKey, double>();
            _relativePersonResources = new Dictionary<DoubleGuidCombinationKey, double>();
            _weightedRelativePersonSkillResourcesSum = new Dictionary<ISkill, double>();
            _relativePersonSkillResourcesSum = new Dictionary<ISkill, double>();
        }
		
        /// <summary>
        /// Gets the person - skill efficiency matrix.
        /// </summary>
        /// <value>The skill efficiency matrix.</value>
        public KeyedSkillResourceDictionary KeyedSkillResourceEfficiencies => _keyedSkillResourceEfficiencies;

	    /// <summary>
        /// Gets the weighted relative person - skill resource matrix.
        /// </summary>
        /// <value>The weighted relative person - skill resource matrix.</value>
        public KeyedSkillResourceDictionary WeightedRelativeKeyedSkillResourceResources => _weightedRelativeKeyedSkillResourceResources;

	    /// <summary>
        /// Gets the target demands.
        /// </summary>
        /// <value>The target demands.</value>
        public IDictionary<ISkill, double> TargetDemands => _targetDemands;

	    /// <summary>
        /// Gets the summa of weighted relative person - skill resources spent for a skill.
        /// </summary>
        /// <value>The weighted relative person - skill resources summa values.</value>
        public IDictionary<ISkill, double> WeightedRelativePersonSkillResourcesSum => _weightedRelativePersonSkillResourcesSum;

	    /// <summary>
        /// Gets the relative person resources, also called TRAFF, spent on each skill in the activity.
        /// </summary>
        /// <value>The relative person - skill resource matrix.</value>
        public KeyedSkillResourceDictionary RelativeKeyedSkillResourceResources => _relativeKeyedSkillResourceResources;

	    /// <summary>
        /// Gets the relative person - skill resources (also called traff) sum for each skill in the activity.
        /// </summary>
        /// <value>The relative person - skill resource values sum.</value>
        public IDictionary<ISkill, double> RelativePersonSkillResourcesSum => _relativePersonSkillResourcesSum;

	    /// <summary>
        /// Gets the absolut person resources, the time, spent on the activity.
        /// </summary>
        /// <value>The absolut person resources.</value>
		public IDictionary<DoubleGuidCombinationKey, double> PersonResources => _personResources;

	    /// <summary>
        /// Gets the relative person resources, also called TRAFF, spent on the activity.
        /// </summary>
        /// <value>The relative person resources.</value>
		public IDictionary<DoubleGuidCombinationKey, double> RelativePersonResources => _relativePersonResources;

	    /// <summary>
        /// Calculates the person resources summa for the final result.
        /// </summary>
        public void CalculatePersonResourcesSummaForReadingResultFromFurness()
        {
            WeightedRelativePersonSkillResourcesSum.Clear();
            foreach (var personKey in PersonResources.Keys)
            {
                Dictionary<ISkill, double> skillValues;
                if (!WeightedRelativeKeyedSkillResourceResources.TryGetValue(personKey, out skillValues)) continue;
                foreach (ISkill skillKey in TargetDemands.Keys)
                {
                    double skillPersonValue;
                    if (!skillValues.TryGetValue(skillKey, out skillPersonValue)) continue;

                    double oldValue;
                    if (WeightedRelativePersonSkillResourcesSum.TryGetValue(skillKey, out oldValue))
                    {
                        WeightedRelativePersonSkillResourcesSum[skillKey] = oldValue + KeyedSkillResourceEfficiencies[personKey][skillKey] * skillPersonValue;
                    }
                    else
                    {
                        WeightedRelativePersonSkillResourcesSum.Add(skillKey, KeyedSkillResourceEfficiencies[personKey][skillKey] * skillPersonValue);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the person resources summa for initialization.
        /// </summary>
        public void CalculatePersonResourcesSummaForFurnessInitialization()
        {
            WeightedRelativePersonSkillResourcesSum.Clear();
            foreach (var personKey in PersonResources.Keys)
            {
                Dictionary<ISkill, double> skillValues;
                if (!WeightedRelativeKeyedSkillResourceResources.TryGetValue(personKey, out skillValues)) continue;
                foreach (ISkill skillKey in TargetDemands.Keys)
                {
                    double skillPersonValue;
                    if (!skillValues.TryGetValue(skillKey, out skillPersonValue)) continue;

                    double oldValue;
                    if (WeightedRelativePersonSkillResourcesSum.TryGetValue(skillKey, out oldValue))
                    {
                        WeightedRelativePersonSkillResourcesSum[skillKey] = oldValue + skillPersonValue;
                    }
                    else
                    {
                        WeightedRelativePersonSkillResourcesSum.Add(skillKey, skillPersonValue);
                    }
                }
            }
        }
    }
}
