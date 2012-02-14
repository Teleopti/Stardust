using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{

    public interface IDividedActivityData
    {
        /// <summary>
        /// Gets the person - skill efficiency matrix.
        /// </summary>
        /// <value>The skill efficiency matrix.</value>
        PersonSkillDictionary PersonSkillEfficiencies { get; }

        /// <summary>
        /// Gets the weighted relative person - skill resource matrix.
        /// </summary>
        /// <value>The weighted relative person - skill resource matrix.</value>
        PersonSkillDictionary WeightedRelativePersonSkillResources { get; }

        /// <summary>
        /// Gets the target demands.
        /// </summary>
        /// <value>The target demands.</value>
        IDictionary<ISkill, double> TargetDemands { get; }

        /// <summary>
        /// Gets the summa of weighted relative person - skill resources spent for a skill.
        /// </summary>
        /// <value>The weighted relative person - skill resources summa values.</value>
        IDictionary<ISkill, double> WeightedRelativePersonSkillResourcesSum { get; }

        /// <summary>
        /// Gets the relative person resources, also called TRAFF, spent on each skill in the activity.
        /// </summary>
        /// <value>The relative person - skill resource matrix.</value>
        PersonSkillDictionary RelativePersonSkillResources { get; }

        /// <summary>
        /// Gets the relative person - skill resources (also called traff) sum for each skill in the activity.
        /// </summary>
        /// <value>The relative person - skill resource values sum.</value>
        IDictionary<ISkill, double> RelativePersonSkillResourcesSum { get; }

        /// <summary>
        /// Gets the absolut person resources, the time, spent on the activity.
        /// </summary>
        /// <value>The absolut person resources.</value>
        IDictionary<IPerson, double> PersonResources { get; }

        /// <summary>
        /// Gets the relative person resources, also called TRAFF, spent on the activity.
        /// </summary>
        /// <value>The relative person resources.</value>
        IDictionary<IPerson, double> RelativePersonResources { get; }

        /// <summary>
        /// Calculates the person resources summa for final result.
        /// </summary>
        void CalculatePersonResourcesSummaForReadingResultFromFurness();

        /// <summary>
        /// Calculates the person resources summa for initialization.
        /// </summary>
        void CalculatePersonResourcesSummaForFurnessInitialization();

    }
    /// <summary>
    /// Takes the input domain classes and a activity and creates a structure that 
    /// can be fed to FurnessData object.
    /// </summary>
    /// <remarks>
    /// For more information on the properties see the ResourceOptimizerTestSet.xls Business logic sheet.
    /// </remarks>
    public class DividedActivityData : IDividedActivityData
    {

        #region Variables
        
        private readonly PersonSkillDictionary _personSkillEfficiencies;
        private readonly PersonSkillDictionary _weightedRelativePersonSkillResources;
        private readonly PersonSkillDictionary _relativePersonSkillResources;
        private readonly Dictionary<ISkill, double> _targetDemands;
        private readonly Dictionary<IPerson, double> _personResources;
        private readonly Dictionary<IPerson, double> _relativePersonResources;
        private readonly Dictionary<ISkill, double> _weightedRelativePersonSkillResourcesSum;
        private readonly Dictionary<ISkill, double> _relativePersonSkillResourcesSum;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DividedActivityData"/> class.
        /// </summary>
        public DividedActivityData()
        {
            _personSkillEfficiencies = new PersonSkillDictionary();
            _weightedRelativePersonSkillResources = new PersonSkillDictionary();
            _relativePersonSkillResources = new PersonSkillDictionary();
            _targetDemands = new Dictionary<ISkill, double>();
            _personResources = new Dictionary<IPerson, double>();
            _relativePersonResources = new Dictionary<IPerson, double>();
            _weightedRelativePersonSkillResourcesSum = new Dictionary<ISkill, double>();
            _relativePersonSkillResourcesSum = new Dictionary<ISkill, double>();
        }

        #region Interface

        /// <summary>
        /// Gets the person - skill efficiency matrix.
        /// </summary>
        /// <value>The skill efficiency matrix.</value>
        public PersonSkillDictionary PersonSkillEfficiencies
        {
            get { return _personSkillEfficiencies; }
        }

        /// <summary>
        /// Gets the weighted relative person - skill resource matrix.
        /// </summary>
        /// <value>The weighted relative person - skill resource matrix.</value>
        public PersonSkillDictionary WeightedRelativePersonSkillResources
        {
            get { return _weightedRelativePersonSkillResources; }
        }

        /// <summary>
        /// Gets the target demands.
        /// </summary>
        /// <value>The target demands.</value>
        public IDictionary<ISkill, double> TargetDemands
        {
            get { return _targetDemands; }
        }

        /// <summary>
        /// Gets the summa of weighted relative person - skill resources spent for a skill.
        /// </summary>
        /// <value>The weighted relative person - skill resources summa values.</value>
        public IDictionary<ISkill, double> WeightedRelativePersonSkillResourcesSum
        {
            get { return _weightedRelativePersonSkillResourcesSum; }
        }

        /// <summary>
        /// Gets the relative person resources, also called TRAFF, spent on each skill in the activity.
        /// </summary>
        /// <value>The relative person - skill resource matrix.</value>
        public PersonSkillDictionary RelativePersonSkillResources
        {
            get { return _relativePersonSkillResources; }
        }

        /// <summary>
        /// Gets the relative person - skill resources (also called traff) sum for each skill in the activity.
        /// </summary>
        /// <value>The relative person - skill resource values sum.</value>
        public IDictionary<ISkill, double> RelativePersonSkillResourcesSum
        {
            get { return _relativePersonSkillResourcesSum; }
        }

        /// <summary>
        /// Gets the absolut person resources, the time, spent on the activity.
        /// </summary>
        /// <value>The absolut person resources.</value>
        public IDictionary<IPerson, double> PersonResources
        {
            get { return _personResources; }
        }

        /// <summary>
        /// Gets the relative person resources, also called TRAFF, spent on the activity.
        /// </summary>
        /// <value>The relative person resources.</value>
        public IDictionary<IPerson, double> RelativePersonResources
        {
            get { return _relativePersonResources; }
        }

        /// <summary>
        /// Calculates the person resources summa for the final result.
        /// </summary>
        public void CalculatePersonResourcesSummaForReadingResultFromFurness()
        {
            WeightedRelativePersonSkillResourcesSum.Clear();
            foreach (IPerson personKey in PersonResources.Keys)
            {
                Dictionary<ISkill, double> skillValues;
                if (!WeightedRelativePersonSkillResources.TryGetValue(personKey, out skillValues)) continue;
                foreach (ISkill skillKey in TargetDemands.Keys)
                {
                    double skillPersonValue;
                    if (!skillValues.TryGetValue(skillKey, out skillPersonValue)) continue;

                    double oldValue;
                    if (WeightedRelativePersonSkillResourcesSum.TryGetValue(skillKey, out oldValue))
                    {
                        WeightedRelativePersonSkillResourcesSum[skillKey] = oldValue + PersonSkillEfficiencies[personKey][skillKey] * skillPersonValue;
                    }
                    else
                    {
                        WeightedRelativePersonSkillResourcesSum.Add(skillKey, PersonSkillEfficiencies[personKey][skillKey] * skillPersonValue);
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
            foreach (IPerson personKey in PersonResources.Keys)
            {
                Dictionary<ISkill, double> skillValues;
                if (!WeightedRelativePersonSkillResources.TryGetValue(personKey, out skillValues)) continue;
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




        #endregion

    }
}
