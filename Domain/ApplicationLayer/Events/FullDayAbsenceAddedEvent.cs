using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	[Serializable]
	public class FullDayAbsenceAddedEvent : RaptorDomainEvent
	{
		public Guid AbsenceId { get; set; }
		public Guid PersonId { get; set; }
		public Guid ScenarioId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
	}

	[Serializable]
	public class ActivityChangedEvent : RaptorDomainEvent
	{
		public Guid ActivityId { get; set; }
		public string Property { get; set; }
		public string OldValue { get; set; }
		public string NewValue { get; set; }
	}

	[Serializable]
	public class PersonTerminatedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public DateTime? PreviousTerminationDate { get; set; }
		public DateTime TerminationDate { get; set; }

		public IEnumerable<PersonPeriodDetail> PersonPeriodsBefore { get; set; }
		public IEnumerable<PersonPeriodDetail> PersonPeriodsAfter { get; set; }
	}
	
	[Serializable]
	public class PersonReactivatedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public DateTime PreviousTerminationDate { get; set; }

		public IEnumerable<PersonPeriodDetail> PersonPeriodsBefore { get; set; }
		public IEnumerable<PersonPeriodDetail> PersonPeriodsAfter { get; set; }
	}

	[Serializable]
	public class PersonDeletedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }

		public IEnumerable<PersonPeriodDetail> PersonPeriodsBefore { get; set; }
	}

	[Serializable]
	public class PersonPeriodStartDateChangedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public DateTime OldStartDate { get; set; }
		public DateTime NewStartDate { get; set; }

		public IEnumerable<PersonPeriodDetail> PersonPeriodsBefore { get; set; }
		public IEnumerable<PersonPeriodDetail> PersonPeriodsAfter { get; set; }
	}

	[Serializable]
	public class PersonPeriodRemovedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public DateTime StartDate { get; set; }

		public IEnumerable<PersonPeriodDetail> PersonPeriodsBefore { get; set; }
		public IEnumerable<PersonPeriodDetail> PersonPeriodsAfter { get; set; }
	}

	[Serializable]
	public class PersonPeriodAddedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public DateTime StartDate { get; set; }

		public IEnumerable<PersonPeriodDetail> PersonPeriodsBefore { get; set; }
		public IEnumerable<PersonPeriodDetail> PersonPeriodsAfter { get; set; }
	}

	[Serializable]
	public class PersonSkillDeactivatedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid SkillId { get; set; }

		public IEnumerable<PersonSkillDetail> SkillsBefore { get; set; }
	}

	[Serializable]
	public class PersonSkillAddedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public DateTime EndDate { get; set; }
		public DateTime StartDate { get; set; }
		public Guid SkillId { get; set; }
		public double Proficiency { get; set; }
		public bool SkillActive { get; set; }

		public IEnumerable<PersonSkillDetail> SkillsBefore { get; set; }
	}

	[Serializable]
	public struct PersonSkillDetail
	{
		public Guid SkillId { get; set; }
		public double Proficiency { get; set; }
		public bool Active { get; set; }
	}

	[Serializable]
	public struct PersonPeriodDetail
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid TeamId { get; set; }
		public IEnumerable<PersonSkillDetail> PersonSkillDetails { get; set; }
	}

	[Serializable]
	public class PersonSkillResetEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public DateTime StartDate { get; set; }

		public IEnumerable<PersonSkillDetail> SkillsBefore { get; set; }
	}

	[Serializable]
	public class PersonSkillRemovedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public double Proficiency { get; set; }
		public bool SkillActive { get; set; }
		public Guid SkillId { get; set; }

		public IEnumerable<PersonSkillDetail> SkillsBefore { get; set; }
	}

	[Serializable]
	public class PersonSkillProficiencyChangedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid SkillId { get; set; }
		public double ProficiencyAfter { get; set; }

		public IEnumerable<PersonSkillDetail> SkillsBefore { get; set; }
	}

	[Serializable]
	public class PersonSkillActivatedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid SkillId { get; set; }

		public IEnumerable<PersonSkillDetail> SkillsBefore { get; set; }
	}

	[Serializable]
	public class PersonTeamChangedEvent : RaptorDomainEvent
	{
		public Guid PersonId { get; set; }
		public DateTime StartDate { get; set; }
		public Guid? OldTeam { get; set; }
		public Guid? NewTeam { get; set; }
	}
}