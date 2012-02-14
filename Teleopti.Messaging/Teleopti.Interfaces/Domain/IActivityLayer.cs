using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// A layer of Activity
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-08-07
    /// </remarks>
    public interface IActivityLayer : ILayer<IActivity>
    {
        /// <summary>
        /// Gets the definition set.
        /// </summary>
        /// <value>The definition set.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-02-05
        /// </remarks>
        IMultiplicatorDefinitionSet DefinitionSet { get; }
    }
}