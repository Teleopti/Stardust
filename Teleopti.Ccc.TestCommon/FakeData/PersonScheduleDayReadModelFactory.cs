using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class PersonScheduleDayReadModelFactory
	{

		static public PersonScheduleDayReadModel CreateSimplePersonScheduleDayReadModel(IPerson person, DateOnly date)
		{
			var shift = new Shift
			{
				Projection = new List<SimpleLayer> { new SimpleLayer()}
			};
			var model = new Model
			{
				FirstName = person.Name.FirstName,
				LastName = person.Name.LastName,
				Shift = shift  ,
				Date = date.Date
			};
			return new PersonScheduleDayReadModel
			{
				PersonId = person.Id.Value,
				Start = date.Date,
				Model = JsonConvert.SerializeObject(model),
				FirstName = model.FirstName,
				LastName = model.LastName
			};
			
		}


		static public PersonScheduleDayReadModel CreatePersonScheduleDayReadModelWithSimpleShift(IPerson person, DateOnly date,  IList<SimpleLayer> simpleLayers)		
		{
			var shift = new Shift
			{
				Projection = simpleLayers
			};
			var model = new Model
			{
				FirstName = person.Name.FirstName,
				LastName = person.Name.LastName,
				Shift = shift,
				Date = date.Date
			};
			return new PersonScheduleDayReadModel
			{
				PersonId = person.Id.Value,
				TeamId = person.MyTeam(date).Id.Value,
				Start = simpleLayers.Any()? simpleLayers.Min( x => x.Start): date.Date,	
				End = simpleLayers.Any()? simpleLayers.Max( x => x.End): date.Date.Add(new TimeSpan(23,59,59)),
				Model = JsonConvert.SerializeObject(model),
				FirstName = model.FirstName,
				LastName = model.LastName ,
				Date = date.Date,
				MinStart = simpleLayers.Any()? simpleLayers.Min( x => x.Start): (DateTime?)null,
				IsDayOff = model.Shift.IsFullDayAbsence
			};		



		//	public Guid PersonId { get; set; }
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
}
