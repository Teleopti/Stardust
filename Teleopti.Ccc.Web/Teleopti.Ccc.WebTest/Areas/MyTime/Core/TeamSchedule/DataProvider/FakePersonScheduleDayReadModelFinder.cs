using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	class FakePersonScheduleDayReadModelFinder : IPersonScheduleDayReadModelFinder
	{

		//private IPersonRepository _personRepository;

		private IPersonAssignmentRepository _personAssignmentRepository;

		public FakePersonScheduleDayReadModelFinder(IPersonAssignmentRepository personAssignmentRepository)
		{
			_personAssignmentRepository = personAssignmentRepository;			
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPerson(DateOnly startDate, DateOnly endDate, Guid personId)
		{
			throw new NotImplementedException();
		}

		public PersonScheduleDayReadModel ForPerson(DateOnly date, Guid personId)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPeople(DateTimePeriod period, IEnumerable<Guid> personIds)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForBulletinPersons(IEnumerable<string> shiftExchangeOfferIdList, Paging paging)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPersons(DateOnly date, IEnumerable<Guid> personIdList, Paging paging, TimeFilterInfo filterInfo = null,
			string timeSortOrder = "")
		{

			var assignments = _personAssignmentRepository.LoadAll().Where(a =>
			{
				var timeFiltered = true;
				if (filterInfo != null)
				{
					if (filterInfo.StartTimes.Any() && !filterInfo.StartTimes.Any( period => period.Contains(a.Period.StartDateTime)))
					{
						timeFiltered = false;
					}

					if (filterInfo.EndTimes.Any() && !filterInfo.EndTimes.Any(period => period.Contains(a.Period.EndDateTime)))
					{
						timeFiltered = false;
					}
				}
				return personIdList.Contains(a.Person.Id.Value) && date == a.Date && timeFiltered;
			});

			return assignments.Select(a =>
			{
				var simpleLayers = a.ShiftLayers.Select(shiftLayer => new SimpleLayer()
				{
					Start = shiftLayer.Period.StartDateTime,
					End = shiftLayer.Period.EndDateTime,
					Description = shiftLayer.Payload.Name
				}).ToList();

				return PersonScheduleDayReadModelFactory.CreatePersonScheduleDayReadModelWithSimpleShift(a.Person, a.Date, simpleLayers);
			});

			//return _personRepository.LoadAll()
			//	.Where( p => personIdList.Contains(p.Id.Value))
			//	.Select(PersonScheduleDayReadModelFactory.CreateSimplePersonScheduleDayReadModel);
		}
	}
}
