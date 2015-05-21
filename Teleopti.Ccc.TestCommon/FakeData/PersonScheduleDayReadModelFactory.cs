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
				Projection = new List<SimpleLayer>()
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

		}

	}
}
