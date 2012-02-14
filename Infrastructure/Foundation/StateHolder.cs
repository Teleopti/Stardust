using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    /// <summary>
    /// A singleton holding an IState object
    /// </summary>
    public class StateHolder : StateHolderReader
    {
        private static readonly object _locker = new object();
        private readonly IState _state;

        private StateHolder(IState state)
        {
            _state = state;
        }
        /// <summary>
        /// Gets the one and only instance of StateHolder.
        /// </summary>
        /// <value>The instance.</value>
        public new static StateHolder Instance
        {
            get { return (StateHolder) StateHolderReader.Instance; }
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>The state.</value>
        internal IState State
        {
            get { return _state; }
        }

        /// <summary>
        /// Gets the statereader.
        /// </summary>
        /// <value>The state.</value>
        public override IStateReader StateReader
        {
            get { return _state; }
        }

        /// <summary>
        /// Initializes the singleton with a specified client cache.
        /// </summary>
        /// <param name="clientCache">The client cache.</param>
        public static void Initialize(IState clientCache)
        {
            InParameter.NotNull("clientCache", clientCache);
            if (InstanceInternal==null)
            {
                lock (_locker)
                {
                    if (InstanceInternal==null)
                        InstanceInternal = new StateHolder(clientCache);
                }
            }
            else
                throw new StateHolderException("Singleton StateHolder must only be initialized once per application domain.");
        }

        /// <summary>
        /// Terminates this instance.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-06-17
        /// </remarks>
        public void Terminate()
        {
            _state.ApplicationScopeData.Dispose();
        }
    }
}