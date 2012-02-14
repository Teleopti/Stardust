﻿namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Defines selectable types of skill types
    /// </summary>
    public enum ForecastSource
    {
        /// <summary>
        /// Backoffice
        /// </summary>
        Backoffice,
        /// <summary>
        /// Time
        /// </summary>
        Time,
        /// <summary>
        /// Email
        /// </summary>
        Email,
        /// <summary>
        /// InboundTelephony
        /// </summary>
        InboundTelephony,
        /// <summary>
        /// OutboundTelephony
        /// </summary>
        OutboundTelephony,
        /// <summary>
        /// Facsimile
        /// </summary>
        Facsimile,
		/// <summary>
		/// Skill for seat limitation
		/// </summary>
		MaxSeatSkill,

        /// <summary>
        /// Skill for Stores for example
        /// </summary>
        NonBlendSkill
    }
}
