using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
    /// <summary>
    /// Container for Recalculate messages
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class RecalculateForecastOnSkillCollectionEvent : IEvent, ILogOnContext
    {
        private readonly Guid _messageId = Guid.NewGuid();

        /// <summary>
        /// Gets the message identity.
        /// </summary>
        public Guid Identity
        {
            get { return _messageId; }
        }

        /// <summary>
        /// The owner of this action. can we use this to say that this person updated the forecast????
        /// </summary>
        public Guid OwnerPersonId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid ScenarioId { get; set; }

        /// <summary>
        /// Collection of recalculate messages
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IEnumerable<RecalculateForecastOnSkill> SkillCollection { get; set; }

        public string LogOnDatasource { get; set; }
        public Guid LogOnBusinessUnitId { get; set; }
    }
}