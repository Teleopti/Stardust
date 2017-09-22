using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
	public class CalendarLinkGenerator : ICalendarLinkGenerator
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IDataSourceForTenant _dataSourceForTenant;
		private readonly ICalendarTransformer _transformer;
		private readonly IFindSharedCalendarScheduleDays _findSharedCalendarScheduleDays;
		private readonly ICheckCalendarPermissionCommand _checkCalendarPermissionCommand;
		private readonly ICheckCalendarActiveCommand _checkCalendarActiveCommand;
		private readonly IDataSourceScope _dataSourceScope;

		public CalendarLinkGenerator(IRepositoryFactory repositoryFactory, IDataSourceForTenant dataSourceForTenant,
								 ICalendarTransformer transformer, IFindSharedCalendarScheduleDays findSharedCalendarScheduleDays,
								 ICheckCalendarPermissionCommand checkCalendarPermissionCommand, ICheckCalendarActiveCommand checkCalendarActiveCommand, IDataSourceScope dataSourceScope)
		{
			_repositoryFactory = repositoryFactory;
			_dataSourceForTenant = dataSourceForTenant;
			_transformer = transformer;
			_findSharedCalendarScheduleDays = findSharedCalendarScheduleDays;
			_checkCalendarPermissionCommand = checkCalendarPermissionCommand;
			_checkCalendarActiveCommand = checkCalendarActiveCommand;
			_dataSourceScope = dataSourceScope;
		}

		public string Generate(CalendarLinkId calendarLinkId)
		{
			var dataSource = _dataSourceForTenant.Tenant(calendarLinkId.DataSourceName);

			using (_dataSourceScope.OnThisThreadUse(dataSource))
			{
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
}