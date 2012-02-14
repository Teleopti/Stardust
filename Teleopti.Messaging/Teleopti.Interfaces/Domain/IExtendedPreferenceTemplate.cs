using System.Drawing;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for ExtendedPreferenceTemplate.
    /// </summary>
    public interface IExtendedPreferenceTemplate : IAggregateRoot
    {
        /// <summary>
        /// Gets the color.
        /// </summary>
        /// <remarks>
        /// Created by: marias
        /// Created date: 2010-08-24
        /// </remarks>
        Color DisplayColor { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <remarks>
        /// Created by: marias
        /// Created date: 2010-08-24
        /// </remarks>
        string Name { get; }

        ///<summary>
        /// The person that owns this template
        ///</summary>
        IPerson Person { get; }

        ///<summary>
        /// Restriction information for this template
        ///</summary>
        IPreferenceRestrictionTemplate Restriction { get; }
    }
}
