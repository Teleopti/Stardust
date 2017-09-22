using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class EtlDayOffSubStep : IEtlDayOffSubStep
	{
		public int StageAndPersistToMart(DayOffEtlLoadSource loadSource, IBusinessUnit businessUnit, IRaptorRepository repository)
		{
			int rowsAffected = 0;

			if (loadSource == DayOffEtlLoadSource.ScheduleDayOff)
				rowsAffected = repository.PersistDayOffFromScheduleDayOffCount();

			if (loadSource == DayOffEtlLoadSource.SchedulePreference)
				rowsAffected = repository.PersistDayOffFromSchedulePreference();

			rowsAffected += repository.FillDayOffDataMart(businessUnit);

			return rowsAffected;
		}
	}
}
