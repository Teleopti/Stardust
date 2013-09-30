using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
	public class PersonScheduleDayReadModel : IPersonScheduleDayReadModel
	{
		public Guid PersonId { get; set; }
		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public DateTime Date { get; set; }
		public DateOnly BelongsToDate { get { return new DateOnly(Date); } }
		public DateTime? ShiftStart { get; set; }
		public DateTime? ShiftEnd { get; set; }
		public string Shift { get; set; }
	}

	public class Model
	{
		public string Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string EmploymentNumber { get; set; }
		public DateTime Date { get; set; }
		public Shift Shift { get; set; }
	}

	public class Shift
	{
		public int WorkTimeMinutes { get; set; }
		public int ContractTimeMinutes { get; set; }
		public IList<SimpleLayer> Projection { get; set; }
	}

	public class SimpleLayer
	{
		public string Color { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public int Minutes { get; set; }
		public string Title { get; set; }
		public bool IsAbsenceConfidential { get; set; }
	}

}