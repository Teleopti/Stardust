using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ArchiveController : ApiController
	{
		private readonly IPersonRepository _personRepository;
		private readonly IEventPopulatingPublisher _eventPublisher;

		public ArchiveController(IPersonRepository personRepository, IEventPopulatingPublisher eventPublisher)
		{
			_personRepository = personRepository;
			_eventPublisher = eventPublisher;
		}

		[Route("api/ResourcePlanner/Archiving/Run"), HttpPost, UnitOfWork]
		public virtual IHttpActionResult RunArchiving([FromBody] ArchiveSchedulesModel model)
		{
			var people = _personRepository.LoadAll().ToList();
			var archiveScheduleEvents = people.Select(person => new ArchiveScheduleEvent
			{
				StartDate = model.StartDate,
				EndDate = model.EndDate,
				FromScenario = model.FromScenario,
				ToScenario = model.ToScenario,
				PersonId = person.Id.GetValueOrDefault(),
				TrackingId = model.TrackId
			}).Cast<IEvent>().ToArray();
			_eventPublisher.Publish(archiveScheduleEvents);

			return Ok(new ArchiveSchedulesResponse
			{
				TotalMessages = people.Count
			});
		}
	}

	public class ArchiveSchedulesResponse
	{
		public int TotalMessages { get; set; }
	}

	public class ArchiveSchedulesModel
	{
		public Guid FromScenario { get; set; }
		public Guid ToScenario { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid TrackId { get; set; }
				
	}
}