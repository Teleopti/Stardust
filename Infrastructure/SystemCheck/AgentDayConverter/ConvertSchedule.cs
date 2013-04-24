using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public class ConvertSchedule
	{
		private readonly IEnumerable<IPersonAssignmentConverter> _converters;

		public ConvertSchedule(IEnumerable<IPersonAssignmentConverter> converters)
		{
			_converters = converters;
		}

		public void ExecuteAllConverters()
		{
			foreach (var converter in _converters)
			{
				converter.Execute();
			}
		}
	}
}