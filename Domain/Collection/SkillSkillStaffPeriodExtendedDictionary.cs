using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
    [Serializable]
    public class SkillSkillStaffPeriodExtendedDictionary : Dictionary<ISkill, ISkillStaffPeriodDictionary>, ISkillSkillStaffPeriodExtendedDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkillSkillStaffPeriodExtendedDictionary"/> class.
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

		public bool GuessResourceCalculationHasBeenMade()
		{
			return Values.SelectMany(skillStaffPeriodDic => skillStaffPeriodDic.Values)
				.Any(skillStaffPeriod => Math.Abs(skillStaffPeriod.CalculatedResource) > 0.00001);
		}
	}
}
