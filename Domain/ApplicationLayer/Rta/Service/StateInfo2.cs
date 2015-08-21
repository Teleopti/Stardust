namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateInfo2
	{
		private readonly StateMapping _stateMapping;
		private readonly StoredStateInfo _stored;

		public StateInfo2(
			StateMapping stateMapping,
			StoredStateInfo stored
			)
		{
			_stateMapping = stateMapping;
			_stored = stored;
		}

		public bool StateGroupChanged()
		{
			return _stateMapping.StateGroupId != _stored.StateGroupId;
		}
	}
}