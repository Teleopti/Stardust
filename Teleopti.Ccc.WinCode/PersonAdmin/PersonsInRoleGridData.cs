using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.WinCode.PersonAdmin
{
    /// <summary>
    /// Datasource class for the PersonsInRole grid. 
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-11-21
    /// </remarks>
    public class PersonsInRoleGridData
    {
        private bool _isPersonInRole;
        private readonly Person _person;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonsInRoleGridData"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-21
        /// </remarks>
        public PersonsInRoleGridData(Person person)
        {
            InParameter.NotNull("person", person);
            _person = person;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the person is part of a role.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is person in role; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-21
        /// </remarks>
        public bool IsPersonInRole
        {
            get { return _isPersonInRole; }
            set { _isPersonInRole = value; }
        }

        /// <summary>
        /// Gets the person's first name. Databinded to the grid's FirstName column.
        /// </summary>
        /// <value>The first name of the person.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-21
        /// </remarks>
        public string FirstName
        {
            get { return _person.Name.FirstName; }
        }

        /// <summary>
        /// Gets the person's last name. Databinded to the grid's LastName column.
        /// </summary>
        /// <value>The last name of the person.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-21
        /// </remarks>
        public string LastName
        {
            get { return _person.Name.LastName; }
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>The person.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-21
        /// </remarks>
        public Person Person
        {
            get { return _person; }
        }
    }
}