namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeatMapPersister
	{
		void Save(ISaveSeatMapCommand command);
	}
}