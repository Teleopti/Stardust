using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Represents a Root Person Group in the hiararchy.
    /// </summary>
    public class RootPersonGroup : PersonGroupBase, IRootPersonGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RootPersonGroup"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-06-26
        /// </remarks>
        public RootPersonGroup()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RootPersonGroup"/> class.
        /// </summary>
        /// <param name="description"></param>
        public RootPersonGroup(string description) : base(description) { }
    }
}
