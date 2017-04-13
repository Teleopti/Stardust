using System.Windows.Forms;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Permissions
{
    /// <summary>
    /// Adapter for ApplicationRole class
    /// </summary>
    /// <remarks>
    /// Created by: Muhamad Risath
    /// Created date: 10/9/2008
    /// </remarks>
    public class ApplicationRoleAdapter
    {
        /// <summary>
        /// Gets or sets the application role.
        /// </summary>
        /// <value>The application role.</value>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/9/2008
        /// </remarks>
        public ApplicationRole ApplicationRole { get; set; }

        /// <summary>
        /// Gets or sets the list view item.
        /// </summary>
        /// <value>The list view item.</value>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/9/2008
        /// </remarks>
        public ListViewItem ListViewItem { get; set; }
    }
}
