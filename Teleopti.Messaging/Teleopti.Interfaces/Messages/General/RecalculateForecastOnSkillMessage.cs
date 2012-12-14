using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Messages.General
{
	///<summary>
	/// Message with details to perform an recalc of forecasting data on a skill.
	///</summary>
	public class RecalculateForecastOnSkillMessage : RaptorDomainMessage
	{
		private readonly Guid _messageId = Guid.NewGuid();

		/// <summary>
		/// Gets the message identity.
		/// </summary>
		public override Guid Identity
		{
			get { return _messageId; }
		}

		///<summary>
		/// The period to recalculate in the skills time zone.
		///</summary>
		public DateOnlyPeriod Period { get; set; }

		///<summary>
		/// The skill to recalculate.
		///</summary>
		public Guid SkillId { get; set; }

        /// <summary>
        /// The workloads to recalculate
        /// </summary>
        public List<Guid> WorkloadIds { get; set; }

		/// <summary>
		/// The owner of this action. can we use this to say that this person updated the forecast????
		/// </summary>
		public Guid OwnerPersonId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Guid ScenarioId { get; set; }

		// add information how the forecast should change
		// percent up/down or some other way
	}

    /// <summary>
    /// Container for Recalculate messages
    /// </summary>
    public class RecalculateForecastOnSkillMessageCollection : RaptorDomainMessage
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
        /// Collection of recalculate messages
        /// </summary>
        public List<RecalculateForecastOnSkillMessage> MessageCollection { get; set; } 
    }
}