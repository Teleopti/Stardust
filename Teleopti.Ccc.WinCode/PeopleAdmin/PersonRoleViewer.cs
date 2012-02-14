using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-02-27
    /// </remarks>
   public class PersonRoleViewer
    {
        private bool _isInRole;
        private readonly string _applicationRoleDescription;
        private readonly ApplicationRole _applicationRole;
        private int _triState;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRoleViewer"/> class.
        /// </summary>
        /// <param name="isInRole">if set to <c>true</c> [is in role].</param>
        /// <param name="applicationRole">The application role.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-02-27
        /// </remarks>
        public PersonRoleViewer(bool isInRole, ApplicationRole applicationRole)
        {
            _isInRole = isInRole;
            _applicationRole = applicationRole;
            _applicationRoleDescription = applicationRole.DescriptionText;
        }

        /// <summary>
        /// Gets the application role.
        /// </summary>
        /// <value>The application role.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-02-25
        /// </remarks>
        public ApplicationRole ApplicationRole
        {
            get { return _applicationRole; }
        }

        /// <summary>
        /// Gets the application role discription.
        /// </summary>
        /// <value>The application role discription.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-02-25
        /// </remarks>
        public string ApplicationRoleDescription
        {
            get { return _applicationRoleDescription; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is in role.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is in role; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-02-25
        /// </remarks>
        public bool IsInRole
        {
            get { return _isInRole; }
            set { _isInRole = value; }
        }

        /// <summary>
        /// Gets or sets the state of the tri.
        /// </summary>
        /// <value>The state of the tri.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-02-28
        /// </remarks>
        public int TriState
        {
            get{ return _triState;}
            set { _triState = value; }
        }
    }
}
