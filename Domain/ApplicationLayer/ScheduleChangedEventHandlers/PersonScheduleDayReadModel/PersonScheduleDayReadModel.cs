using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public class PersonScheduleDayReadModel : IPersonScheduleDayReadModel, IEquatable<PersonScheduleDayReadModel>
	{
		public Guid PersonId { get; set; }	
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public DateTime Date { get; set; }
		public DateOnly BelongsToDate => new DateOnly(Date);
		public DateTime? Start { get; set; }
		public DateTime? End { get; set; }
		public bool IsDayOff { get; set; }
		public string Model { get; set; }
		public Guid? ShiftExchangeOffer { get; set; }
		public DateTime? MinStart { get; set; }
		public bool IsLastPage { get; set; }
		public int Total { get; set; }
		public DateTime ScheduleLoadTimestamp { get; set; }
		public int? Version { get; set; }

		public bool Equals(PersonScheduleDayReadModel other)
		{
			if (other == null) return false;
			return PersonId == other.PersonId && Date == other.Date && 
				   BelongsToDate == other.BelongsToDate && Start == other.Start &&
				   End == other.End && IsDayOff == other.IsDayOff
				   && Model == other.Model;
		}		
	}

	public class Model
	{
		public string Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string EmploymentNumber { get; set; }
		public DateTime Date { get; set; }
		public Shift Shift { get; set; }
		public DayOff DayOff { get; set; }
	}

	public class DayOff
	{
		public string Title { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
	}

	public class Shift
	{
		public int WorkTimeMinutes { get; set; }
		public int ContractTimeMinutes { get; set; }
		public bool IsFullDayAbsence { get; set; }
		public IList<SimpleLayer> Projection { get; set; }
	}

	public class SimpleLayer 
	{
		public string Color { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public int Minutes { get; set; }
		public string Description { get; set; }
		public bool IsAbsenceConfidential { get; set; }
	}

}