using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
    /// <summary>
    /// A system role, provided by foreign systems (like Matrix)
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 2008-01-15
    /// </remarks>
    public class SystemRole : VersionedAggregateRoot, IAuthorizationEntity, IDeleteTag
    {
        private string _name;
        private string _descriptionText;
        private bool _isDeleted;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public virtual string DescriptionText
        {
            get { return _descriptionText; }
            set { _descriptionText = value; }
        }

        #region IAuthorizationEntity Members

        /// <summary>
        /// Gets the authorization unique key. This must be unique and that is used for any
        /// comparisons within the authorization related stuff.
        /// </summary>
        /// <value>The authorization key.</value>
        public virtual string AuthorizationKey
        {
            get { return DescriptionText; }
        }

        /// <summary>
        /// Gets the Name value. Usually this is the key.
        /// </summary>
        /// <value>The name field.</value>
        public virtual string AuthorizationName
        {
            get 
            {
              return Name;
            }
        }

        /// <summary>
        /// Gets the description or additional info value. Usually that is
        /// a longer description about the authorization entity.
        /// </summary>
        /// <value>The description field.</value>
        /// <remarks>
        /// Usually this value goes to the tooltip to the control.
        /// </remarks>
        public virtual string AuthorizationDescription
        {
            get
            {
                return DescriptionText;
            }
        }

        /// <summary>
        /// Gets any additional value connected to the authorization
        /// </summary>
        /// <value>The value field.</value>
        /// <remarks>
        /// Usually this value holds some numeric data.
        /// </remarks>
        public virtual string AuthorizationValue
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        #endregion

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }

}
