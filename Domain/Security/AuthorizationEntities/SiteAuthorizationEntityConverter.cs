using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{

    /// <summary>
    /// Site - authentication info converter class.
    /// </summary>
    public class SiteAuthorizationEntityConverter : EntityContainer<ISite>, IAuthorizationEntity
    {
        private IApplicationRole _applicationRole;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteAuthorizationEntityConverter"/> class.
        /// </summary>
        /// <param name="site">The Site.</param>
        /// <param name="applicationRole">The application role.</param>
        public SiteAuthorizationEntityConverter(ISite site, IApplicationRole applicationRole) : base(site)
        {
            InParameter.NotNull("applicationRole", applicationRole);
            _applicationRole = applicationRole;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SiteAuthorizationEntityConverter()
        {
            
        }

        #region IAuthorizationEntity Members

        /// <summary>
        /// Gets the authorization unique key. This must be unique and that is used for any
        /// comparisons within the authorization related stuff.
        /// </summary>
        /// <value>The authorization key.</value>
        public string AuthorizationKey
        {
            get { return ContainedEntity.Id.ToString(); }
        }

        /// <summary>
        /// Gets the Name value. Usually this is the key.
        /// </summary>
        /// <value>The name field.</value>
        public string AuthorizationName
        {
            get { return ContainedEntity.Description.ToString(); }
        }

        /// <summary>
        /// Gets the description or additional info value. Usually that is
        /// a longer description about the authorization entity.
        /// </summary>
        /// <value>The description field.</value>
        /// <remarks>
        /// Usually this value goes to the tooltip to the control.
        /// </remarks>
        public string AuthorizationDescription
        {
            get { return ContainedEntity.Description.ToString() + " Site"; }
        }

        /// <summary>
        /// Gets any additional value connected to the authorization
        /// </summary>
        /// <value>The value field.</value>
        /// <remarks>
        /// Usually this value holds some numeric data.
        /// </remarks>
        public string AuthorizationValue
        {
            get { return _applicationRole.Name; }
        }

        #endregion
    }
}
