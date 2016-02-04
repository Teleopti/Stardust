using System;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Messages.General
{
	///<summary>
	/// Message with details to perform an recalc of forecasting data on a skill.
	///</summary>
	public class RecalculateForecastOnSkillMessage 
	{
				
		///<summary>
		/// The skill to recalculate.
		///</summary>
		public Guid SkillId { get; set; }

        /// <summary>
        /// The workloads to recalculate
        /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public Collection<Guid> WorkloadIds { get; set; }

	}

    /// <summary>
    /// Container for Recalculate messages
    /// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public class RecalculateForecastOnSkillMessageCollection : MessageWithLogOnContext
    {
        private readonly Guid _messageId = Guid.NewGuid();

        /// <summary>
        /// Gets the message identity.
        /// </summary>
        public override Guid Identity
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
		public Collection<RecalculateForecastOnSkillMessage> MessageCollection { get; set; } 
    }
}