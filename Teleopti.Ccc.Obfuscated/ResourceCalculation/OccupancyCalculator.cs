using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    /// <summary>
    /// Calculates multiskill effect on occupancy.
    /// </summary>
    public class OccupancyCalculator
    {
        private readonly IDictionary<ISkill, ISkillStaffPeriod> _relevantSkillStaffPeriods;
        private readonly KeyedSkillResourceDictionary _relativeKeyedSkillResourceResources;

        /// <summary>
        /// Initializes a new instance of the <see cref="OccupancyCalculator"/> class.
        /// </summary>
        /// <param name="relevantSkillStaffPeriods">The relevant skill staff periods.</param>
        /// <param name="relativeKeyedSkillResourceResources">The relative person skill resources.</param>
        public OccupancyCalculator(IDictionary<ISkill, ISkillStaffPeriod> relevantSkillStaffPeriods,
                                   KeyedSkillResourceDictionary relativeKeyedSkillResourceResources)
        {
            _relevantSkillStaffPeriods = relevantSkillStaffPeriods;
            _relativeKeyedSkillResourceResources = relativeKeyedSkillResourceResources;
        }

        /// <summary>
        /// Does the magic. Takes the multiskill effect into account and recalculates the occupancy data 
        /// and updates the relevant skill staff period data.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-07
        /// </remarks>
        public void AdjustOccupancy()
        {
            IList<SkillCollectionKey> skillcombinations = CreateCalculatedSkillCollectionKeys();

            foreach (KeyValuePair<string, Dictionary<ISkill, double>> personSkillValues in _relativeKeyedSkillResourceResources)
            {
                SkillCollectionKey key = _relativeKeyedSkillResourceResources.SkillCombination(personSkillValues.Key);
                int index = skillcombinations.IndexOf(key);
                if (index > -1)
                {
                    SkillCollectionKey skillCombination = skillcombinations[index];
                    double skillTraff;
                    if (personSkillValues.Value.TryGetValue(key.SkillCollection.First(), out skillTraff))
                    {
                        skillCombination.SumTraff += skillTraff;
                        skillCombination.SumOccWeight += skillTraff*
                                                         skillCombination.VirtualSkillStaffPeriod.Payload.
                                                             CalculatedOccupancy;
                    }
                }
            }

            //create the new minOcc for each skill
            foreach (KeyValuePair<ISkill, ISkillStaffPeriod> skillSkillStaffPeriod in _relevantSkillStaffPeriods)
            {
                ISkill skill = skillSkillStaffPeriod.Key;
                if(skill.SkillType is ISkillTypePhone)
                {
                    double sumTraff = 0;
                    double sumOccWeight = 0;
                    foreach (SkillCollectionKey skillcombination in skillcombinations)
                    {
                        if (skillcombination.SkillCollection.Contains(skill))
                        {
                            sumTraff += skillcombination.SumTraff;
                            sumOccWeight += skillcombination.SumOccWeight;
                        }
                    }

                    ISkillStaffPeriod skillStaffPeriod = skillSkillStaffPeriod.Value;
                    skillStaffPeriod.Payload.MultiskillMinOccupancy = new Percent(sumOccWeight / sumTraff);
                    skillStaffPeriod.CalculateStaff();
                }
            }
        }

    
        private IList<SkillCollectionKey> CreateCalculatedSkillCollectionKeys()
        {
            //Identify the combined skills
            IList<SkillCollectionKey> ret = new List<SkillCollectionKey>();

            //Step the list and create combined SkillStaffPeriods
            foreach (SkillCollectionKey skillCollectionKey in _relativeKeyedSkillResourceResources.UniqueSkillCombinations())
            {
                var listToCombine = new List<ISkillStaffPeriod>();
                foreach (ISkill skill in skillCollectionKey.SkillCollection)
                {
                    ISkillStaffPeriod current;
                    if (_relevantSkillStaffPeriods.TryGetValue(skill, out current))
                        listToCombine.Add(current);
                }
                ISkillStaffPeriod combinedSkillStaffPeriod = SkillStaffPeriod.Combine(listToCombine);

                if (combinedSkillStaffPeriod != null)
                {
                    //Create a new skillStaffPeriod with minOcc = 0
                    Percent newMinOcc = new Percent(0);
                    Percent newMaxOcc = combinedSkillStaffPeriod.Payload.ServiceAgreementData.MaxOccupancy;

                    ServiceAgreement newSeviceAgreement =
                        new ServiceAgreement(combinedSkillStaffPeriod.Payload.ServiceAgreementData.ServiceLevel, newMinOcc, newMaxOcc);
                    ISkillStaffPeriod newSkillStaffPeriod =
                        new SkillStaffPeriod(combinedSkillStaffPeriod.Period, combinedSkillStaffPeriod.Payload.TaskData,
                                             newSeviceAgreement, combinedSkillStaffPeriod.StaffingCalculatorService);
                    newSkillStaffPeriod.Payload.Shrinkage = combinedSkillStaffPeriod.Payload.Shrinkage;
                    newSkillStaffPeriod.CalculateStaff();
                    skillCollectionKey.VirtualSkillStaffPeriod = newSkillStaffPeriod;

                    ret.Add(skillCollectionKey);
                }
            }

            return ret;
        }
    }
}