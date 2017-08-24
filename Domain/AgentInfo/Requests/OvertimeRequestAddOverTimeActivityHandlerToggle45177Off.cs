using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class OvertimeRequestAddOverTimeActivityHandlerToggle45177Off : IRequestAddOverTimeActivityHandler
	{
		private readonly ICommandDispatcher _commandDispatcher;

		public OvertimeRequestAddOverTimeActivityHandlerToggle45177Off(ICommandDispatcher commandDispatcher)
		{
			_commandDispatcher = commandDispatcher;
		}
		public void Handle(Guid activityId, IOvertimeRequest overtimeRequest)
		{
			var agentDateTime = TimeZoneHelper.ConvertFromUtc(overtimeRequest.Period.StartDateTime, overtimeRequest.Person.PermissionInformation.DefaultTimeZone());
			_commandDispatcher.Execute(new AddOvertimeActivityCommand
			{
				ActivityId = activityId,
				Date = new DateOnly(agentDateTime),
				MultiplicatorDefinitionSetId = overtimeRequest.MultiplicatorDefinitionSet.Id.GetValueOrDefault(),
				Period = overtimeRequest.Period,
				PersonId = overtimeRequest.Person.Id.GetValueOrDefault()
			});
		}
	}
}