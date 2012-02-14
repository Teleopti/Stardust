using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Permissions
{
    /// <summary>
    /// PersonAdapter class
    /// </summary>
    /// <remarks>
    /// Created by: Muhamad Risath
    /// Created date: 11/17/2008
    /// </remarks>
    public class PersonAdapter
    {
        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>The person.</value>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public IPerson Person { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is dirty.
        /// </summary>
        /// <value><c>true</c> if this instance is dirty; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public bool IsDirty { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is lazy loaded.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is lazy loaded; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public bool IsLazyLoaded { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAdapter"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public PersonAdapter(IPerson person)
        {
            IsLazyLoaded = true;
            Person = person;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAdapter"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="isDirty">if set to <c>true</c> [is dirty].</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public PersonAdapter(IPerson person, bool isDirty)
        {
            IsLazyLoaded = true;
            Person = person;
            IsDirty = isDirty;
        }
    }
}
