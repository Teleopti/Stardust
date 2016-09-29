using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class EditScheduleNoteCommandHandler:IHandleCommand<EditScheduleNoteCommand>
	{
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IScheduleDayProvider _scheduleDayProvider;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _diffreenceService;

		public EditScheduleNoteCommandHandler(IProxyForId<IPerson> personForId, IScheduleDayProvider scheduleDayProvider, IScheduleDifferenceSaver scheduleDifferenceSaver, IDifferenceCollectionService<IPersistableScheduleData> diffreenceService)
		{
			_personForId = personForId;
			_scheduleDayProvider = scheduleDayProvider;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_diffreenceService = diffreenceService;
		}

		public void Handle(EditScheduleNoteCommand command)
		{
			var person = _personForId.Load(command.PersonId);
			var scheduleDay = _scheduleDayProvider.GetScheduleDay(command.Date, person, new ScheduleDictionaryLoadOptions(true, true));
			var scheduleDic = _scheduleDayProvider.GetScheduleDictionary(command.Date, person, new ScheduleDictionaryLoadOptions(true, true));

			var note = scheduleDay.NoteCollection().FirstOrDefault();
			if (!string.IsNullOrEmpty(command.InternalNote))
			{
				if (note != null)
				{
					note.ClearScheduleNote();
					note.AppendScheduleNote(command.InternalNote);
				}
				else
				{
					scheduleDay.CreateAndAddNote(command.InternalNote);
				}
			}
			else if (note != null)
			{
				scheduleDay.DeleteNote();
			}
			((IReadOnlyScheduleDictionary)scheduleDic).MakeEditable();
			var errorResponses = scheduleDic.Modify(scheduleDay, new DoNothingScheduleDayChangeCallBack()).ToList();
			if (errorResponses.Count > 0)
			{
				command.ErrorMessages = errorResponses.Select(x => x.Message).ToList();
				return;
			}

			var range = scheduleDic[person];
			var diff = range.DifferenceSinceSnapshot(_diffreenceService);

			_scheduleDifferenceSaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate)range);
		}
	}
}
