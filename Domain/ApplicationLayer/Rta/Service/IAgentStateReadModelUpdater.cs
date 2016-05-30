namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateReadModelUpdater
	{
		void UpdateState(Context info);
		void UpdateReadModel(Context info);
	}
}