using System;
using System.Collections.Generic;
using System.Drawing;

namespace Teleopti.Interfaces.Messages.Denormalize
{
	/// <summary>
	/// Denormalize the projection of schedules.
	/// </summary>
	[Serializable]
	public class DenormalizeScheduleProjection : RaptorDomainMessage
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

	public class DenormalizedSchedule : RaptorDomainMessage
	{
		private readonly Guid _messageId = Guid.NewGuid();

		/// <summary>
		/// Gets the message identity.
		/// </summary>
		public override Guid Identity
		{
			get { return _messageId; }
		}

		public bool IsDefaultScenario { get; set; }
		public Guid ScenarioId { get; set; }
		public Guid PersonId { get; set; }
		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
		public DateTime Date { get; set; }
		public TimeSpan WorkTime { get; set; }
		public TimeSpan ContractTime { get; set; }
		public string Label { get; set; }
		public Color DisplayColor { get; set; }
		public bool IsWorkDay { get; set; }
		public DateTime? StartDateTime { get; set; }
		public DateTime? EndDateTime { get; set; }

		public ICollection<DenormalizedScheduleProjectionLayer> Layers { get; set; }

		public bool IsInitialLoad { get; set; }
	}

	public class DenormalizedScheduleProjectionLayer
	{
		public Guid PayloadId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public TimeSpan WorkTime { get; set; }
		public TimeSpan ContractTime { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
		public string PayrollCode { get; set; }
		public int DisplayColor { get; set; }
		public bool IsAbsence { get; set; }
	}
}
