using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{
    public class ExternalLogOnModel : EntityContainer<IExternalLogOn>
    {
        /// <summary>
        /// Gets the external log on.
        /// </summary>
        /// <value>The external log on.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-15
        /// </remarks>
        public IExternalLogOn ExternalLogOn { get { return ContainedEntity; } }

        /// <summary>
        /// Gets or sets the state of the tri.
        /// </summary>
        /// <value>The state of the tri.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-15
        /// </remarks>
        public int TriState { get; set; }

        /// <summary>
        /// Gets or sets the external log on in person count.
        /// </summary>
        /// <value>The external log on in person count.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-15
        /// </remarks>
        public int ExternalLogOnInPersonCount { get; set; }

        /// <summary>
        /// Gets the description text.
        /// </summary>
        /// <value>The description text.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-15
        /// </remarks>
        public string DescriptionText { get { return ContainedEntity.AcdLogOnName; } }

		public string AcdText { get { return ContainedEntity.DataSourceId.ToString(CultureInfo.CurrentCulture); } }

        public string LogObjectName { get { return ContainedEntity.DataSourceName ; } }
    }
}
