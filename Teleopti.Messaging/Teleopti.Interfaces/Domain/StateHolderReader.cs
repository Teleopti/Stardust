using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Expose parts of StateHolder to Domain assembly
    /// </summary>
    /// <remarks>
    /// Tested from StateHolderTest in infrastructuretest
    /// </remarks>
    public abstract class StateHolderReader
    {
        private static StateHolderReader _instanceInternal;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-12-14
        /// </remarks>
        public static StateHolderReader Instance
        {
            get
            {
                if (_instanceInternal == null)
                    throw new InvalidOperationException("StateHolder not initialized correctly.");
                return InstanceInternal;
            }
        }

        /// <summary>
        /// Gets or sets the instance internal.
        /// </summary>
        /// <value>The instance internal.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-12-14
        /// </remarks>
        protected static StateHolderReader InstanceInternal
        {
            get { return _instanceInternal; }
            set { _instanceInternal = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is initialized; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-12-14
        /// </remarks>
        public static bool IsInitialized
        {
            get { return (_instanceInternal != null && _instanceInternal.StateReader.ApplicationScopeData!=null); }
        }

        /// <summary>
        /// Gets the statereader.
        /// </summary>
        /// <value>The state.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-12-14
        /// </remarks>
        public abstract IStateReader StateReader { get; }

    }
}