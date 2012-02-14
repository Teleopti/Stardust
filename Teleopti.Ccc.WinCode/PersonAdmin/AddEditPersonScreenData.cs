using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Presentation;

namespace Teleopti.Ccc.WinCode.PersonAdmin
{
    /// <summary>
    /// Class holding data for AddEditPersonScreen form
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-11-12
    /// </remarks>
    public class AddEditPersonScreenData: EntityContainer<Person>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="AddEditPersonScreenData"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/3/2007
        /// </remarks>
        public AddEditPersonScreenData(Person person) : base(person)
        {
            //
        }

        /// <summary>
        /// Gets or sets the name of the first.
        /// </summary>
        /// <value>The name of the first.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-12
        /// </remarks>
        public string FirstName
        {
            get { return ContainedEntity.Name.FirstName; }
            set
            {
                ContainedEntity.Name = new Name(value, ContainedEntity.Name.LastName);
            }
        }

        /// <summary>
        /// Gets or sets the name of the last.
        /// </summary>
        /// <value>The name of the last.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-12
        /// </remarks>
        public string LastName
        {
            get { return ContainedEntity.Name.LastName; }
            set { ContainedEntity.Name = new Name(ContainedEntity.Name.FirstName, value); }
        }
    }
}