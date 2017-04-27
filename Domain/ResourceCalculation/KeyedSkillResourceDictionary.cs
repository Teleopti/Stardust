using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Matrix to hold Person - Skill data. Technically speaking that is a nested dictionary consist of
    /// an outer dictionary with a key of Person and an inner dictionary as value and the inner dictionary
    /// of double as value with a key of Skill.
    /// </summary>
    [Serializable]
	public class KeyedSkillResourceDictionary : Dictionary<DoubleGuidCombinationKey, Dictionary<ISkill, double>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedSkillResourceDictionary"/> class.
        /// </summary>
        public KeyedSkillResourceDictionary()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedSkillResourceDictionary"/> class.
        /// </summary>
        /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo"/> object containing the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
        /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext"/> structure containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
        protected KeyedSkillResourceDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
