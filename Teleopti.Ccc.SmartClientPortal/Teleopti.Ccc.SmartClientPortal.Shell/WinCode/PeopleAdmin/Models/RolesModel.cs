using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{
    /// <summary>
    /// Model class for ApplicationRole view
    /// </summary>
    public class RolesModel : EntityContainer<IApplicationRole>
    {

        /// <summary>
        /// Gets the role.
        /// </summary>
        /// <value>The role.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-29
        /// </remarks>
        public IApplicationRole Role { get { return ContainedEntity; } }

        /// <summary>
        /// Gets or sets the state of the tri.
        /// </summary>
        /// <value>The state of the tri.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-29
        /// </remarks>
        public int TriState { get; set; }

        /// <summary>
        /// Gets or sets the role exists in person count.
        /// </summary>
        /// <value>The role exists in person count.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-29
        /// </remarks>
        public int RoleExistsInPersonCount { get; set; }

        /// <summary>
        /// Gets the description text.
        /// </summary>
        /// <value>The description text.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-29
        /// </remarks>
        public string DescriptionText { get { return ContainedEntity.DescriptionText; } }
    }
}
