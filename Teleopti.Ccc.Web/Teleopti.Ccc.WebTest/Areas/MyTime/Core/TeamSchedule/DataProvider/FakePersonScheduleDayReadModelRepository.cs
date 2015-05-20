using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public class FakePersonScheduleDayReadModelRepository
	{
		private IList<PersonScheduleDayReadModel> _personScheduleDayReadModels = new List<PersonScheduleDayReadModel>();
		
		public void Add(PersonScheduleDayReadModel readModel)
		{
			_personScheduleDayReadModels.Add(readModel);			
		}

		public IList<PersonScheduleDayReadModel> LoadAll()
		{
			return _personScheduleDayReadModels;
		}
		//public Guid PersonId { get; set; }
		//public Guid TeamId { get; set; }
		//public Guid SiteId { get; set; }
		//public Guid BusinessUnitId { get; set; }
		//public string FirstName { get; set; }
		//public string LastName { get; set; }
		//public DateTime Date { get; set; }
		//public DateOnly BelongsToDate { get { return new DateOnly(Date); } }
		//public DateTime? Start { get; set; }
		//public DateTime? End { get; set; }
		//public bool IsDayOff { get; set; }
		//public string Model { get; set; }
		//public Guid? ShiftExchangeOffer { get; set; }
		//public DateTime? MinStart { get; set; }
		//public bool IsLastPage { get; set; }
		//public int Total { get; set; }
	}
}
