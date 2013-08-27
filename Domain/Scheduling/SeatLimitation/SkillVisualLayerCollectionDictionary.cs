using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
    /// <summary>
    /// Matrix to hold Person - Skill data. Technically speaking that is a nested dictionary consist of
    /// an outer dictionary with a key of Person and an inner dictionary as value and the inner dictionary
    /// of double as value with a key of Skill.
    /// </summary>
    [Serializable]
    public class SkillVisualLayerCollectionDictionary : Dictionary<ISkill, IList<IVisualLayerCollection>>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedSkillResourceDictionary"/> class.
        /// </summary>
        public SkillVisualLayerCollectionDictionary()
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedSkillResourceDictionary"/> class.
        /// </summary>
        /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo"/> object containing the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
        /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext"/> structure containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
        protected SkillVisualLayerCollectionDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            //
        }
    }
}
