using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Presentation;

namespace Teleopti.Ccc.WinCode.PersonAdmin
{
    /// <summary>
    /// Datasource class for the PersonsInRole grid. 
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-11-21
    /// </remarks>
    public class PersonGridData : EntityContainer<Person>
    {

        /// <summary>
        /// Gets or sets the FirstName.
        /// </summary>
        /// <value>The FirstName.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/11/2007
        /// </remarks>
        public string FirstName
        {
            get 
            {
                if (ContainedEntity.Name != null)
                    return ContainedEntity.Name.FirstName;
                else
                    return string.Empty;
            }
            set 
            {
                Name name = new Name(value, LastName);
                ContainedEntity.Name = name;
            }
        }

        /// <summary>
        /// Gets or sets the LastName.
        /// </summary>
        /// <value>The LastName.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/11/2007
        /// </remarks>
        public string LastName
        {
            get
            {
                if (ContainedEntity.Name != null)
                    return ContainedEntity.Name.LastName;
                else
                    return string.Empty;
            }
            set
            {
                Name name = new Name(FirstName, value);
                ContainedEntity.Name = name;
            }
        }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/11/2007
        /// </remarks>
        public string Email
        {
            get { return ContainedEntity.Email; }
            set { ContainedEntity.Email = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is agent.
        /// </summary>
        /// <value><c>true</c> if this instance is agent; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/11/2007
        /// </remarks>
        public bool IsAgent
        {
            get { return ContainedEntity.IsAgent(); }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is user.
        /// </summary>
        /// <value><c>true</c> if this instance is user; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/11/2007
        /// </remarks>
        public bool IsUser
        {
            get { return ContainedEntity.PermissionInformation != null; }
        }

    }
}