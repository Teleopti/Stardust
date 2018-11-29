using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


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
				Id = person.Id.ToString(),
				FirstName = person.Name.FirstName,
				LastName = person.Name.LastName,
				Shift = shift  ,
				Date = date.Date,
				EmploymentNumber = ""
			};
			return new PersonScheduleDayReadModel
			{
				PersonId = person.Id.Value,
				Date = date.Date,				
				Model = JsonConvert.SerializeObject(model),
				FirstName = model.FirstName,
				LastName = model.LastName
			};
			
		}

		static public PersonScheduleDayReadModel CreateDayOffPersonScheduleDayReadModel(IPerson person, DateOnly date)
		{
			var dayOff = new DayOff
			{
				Start = date.Add(new TimeSpan(8, 0, 0)).Date,
				End = date.Add(new TimeSpan(17, 0, 0)).Date,
				Title = "Day Off"
			};

			var shift = new Shift
			{
				Projection = new List<SimpleLayer>(),
				IsFullDayAbsence = false
			};
		
			var model = new Model
			{
				FirstName = person.Name.FirstName,
				LastName = person.Name.LastName,
				DayOff = dayOff,
				Shift = shift,
				Date = date.Date		
			};
			return new PersonScheduleDayReadModel
			{
				PersonId = person.Id.Value,			
				Start = dayOff.Start.Date,
				End = dayOff.End.Date,
				Model = JsonConvert.SerializeObject(model),
				FirstName = model.FirstName,
				LastName = model.LastName,
				Date = date.Date,
				MinStart = dayOff.Start.Date,
				IsDayOff = true
			};		
		}


		static public PersonScheduleDayReadModel CreatePersonScheduleDayReadModelWithSimpleShift(IPerson person, DateOnly date,  IList<SimpleLayer> simpleLayers, bool isFulldayAbsence = false)
		{
			var contractTimeMinutes = (int)simpleLayers.Last().End.Subtract(simpleLayers.First().Start).TotalMinutes;
			var shift = new Shift
			{
				Projection = simpleLayers,
				ContractTimeMinutes = contractTimeMinutes,
				IsFullDayAbsence = isFulldayAbsence
			};
			var model = new Model
			{
				Id = person.Id.ToString(),
				FirstName = person.Name.FirstName,
				LastName = person.Name.LastName,
				Shift = shift,
				Date = date.Date,
				EmploymentNumber = person.EmploymentNumber,
			};
			return new PersonScheduleDayReadModel
			{
				PersonId = person.Id.GetValueOrDefault(),			
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
