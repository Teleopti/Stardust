using System;
using System.Drawing;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
	public class ScheduleDayReadModel : IEquatable<ScheduleDayReadModel>
	{
		private string _label;

		public ScheduleDayReadModel()
		{
			StartDateTime = new DateTime(1900, 1, 1);
			EndDateTime = new DateTime(1900, 1, 1);
		}
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }
		public DateOnly BelongsToDate { get { return new DateOnly(Date); } }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public bool Workday { get; set; }
		public bool NotScheduled { get; set; }
		public string Label
		{
			get
			{
				if (_label == null)
					return "";
				return _label;
			}
			set { _label = value; }
		}

		public int ColorCode { get; set; }
		public Color DisplayColor { get { return Color.FromArgb(ColorCode); } }
		public long WorkTimeTicks { get; set; }
		public TimeSpan WorkTime { get { return TimeSpan.FromTicks(WorkTimeTicks); } }
		public long ContractTimeTicks { get; set; }
		public TimeSpan ContractTime { get { return TimeSpan.FromTicks(ContractTimeTicks); } }
		public int Version { get; set; }

		public bool Equals(ScheduleDayReadModel other)
		{
			if (other == null) return false;
			return PersonId == other.PersonId
				   && Date == other.Date
				   && StartDateTime == other.StartDateTime
				   && EndDateTime == other.EndDateTime
				   && Workday == other.Workday
				   && NotScheduled == other.NotScheduled
				   && Label == other.Label;
		}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}