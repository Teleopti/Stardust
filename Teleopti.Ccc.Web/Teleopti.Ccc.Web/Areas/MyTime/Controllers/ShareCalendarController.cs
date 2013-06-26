using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
    public class ShareCalendarController : Controller
    {
	    private readonly IRepositoryFactory _repositoryFactory;
	    private readonly IDataSourcesProvider _dataSourcesProvider;
	    private readonly IJsonDeserializer<ExpandoObject> _deserializer;

	    public ShareCalendarController(IRepositoryFactory repositoryFactory, IDataSourcesProvider dataSourcesProvider, IJsonDeserializer<ExpandoObject> deserializer)
	    {
		    _repositoryFactory = repositoryFactory;
		    _dataSourcesProvider = dataSourcesProvider;
		    _deserializer = deserializer;
	    }

		[HttpGet]
		public ContentResult iCal(string dataSourceName, Guid publishedId)
		{
			var dataSource = _dataSourcesProvider.RetrieveDataSourceByName(dataSourceName);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRepository = _repositoryFactory.CreatePersonRepository(uow);
				var person = personRepository.Get(publishedId);
				if (person == null)
					return null;
				var personScheduleDayReadModelFinder = _repositoryFactory.CreatePersonScheduleDayReadModelFinder(uow);
				var scheduleDays = personScheduleDayReadModelFinder.ForPerson(DateOnly.Today.AddDays(-60),
																		   DateOnly.Today.AddDays(180), publishedId);
				if (scheduleDays == null || scheduleDays.IsEmpty())
					return null;
				foreach (var scheduleDay in scheduleDays)
				{
					dynamic shift = _deserializer.DeserializeObject(scheduleDay.Shift);
					if (shift == null)
						return null;
					var layers = shift.Projection as IEnumerable<dynamic>;
					layers.ForEach(l =>
					{
						l.TimeZoneInfo = person.PermissionInformation.DefaultTimeZone();
					});
					foreach (var layer in layers)
					{
						var b = layer;
					}
				}
			}
			
			var a = 
@"BEGIN:VCALENDAR
PRODID:-//Microsoft Corporation//Outlook 14.0 MIMEDIR//EN
VERSION:2.0
METHOD:PUBLISH
X-CALSTART:20130619T073000Z
X-CALEND:20130619T123000Z
X-CLIPSTART:20130618T220000Z
X-CLIPEND:20130619T220000Z
X-WR-RELCALID:{0000002E-5A57-F9BA-4C4D-002770131FD3}
X-WR-CALNAME:Kunning Mao
X-PRIMARY-CALENDAR:TRUE
X-OWNER;CN=""Kunning Mao"":mailto:Kunning.Mao@teleopti.com
X-MS-OLK-WKHRSTART;TZID=""W. Europe Standard Time"":080000
X-MS-OLK-WKHREND;TZID=""W. Europe Standard Time"":233000
X-MS-OLK-WKHRDAYS:SU,MO,TU,WE,TH,FR,SA
BEGIN:VTIMEZONE
TZID:W. Europe Standard Time
BEGIN:STANDARD
DTSTART:16011028T030000
RRULE:FREQ=YEARLY;BYDAY=-1SU;BYMONTH=10
TZOFFSETFROM:+0200
TZOFFSETTO:+0100
END:STANDARD
BEGIN:DAYLIGHT
DTSTART:16010325T020000
RRULE:FREQ=YEARLY;BYDAY=-1SU;BYMONTH=3
TZOFFSETFROM:+0100
TZOFFSETTO:+0200
END:DAYLIGHT
END:VTIMEZONE
BEGIN:VEVENT
DTEND:20130619T074500Z
DTSTAMP:20130619T114259Z
DTSTART:20130619T073000Z
SEQUENCE:0
SUMMARY;LANGUAGE=sv:Busy
TRANSP:OPAQUE
UID:5oAdlkTvSk+zFtUJ32Tl+Q==
X-MICROSOFT-CDO-BUSYSTATUS:BUSY
END:VEVENT
BEGIN:VEVENT
DTEND:20130619T123000Z
DTSTAMP:20130619T114259Z
DTSTART:20130619T110000Z
SEQUENCE:0
SUMMARY;LANGUAGE=sv:Busy
TRANSP:OPAQUE
UID:OXiSPtibBUuxhjgyLqwheg==
X-MICROSOFT-CDO-BUSYSTATUS:BUSY
END:VEVENT
END:VCALENDAR
";

			
			return Content(a, "text/plain");

        }

    }
}
