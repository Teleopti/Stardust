using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Analytics.Etl.IntegrationTest.TestData
{
	public class ScheduleDayReadModelSetup: IDataSetup
	{
		public ScheduleDayReadModel Model;

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var period = new DateOnlyPeriod(Model.BelongsToDate, Model.BelongsToDate);
			var rep = new ScheduleDayReadModelRepository(currentUnitOfWork);
			rep.ClearPeriodForPerson(period,Model.PersonId);
			rep.SaveReadModel(Model);
		}
	}
}