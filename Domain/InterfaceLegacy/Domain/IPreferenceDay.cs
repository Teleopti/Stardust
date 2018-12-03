
namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// The preference information for one agent and day
    /// </summary>
    public interface IPreferenceDay: IPersistableScheduleData, IVersioned
    {
        /// <summary>
        /// Gets the restriction.
        /// </summary>
        /// <value>The restriction.</value>
        IPreferenceRestriction Restriction { get; }
        /// <summary>
        /// Gets the restriction date.
        /// </summary>
        /// <value>The restriction date.</value>
        DateOnly RestrictionDate { get; }

        ///<summary>
        ///An indication of which extended preference template the agent selected for this day
        ///</summary>
        string TemplateName { get; set; }
    }
}
