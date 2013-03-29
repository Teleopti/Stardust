﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleHandlers;

namespace Teleopti.Interfaces.Messages.Denormalize
{
	/// <summary>
	/// Denormalized schedule message
	/// </summary>
	public class ProjectionChangedEventBase : RaptorDomainEvent
	{
		private readonly Guid _messageId = Guid.NewGuid();

		/// <summary>
		/// creates a thingy
		/// </summary>
		public ProjectionChangedEventBase() { IsDefaultScenario = true; }

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
		public ICollection<ProjectionChangedEventScheduleDay> ScheduleDays { get; set; }

		/// <summary>
		/// Is this the initial load of read models
		/// </summary>
		public bool IsInitialLoad { get; set; }
	}
}