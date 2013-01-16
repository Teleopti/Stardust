using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class PersonScheduleDayReadModelsCreator : IPersonScheduleDayReadModelsCreator
	{
		private readonly IPersonRepository _personRepository;

		public PersonScheduleDayReadModelsCreator(IPersonRepository personRepository)
		{
			_personRepository = personRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public PersonScheduleDayReadModel GetReadModels(DenormalizedScheduleBase schedule)
		{
			var person = _personRepository.Get(schedule.PersonId);
			var ret = new PersonScheduleDayReadModel();

			ret.PersonId = schedule.PersonId;
			ret.TeamId = schedule.TeamId;
			ret.SiteId = schedule.SiteId;
			ret.BusinessUnitId = schedule.BusinessUnitId;
			ret.Date = schedule.Date;

			if (schedule.StartDateTime.HasValue && schedule.EndDateTime.HasValue)
			{
				ret.ShiftStart = schedule.StartDateTime;
				ret.ShiftEnd = schedule.EndDateTime;
			}

			var shift = new Shift
			{
				Date = schedule.Date,
				FirstName = person.Name.FirstName,
				LastName = person.Name.LastName,
				EmploymentNumber = person.EmploymentNumber,
				Id = schedule.PersonId.ToString(),
				ContractTimeMinutes = (int) schedule.ContractTime.TotalMinutes,
				WorkTimeMinutes = (int)schedule.WorkTime.TotalMinutes,
				Projection = new List<SimpleLayer>()
			};

			foreach (var layer in schedule.Layers)
			{
				shift.Projection.Add(new SimpleLayer
				                     	{
				                     		Color = ColorTranslator.ToHtml(Color.FromArgb(layer.DisplayColor)),
				                     		Title = layer.Name,
				                     		Start = layer.StartDateTime,
				                     		End = layer.EndDateTime,
				                     		Minutes = (int) layer.EndDateTime.Subtract(layer.StartDateTime).TotalMinutes
				                     	});
			}

			ret.Shift = Newtonsoft.Json.JsonConvert.SerializeObject(shift);

			return ret;
		}
	}
	
	public class Shift
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string EmploymentNumber { get; set; }
		public string Id { get; set; }
		public DateTime Date { get; set; }
		public int WorkTimeMinutes { get; set; }
		public int ContractTimeMinutes { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IList<SimpleLayer> Projection { get; set; }
	}

	public class SimpleLayer
	{
		public string Color { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public int Minutes { get; set; }
		public string Title { get; set; }
	}

	public interface IPersonScheduleDayReadModelsCreator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		PersonScheduleDayReadModel GetReadModels(DenormalizedScheduleBase schedule);
	}
}