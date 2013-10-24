using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.IntegrationTest.TestData
{
    public class ScheduleDayReadModelSetup: IDataSetup
    {
        public ScheduleDayReadModel Model;

        public void Apply(IUnitOfWork uow)
        {
            var rep = new ScheduleDayReadModelRepository(uow);
            rep.SaveReadModel(Model);
        }
    }
}