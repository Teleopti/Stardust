using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Messages.Denormalize
{
	/// <summary>
	/// Schedule needs to be redenormalized for one person.
	/// </summary>
	public abstract class ScheduleDenormalizeBase : RaptorDomainMessage
	{
		private readonly Guid _messageId = Guid.NewGuid();

		/// <summary>
		/// Gets the message identity.
		/// </summary>
		public override Guid Identity
		{
			get { return _messageId; }
		}

		/// <summary>
		/// Gets or sets the start date time.
		/// </summary>
		public DateTime StartDateTime { get; set; }

		/// <summary>
		/// Gets or sets the end date time.
		/// </summary>
		public DateTime EndDateTime { get; set; }

		/// <summary>
		/// Gets or sets the scenario id.
		/// </summary>
		public Guid ScenarioId { get; set; }

		/// <summary>
		/// Gets or sets the person id.
		/// </summary>
		public Guid PersonId { get; set; }

		///<summary>
		/// Gets or sets the skip delete option to be used in the initial load.
		///</summary>
		public bool SkipDelete { get; set; }
	}

	/// <summary>
	/// Schedule have changed for one person.
	/// </summary>
	[Serializable]
	public class ScheduleChanged : ScheduleDenormalizeBase
	{
	}

	/// <summary>
	/// Schedule day read model needs to be initialized for one person.
	/// </summary>
	[Serializable]
	public class ScheduleDayInitialize : ScheduleDenormalizeBase
	{
	}

	/// <summary>
	/// Person schedule day read model needs to be initialized for one person.
	/// </summary>
	[Serializable]
	public class PersonScheduleDayInitialize : ScheduleDenormalizeBase
	{
	}

	/// <summary>
	/// Schedule day projection read model needs to be initialized for one person.
	/// </summary>
	[Serializable]
	public class ScheduleProjectionInitialize : ScheduleDenormalizeBase
	{
	}

	/// <summary>
	/// A denormalized schedule day
	/// </summary>
	public class DenormalizedScheduleDay
	{
		/// <summary>
		/// Team id
		/// </summary>
		public Guid TeamId { get; set; }
		/// <summary>
		/// Site id
		/// </summary>
		public Guid SiteId { get; set; }
		
		/// <summary>
		/// Date
		/// </summary>
		public DateTime Date { get; set; }
		/// <summary>
		/// The work time
		/// </summary>
		public TimeSpan WorkTime { get; set; }
		/// <summary>
		/// The contract time
		/// </summary>
		public TimeSpan ContractTime { get; set; }
		/// <summary>
		/// The label
		/// </summary>
		public string Label { get; set; }
		/// <summary>
		/// The color
		/// </summary>
		public int DisplayColor { get; set; }
		/// <summary>
		/// Is this a work day
		/// </summary>
		public bool IsWorkday { get; set; }
		/// <summary>
		/// Optional shift start
		/// </summary>
		public DateTime? StartDateTime { get; set; }
		/// <summary>
		/// Optional shift end
		/// </summary>
		public DateTime? EndDateTime { get; set; }

		/// <summary>
		/// The layers for this shift
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public ICollection<DenormalizedScheduleProjectionLayer> Layers { get; set; }

		/// <summary>
		/// True if the schedules are removed on that day
		/// </summary>
		public bool NotScheduled { get; set; }
	}

	/// <summary>
	/// Denormalized schedule message
	/// </summary>
	public class DenormalizedScheduleBase : RaptorDomainMessage
	{
		private readonly Guid _messageId = Guid.NewGuid();

		/// <summary>
		/// creates a thingy
		/// </summary>
		public DenormalizedScheduleBase() { IsDefaultScenario = true; }

		/// <summary>
		/// Gets the message identity.
		/// </summary>
		public override Guid Identity
		{
			get { return _messageId; }
		}

		/// <summary>
		/// Is this default scenario
		/// </summary>
		public bool IsDefaultScenario { get; set; }
		/// <summary>
		/// Scenario id
		/// </summary>
		public Guid ScenarioId { get; set; }
		/// <summary>
		/// Person id
		/// </summary>
		public Guid PersonId { get; set; }

		/// <summary>
		/// The layers for this shift
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public ICollection<DenormalizedScheduleDay> ScheduleDays { get; set; }

		/// <summary>
		/// Is this the initial load of read models
		/// </summary>
		public bool IsInitialLoad { get; set; }
	}

	/// <summary>
	/// Denormalized schedule for normal usage
	/// </summary>
	public class DenormalizedSchedule : DenormalizedScheduleBase
	{
	}

	/// <summary>
	/// Denormalized schedule for use in initial load of schedule projection read model
	/// </summary>
	public class DenormalizedScheduleForScheduleProjection : DenormalizedScheduleBase
	{
	}

	/// <summary>
	/// Denormalized schedule for use in initial load of schedule projection read model
	/// </summary>
	public class DenormalizedScheduleForScheduleDay : DenormalizedScheduleBase
	{
	}

	/// <summary>
	/// Denormalized schedule for use in initial load of schedule projection read model
	/// </summary>
	public class DenormalizedScheduleForPersonScheduleDay : DenormalizedScheduleBase
	{
	}

	/// <summary>
	/// A denormalized shift layer
	/// </summary>
	public class DenormalizedScheduleProjectionLayer
	{
		/// <summary>
		/// The payload id
		/// </summary>
		public Guid PayloadId { get; set; }
		/// <summary>
		/// The layer start time
		/// </summary>
		public DateTime StartDateTime { get; set; }
		/// <summary>
		/// The layer end time
		/// </summary>
		public DateTime EndDateTime { get; set; }
		/// <summary>
		/// The layer work time
		/// </summary>
		public TimeSpan WorkTime { get; set; }
		/// <summary>
		/// The layer contract time
		/// </summary>
		public TimeSpan ContractTime { get; set; }
		/// <summary>
		/// The name of the payload
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// The short name of the payload
		/// </summary>
		public string ShortName { get; set; }
		/// <summary>
		/// The payroll code of the payload
		/// </summary>
		public string PayrollCode { get; set; }
		/// <summary>
		/// The display color of the payload
		/// </summary>
		public int DisplayColor { get; set; }
		/// <summary>
		/// Is this absence
		/// </summary>
		public bool IsAbsence { get; set; }
	}
}
