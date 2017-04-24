using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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

		MaxSeatSkill,

        /// <summary>
        /// Skill for Stores for example
        /// </summary>
        NonBlendSkill,

		/// <summary>
		/// Skill for retail stores.
		/// </summary>
		Retail,

		/// <summary>
		/// Skill for chat conversations.
		/// </summary>
		Chat
    }
}
