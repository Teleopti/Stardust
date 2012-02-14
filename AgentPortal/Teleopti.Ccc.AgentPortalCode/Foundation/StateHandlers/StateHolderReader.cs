using System;

namespace Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers
{
    /// <summary>
    /// This abstract class will used for reading stateholder data
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-03-03
    /// </remarks>
    public abstract class StateHolderReader
    {
        /// <summary>
        /// Keeps the singleton instance
        /// </summary>
        private static StateHolderReader _instanceInternal;

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        public static bool IsInitialized
        {
            get { return (_instanceInternal != null); }
        }

        /// <summary>
        /// Keeps the singleton instance
        /// </summary>
        protected static StateHolderReader InstanceInternal
        {
            get { return _instanceInternal; }
            set { _instanceInternal = value; }
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static StateHolderReader Instance
        {
            get
            {
                if (!IsInitialized)
                    throw new InvalidOperationException("State Holder is not initialized");
                return InstanceInternal;
            }
        }

        /// <summary>
        /// Gets the state reader.
        /// </summary>
        /// <value>The state reader.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-04
        /// </remarks>
        public abstract IStateReader StateReader { get; }
    }
}
