using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
	public class CalendarLinkGenerator : ICalendarLinkGenerator
	{
        private readonly IRepositoryFactory _repositoryFactory;
		private readonly IDataSourcesProvider _dataSourcesProvider;
	    private readonly ICalendarTransformer _transformer;
	    private readonly IFindSharedCalendarScheduleDays _findSharedCalendarScheduleDays;
	    private readonly ICheckCalendarPermissionCommand _checkCalendarPermissionCommand;
	    private readonly ICheckCalendarActiveCommand _checkCalendarActiveCommand;
	    
		public CalendarLinkGenerator(IRepositoryFactory repositoryFactory, IDataSourcesProvider dataSourcesProvider,
		                             ICalendarTransformer transformer, IFindSharedCalendarScheduleDays findSharedCalendarScheduleDays,
		                             ICheckCalendarPermissionCommand checkCalendarPermissionCommand, ICheckCalendarActiveCommand checkCalendarActiveCommand)
		{
			_repositoryFactory = repositoryFactory;
			_dataSourcesProvider = dataSourcesProvider;
		    _transformer = transformer;
		    _findSharedCalendarScheduleDays = findSharedCalendarScheduleDays;
		    _checkCalendarPermissionCommand = checkCalendarPermissionCommand;
		    _checkCalendarActiveCommand = checkCalendarActiveCommand;
		}

		public string Generate(CalendarLinkId calendarLinkId)
		{
			var dataSource = _dataSourcesProvider.RetrieveDataSourceByName(calendarLinkId.DataSourceName);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRepository = _repositoryFactory.CreatePersonRepository(uow);
				var person = personRepository.Get(calendarLinkId.PersonId);

                _checkCalendarPermissionCommand.Execute(dataSource, person, personRepository);
				_checkCalendarActiveCommand.Execute(uow, person);

				var scheduleDays = _findSharedCalendarScheduleDays.GetScheduleDays(calendarLinkId, uow, person.WorkflowControlSet.SchedulePublishedToDate);
				return _transformer.Transform(scheduleDays);
			}
		}
	}
}