using System;
using System.Collections.Generic;

namespace Teleopti.Wfm.Api.Command
{
	public class SetMainShiftDto : ICommandDto
	{
		public ICollection<ActivityLayerDto> LayerCollection;
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }
		public Guid? ScenarioId { get; set; }
		public Guid ShiftCategory { get; set; }
	}
}