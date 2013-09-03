using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;

namespace Teleopti.Ccc.DomainTest.Scheduling.SeatLimitations
{
    internal class SkillVisualLayerCollectionDictionaryTestClass : SkillVisualLayerCollectionDictionary
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillVisualLayerCollectionDictionaryTestClass"/> class.
        /// </summary>
        public SkillVisualLayerCollectionDictionaryTestClass()
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillVisualLayerCollectionDictionaryTestClass"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        public SkillVisualLayerCollectionDictionaryTestClass(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            //
        }
    }
}
