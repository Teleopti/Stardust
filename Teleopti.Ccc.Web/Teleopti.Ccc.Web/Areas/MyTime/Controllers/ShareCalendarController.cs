using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using System.Web.Mvc;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class ShareCalendarController : Controller
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IDataSourcesProvider _dataSourcesProvider;
		private readonly IJsonDeserializer<ExpandoObject> _deserializer;
		private readonly INow _now;
		private readonly ICurrentPrincipalContext _currentPrincipalContext;
		private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;
		private readonly IPermissionProvider _permissionProvider;

		public ShareCalendarController(IRepositoryFactory repositoryFactory, IDataSourcesProvider dataSourcesProvider, IJsonDeserializer<ExpandoObject> deserializer, INow now, ICurrentPrincipalContext currentPrincipalContext, IRoleToPrincipalCommand roleToPrincipalCommand, IPermissionProvider permissionProvider)
		{
			_repositoryFactory = repositoryFactory;
			_dataSourcesProvider = dataSourcesProvider;
			_deserializer = deserializer;
			_now = now;
			_currentPrincipalContext = currentPrincipalContext;
			_roleToPrincipalCommand = roleToPrincipalCommand;
			_permissionProvider = permissionProvider;
		}

		[HttpGet]
		public ContentResult iCal(string id)
		{
			string dataSourceNameAndPersonId;
			try
			{
				dataSourceNameAndPersonId = StringEncryption.Decrypt(id);
			}
			catch (Exception)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return Content("Invalid url", "text/plain");
			}
			var pos = dataSourceNameAndPersonId.LastIndexOf("/", StringComparison.Ordinal);
			var dataSourceName = dataSourceNameAndPersonId.Substring(0, pos);
			var personId = dataSourceNameAndPersonId.Substring(pos + 1);
			var publishedId = new Guid(personId);

			var dataSource = _dataSourcesProvider.RetrieveDataSourceByName(dataSourceName);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRepository = _repositoryFactory.CreatePersonRepository(uow);
				var person = personRepository.Get(publishedId);
				if (person == null)
					return null;
				_currentPrincipalContext.SetCurrentPrincipal(person, dataSource, null);
				_roleToPrincipalCommand.Execute(Thread.CurrentPrincipal as ITeleoptiPrincipal, uow, personRepository);
				if (!_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShareCalendar))
					return Content("No permission for calendar sharing", "text/plain");

				var personalSettingDataRepository = _repositoryFactory.CreatePersonalSettingDataRepository(uow);
				var calendarLinkSettings = personalSettingDataRepository.FindValueByKeyAndOwnerPerson("CalendarLinkSettings", person,
																						  new CalendarLinkSettings());
				if (!calendarLinkSettings.IsActive)
					return Content("Calendar sharing inactive", "text/plain");
				var personScheduleDayReadModelFinder = _repositoryFactory.CreatePersonScheduleDayReadModelFinder(uow);
				var scheduleDays = personScheduleDayReadModelFinder.ForPerson(_now.DateOnly().AddDays(-60),
																		   _now.DateOnly().AddDays(180), publishedId);
				if (scheduleDays == null || scheduleDays.IsEmpty())
					return null;
				return Content(generateCalendar(scheduleDays), "text/plain");
			}
		}

		private string generateCalendar(IEnumerable<PersonScheduleDayReadModel> scheduleDays)
		{
			var iCal = new iCalendar {ProductID = "Teleopti"};

			foreach (var scheduleDay in scheduleDays)
			{
				dynamic shift = _deserializer.DeserializeObject(scheduleDay.Shift);
				if (shift == null)
					continue;
				var layers = shift.Projection as IEnumerable<dynamic>;
				foreach (var layer in layers)
				{
					var evt = iCal.Create<Event>();

					evt.Start = new iCalDateTime(layer.Start);
					evt.End = new iCalDateTime(layer.End);
					evt.Summary = layer.Title;
				}
			}

			var serializer = new iCalendarSerializer();
			var icsContent = serializer.SerializeToString(iCal);
			return icsContent;
		}
	}
}
