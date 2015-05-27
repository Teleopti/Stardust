using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
	public interface IEtlDayOffSubStep
	{
		int StageAndPersistToMart(DayOffEtlLoadSource loadSource, IBusinessUnit businessUnit, IRaptorRepository repository);
	}
}
