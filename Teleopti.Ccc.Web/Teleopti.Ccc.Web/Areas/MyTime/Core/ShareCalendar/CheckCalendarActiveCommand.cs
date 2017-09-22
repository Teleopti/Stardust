using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
    public class CheckCalendarActiveCommand : ICheckCalendarActiveCommand
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public CheckCalendarActiveCommand(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public void Execute(IUnitOfWork uow, IPerson person)
        {
            var personalSettingDataRepository = _repositoryFactory.CreatePersonalSettingDataRepository(uow);
            var calendarLinkSettings =
                new CalendarLinkSettingsPersisterAndProvider(personalSettingDataRepository).GetByOwner(person);
            if (!calendarLinkSettings.IsActive)
                throw new InvalidOperationException("Calendar sharing inactive");
        }
    }
}