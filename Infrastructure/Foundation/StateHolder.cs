using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class MessageBrokerInStateHolder
	{
		public static IMessageBrokerComposite Instance { get; set; }
	}

	public class StateHolder : StateHolderReader
	{
		private static readonly object _locker = new object();

		private readonly IState _state;

		private StateHolder(IState state)
		{
			_state = state;
		}

		public static void Initialize(IState clientCache, IMessageBrokerComposite messageBroker)
		{
			InParameter.NotNull(nameof(clientCache), clientCache);
			if (InstanceInternal == null)
			{
				lock (_locker)
				{
					if (InstanceInternal != null) return;
					InstanceInternal = new StateHolder(clientCache);
					MessageBrokerInStateHolder.Instance = messageBroker;
				}
			}
			else
				throw new StateHolderException("Singleton StateHolder must only be initialized once per application domain.");
		}

		public new static StateHolder Instance => (StateHolder) StateHolderReader.Instance;

		internal IState State => _state;

		public override IStateReader StateReader => _state;

		public void Terminate()
		{
			_state.ApplicationScopeData.Dispose();
		}
	}
}