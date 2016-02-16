using System;

namespace Teleopti.Interfaces.Domain
{
	public abstract class StateHolderReader
	{
		private static StateHolderReader _instanceInternal;

		public static StateHolderReader Instance
		{
			get
			{
				if (InstanceInternal == null)
					throw new InvalidOperationException("StateHolder not initialized correctly.");
				return InstanceInternal;
			}
		}

		protected static StateHolderReader InstanceInternal
		{
			get { return _instanceInternal; }
			set { _instanceInternal = value; }
		}

		public static bool IsInitialized
		{
			get
			{
				return InstanceInternal != null && InstanceInternal.StateReader.ApplicationScopeData != null;
			}
		}

		public abstract IStateReader StateReader { get; }

	}
}