using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{

    /// <summary>
    /// Tells which referenced IAggregateRoot is most signifacant.
    /// Used today by MessageBroker to enable more fine grained filters.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-05-11
    /// </remarks>
    public interface IMainReference
    {
        /// <summary>
        /// Gets the referenced "main" root.
        /// </summary>
        /// <value>The main root.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-05-11
        /// </remarks>
        IAggregateRoot MainRoot { get; }
    }
}
