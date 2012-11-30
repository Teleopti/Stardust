using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
	public interface IEtlDayOffSubStep
	{
		int StageAndPersistToMart(DayOffEtlLoadSource loadSource, IBusinessUnit businessUnit, IRaptorRepository repository);
	}
}
