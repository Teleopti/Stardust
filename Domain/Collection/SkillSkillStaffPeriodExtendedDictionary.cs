using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
    [Serializable]
    public class SkillSkillStaffPeriodExtendedDictionary : Dictionary<ISkill, ISkillStaffPeriodDictionary>, ISkillSkillStaffPeriodExtendedDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkillSkillStaffPeriodDictionary"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-12-11
        /// </remarks>
        public SkillSkillStaffPeriodExtendedDictionary() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillSkillStaffPeriodExtendedDictionary"/> class.
        /// </summary>
        /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo"/> object containing the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
        /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext"/> structure containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
		[SecurityCritical]
        protected SkillSkillStaffPeriodExtendedDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Periods this instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-12-10
        /// </remarks>
        public DateTimePeriod? Period()
        {
            DateTime? minTime = null, maxTime = null;

            foreach (var periodCollection in Values)
            {
                if (periodCollection.Any())
                {
                    var currentMinTime = periodCollection.Min(p => p.Value.Period.StartDateTime);
                    var currentMaxTime = periodCollection.Max(p => p.Value.Period.EndDateTime);
                    if (!maxTime.HasValue || currentMaxTime>maxTime.Value)
                    {
                        maxTime = currentMaxTime;
                    }

                    if (!minTime.HasValue || currentMinTime < minTime.Value)
                    {
                        minTime = currentMinTime;
                    }
                }
            }

            DateTimePeriod? returnValue = (!minTime.HasValue || !maxTime.HasValue)
                                              ? (DateTimePeriod?)null
                                              : new DateTimePeriod(minTime.Value, maxTime.Value);
            return returnValue;
        }
    }
}
