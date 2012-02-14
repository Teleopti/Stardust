using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.DomainTest.Security.AuthorizationBlock;

namespace Teleopti.Ccc.WinCodeTest.Common.Presentation
{

    /// <summary>
    /// Domain entity presenter test class implementation for test purposes.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 11/23/2007
    /// </remarks>
    public class DomainEntityPresenterTestClass : Entity
    {

        string _name;
        string _description;

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEntityPresenterTestClass"/> class.
        /// </summary>
        public DomainEntityPresenterTestClass()
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationEntityTestClass"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/23/2007
        /// </remarks>
        public DomainEntityPresenterTestClass(string name, string description)
        {
            _name = name;
            _description = description;
        }

        /// <summary>
        /// Gets or sets the name field.
        /// </summary>
        /// <value>The key field.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/23/2007
        /// </remarks>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description field.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/23/2007
        /// </remarks>
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }
    }
}
