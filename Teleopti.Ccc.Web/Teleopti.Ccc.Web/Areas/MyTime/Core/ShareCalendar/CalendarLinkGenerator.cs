using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
	public class CalendarLinkGenerator : ICalendarLinkGenerator
	{
        private readonly IRepositoryFactory _repositoryFactory;
		private readonly IApplicationData _applicationData;
		private readonly ICalendarTransformer _transformer;
	    private readonly IFindSharedCalendarScheduleDays _findSharedCalendarScheduleDays;
	    private readonly ICheckCalendarPermissionCommand _checkCalendarPermissionCommand;
	    private readonly ICheckCalendarActiveCommand _checkCalendarActiveCommand;

			public CalendarLinkGenerator(IRepositoryFactory repositoryFactory, IApplicationData applicationData,
		                             ICalendarTransformer transformer, IFindSharedCalendarScheduleDays findSharedCalendarScheduleDays,
		                             ICheckCalendarPermissionCommand checkCalendarPermissionCommand, ICheckCalendarActiveCommand checkCalendarActiveCommand)
		{
			_repositoryFactory = repositoryFactory;
				_applicationData = applicationData;
				_transformer = transformer;
		    _findSharedCalendarScheduleDays = findSharedCalendarScheduleDays;
		    _checkCalendarPermissionCommand = checkCalendarPermissionCommand;
		    _checkCalendarActiveCommand = checkCalendarActiveCommand;
		}

		public string Generate(CalendarLinkId calendarLinkId)
		{
			var dataSource = _applicationData.DataSource(calendarLinkId.DataSourceName);

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