using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class RecalculateForecastOnSkillCommandHandler : IHandleCommand<RecalculateForecastOnSkillCommandDto>
	{
		public CommandResultDto Handle(RecalculateForecastOnSkillCommandDto command)
		{
			var message = new RecalculateForecastOnSkillMessage(null);
			throw new System.NotImplementedException();
		}
	}
}