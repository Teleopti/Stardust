using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Matrix to hold Person - Skill data. Technically speaking that is a nested dictionary consist of
    /// an outer dictionary with a key of Person and an inner dictionary as value and the inner dictionary
    /// of double as value with a key of Skill.
    /// </summary>
    [Serializable]
    public class PersonSkillDictionary : Dictionary<IPerson, Dictionary<ISkill, double>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonSkillDictionary"/> class.
        /// </summary>
        public PersonSkillDictionary()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonSkillDictionary"/> class.
        /// </summary>
        /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo"/> object containing the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
        /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext"/> structure containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
        protected PersonSkillDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets the SkillCollectionKey for the specifyed person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-06
        /// </remarks>
        public SkillCollectionKey SkillCombination(IPerson person)
        {
            Dictionary<ISkill, double> personSkills = this[person];
            List<ISkill> skillList = new List<ISkill>(personSkills.Keys);
            SkillCollectionKey key = new SkillCollectionKey(skillList);
            return key;
        }

        /// <summary>
        /// Gets a list with unique skill combinations.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-03-06
        /// </remarks>
        public ICollection<SkillCollectionKey> UniqueSkillCombinations()
        {
            ICollection<SkillCollectionKey> result = new HashSet<SkillCollectionKey>();
            foreach (IPerson person in Keys)
            {
                SkillCollectionKey skillCollectionKey = SkillCombination(person);
                result.Add(skillCollectionKey);
            }

            return result;
        }
    }
}
