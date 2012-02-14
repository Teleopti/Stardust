using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    /// <summary>
    /// Testable class for PersonSkillDictionary
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2229:ImplementSerializationConstructors"), Serializable]
    public class PersonSkillDictionaryTestClass : PersonSkillDictionary
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonSkillDictionaryTestClass"/> class.
        /// </summary>
        /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo"/> object containing the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
        /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext"/> structure containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param>
        public PersonSkillDictionaryTestClass(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            //
        }
    }
}
