using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    /// <summary>
    /// Represents MultiplicatorTypeView.
    /// </summary>
    public class MultiplicatorTypeView
    {
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2009-01-10
        /// </remarks>
        public string DisplayName { get; set; }
        /// <summary>
        /// Gets or sets the type of the multiplicator.
        /// </summary>
        /// <value>The type of the multiplicator.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2009-01-10
        /// </remarks>
        public MultiplicatorType MultiplicatorType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiplicatorTypeView"/> class.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <param name="multiplicatorType">Type of the multiplicator.</param>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2009-01-10
        /// </remarks>
        public MultiplicatorTypeView(string displayName,MultiplicatorType multiplicatorType)
        {
            DisplayName = displayName;
            MultiplicatorType = multiplicatorType;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

}
