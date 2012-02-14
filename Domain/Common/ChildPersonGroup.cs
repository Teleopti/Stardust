using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Represents a Child Person Group in the hiararchy.
    /// </summary>
    public class ChildPersonGroup : PersonGroupBase, IChildPersonGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChildPersonGroup"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-27
        /// </remarks>
        public ChildPersonGroup()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildPersonGroup"/> class.
        /// </summary>
        /// <param name="description"></param>
        public ChildPersonGroup(string description) : base(description) { }
    }
}
