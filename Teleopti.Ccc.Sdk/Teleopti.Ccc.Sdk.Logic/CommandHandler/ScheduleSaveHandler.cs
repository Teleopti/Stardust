using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public interface IScheduleSaveHandler
	{
		void ProcessSave(IScheduleDay scheduleDay, INewBusinessRuleCollection rules, IScheduleTag scheduleTagEntity);
	}

	public class ScheduleSaveHandler : IScheduleSaveHandler
	{
		private readonly ISaveSchedulePartService _saveSchedulePartService;

		public ScheduleSaveHandler(ISaveSchedulePartService saveSchedulePartService)
		{
			_saveSchedulePartService = saveSchedulePartService;
		}

		public void ProcessSave(IScheduleDay scheduleDay, INewBusinessRuleCollection rules, IScheduleTag scheduleTagEntity)
		{
			var result = _saveSchedulePartService.Save(scheduleDay, rules, scheduleTagEntity);

			if (result != null)
			{
				var errorMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture,
					"At least one business rule was broken. Messages are: {0}{1}", Environment.NewLine,
					string.Join(Environment.NewLine, result));
				throw new FaultException(errorMessage);
			}
		}
	}
}
