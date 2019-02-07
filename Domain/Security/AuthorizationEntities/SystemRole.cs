using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
    /// <summary>
    /// A system role, provided by foreign systems (like Matrix)
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 2008-01-15
    /// </remarks>
    public class SystemRole : AggregateRoot_Events_ChangeInfo_Versioned, IDeleteTag
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

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

	    public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}
