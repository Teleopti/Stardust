using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator
{
	public class ReadModelData
	{
		public Guid PersonId { get; set; }
		public DateOnly Date { get; set; }
		public IList<ScheduleProjectionReadOnlyModel> ScheduleProjectionReadOnly { get; set; }
		public PersonScheduleDayReadModel PersonScheduleDay { get; set; }
		public ScheduleDayReadModel ScheduleDay { get; set; }
	}

	public interface IReadModelFixer
	{
		void FixScheduleProjectionReadOnly(ReadModelData data);
		void FixPersonScheduleDay(ReadModelData data);
		void FixScheduleDay(ReadModelData data);
	}
}