using System;
using System.Collections.Generic;
using System.Threading;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
	public class CalendarLinkGenerator : ICalendarLinkGenerator
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IDataSourcesProvider _dataSourcesProvider;
		private readonly IJsonDeserializer _deserializer;
		private readonly INow _now;
		private readonly ICurrentPrincipalContext _currentPrincipalContext;
		private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;
		private readonly IPermissionProvider _permissionProvider;
		private const int start = -60;
		private const int end = 180;

		public CalendarLinkGenerator(IRepositoryFactory repositoryFactory, IDataSourcesProvider dataSourcesProvider,
		                             IJsonDeserializer deserializer, INow now,
		                             ICurrentPrincipalContext currentPrincipalContext,
		                             IRoleToPrincipalCommand roleToPrincipalCommand, IPermissionProvider permissionProvider)
		{
			_repositoryFactory = repositoryFactory;
			_dataSourcesProvider = dataSourcesProvider;
			_deserializer = deserializer;
			_now = now;
			_currentPrincipalContext = currentPrincipalContext;
			_roleToPrincipalCommand = roleToPrincipalCommand;
			_permissionProvider = permissionProvider;
		}

		public string Generate(CalendarLinkId calendarLinkId)
		{
			var dataSource = _dataSourcesProvider.RetrieveDataSourceByName(calendarLinkId.DataSourceName);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRepository = _repositoryFactory.CreatePersonRepository(uow);
				var person = personRepository.Get(calendarLinkId.PersonId);

				checkPermission(person, dataSource, dataSource.Application, personRepository);
				checkStatus(uow, person);

				var scheduleDays = getScheduleDays(calendarLinkId, uow, person.WorkflowControlSet.SchedulePublishedToDate);
				return generateCalendar(scheduleDays);
			}
		}

		private IEnumerable<PersonScheduleDayReadModel> getScheduleDays(CalendarLinkId calendarLinkId, IUnitOfWork uow, DateTime? schedulePublishedToDate)
		{
			var endDate = _now.LocalDateOnly().AddDays(end);
			if (schedulePublishedToDate.HasValue)
			{
				var publishedToDate = new DateOnly(schedulePublishedToDate.Value);
				if (publishedToDate < endDate) 
					endDate = publishedToDate;
			}
			var personScheduleDayReadModelFinder = _repositoryFactory.CreatePersonScheduleDayReadModelFinder(uow);
			var scheduleDays = personScheduleDayReadModelFinder.ForPerson(_now.LocalDateOnly().AddDays(start),
																		  endDate, 
																		  calendarLinkId.PersonId);
			return scheduleDays;
		}

		private void checkStatus(IUnitOfWork uow, IPerson person)
		{
			var personalSettingDataRepository = _repositoryFactory.CreatePersonalSettingDataRepository(uow);
			var calendarLinkSettings =
				new CalendarLinkSettingsPersisterAndProvider(personalSettingDataRepository).GetByOwner(person);
			if (!calendarLinkSettings.IsActive)
				throw new InvalidOperationException("Calendar sharing inactive");
		}

		private void checkPermission(IPerson person, IDataSource dataSource, IUnitOfWorkFactory unitOfWorkFactory, IPersonRepository personRepository)
		{
			_currentPrincipalContext.SetCurrentPrincipal(person, dataSource, null);
			_roleToPrincipalCommand.Execute(Thread.CurrentPrincipal as ITeleoptiPrincipal, unitOfWorkFactory, personRepository);
			if (!_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShareCalendar))
				throw new PermissionException("No permission for calendar sharing");
		}

		private string generateCalendar(IEnumerable<PersonScheduleDayReadModel> scheduleDays)
		{
			var iCal = new iCalendar { ProductID = "Teleopti" };

			foreach (var scheduleDay in scheduleDays)
			{
				var shift = _deserializer.DeserializeObject<Model>(scheduleDay.Model).Shift;
				foreach (var layer in shift.Projection)
				{
					var evt = iCal.Create<Event>();

					evt.Start = new iCalDateTime(layer.Start);
					evt.End = new iCalDateTime(layer.End);
					evt.Summary = layer.Description;
				}
			}

			var serializer = new iCalendarSerializer();
			var icsContent = serializer.SerializeToString(iCal);
			return icsContent;
		}
	}
}