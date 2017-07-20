using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class EditScheduleNoteCommandHandler : IHandleCommand<EditScheduleNoteCommand>
	{
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IScheduleDayProvider _scheduleDayProvider;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _diffreenceService;

		public EditScheduleNoteCommandHandler(IProxyForId<IPerson> personForId, IScheduleDayProvider scheduleDayProvider,
			IScheduleDifferenceSaver scheduleDifferenceSaver,
			IDifferenceCollectionService<IPersistableScheduleData> diffreenceService)
		{
			_personForId = personForId;
			_scheduleDayProvider = scheduleDayProvider;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_diffreenceService = diffreenceService;
		}

		public void Handle(EditScheduleNoteCommand command)
		{
			var person = _personForId.Load(command.PersonId);
			var scheduleDay = _scheduleDayProvider.GetScheduleDay(command.Date, person,
				new ScheduleDictionaryLoadOptions(true, true));
			var scheduleDic = _scheduleDayProvider.GetScheduleDictionary(command.Date, person,
				new ScheduleDictionaryLoadOptions(true, true));

			updateInternalNote(command.InternalNote, scheduleDay);
			updatePublicNote(command.PublicNote, scheduleDay);
			((IReadOnlyScheduleDictionary) scheduleDic).MakeEditable();
			var errorResponses = scheduleDic.Modify(scheduleDay, new DoNothingScheduleDayChangeCallBack()).ToList();
			if (errorResponses.Count > 0)
			{
				command.ErrorMessages = errorResponses.Select(x => x.Message).ToList();
				return;
			}

			var range = scheduleDic[person];
			var diff = range.DifferenceSinceSnapshot(_diffreenceService);

			_scheduleDifferenceSaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate) range);
		}

		private static void updateInternalNote(string internalNote, IScheduleDay scheduleDay)
		{
			var note = scheduleDay.NoteCollection().FirstOrDefault();
			if (!string.IsNullOrEmpty(internalNote))
			{
				if (note != null)
				{
					note.ClearScheduleNote();
					note.AppendScheduleNote(internalNote);
				}
				else
				{
					scheduleDay.CreateAndAddNote(internalNote);
				}
			}
			else if (note != null)
			{
				scheduleDay.DeleteNote();
			}
		}

		private static void updatePublicNote(string publicNote, IScheduleDay scheduleDay)
		{
			if(publicNote == null) return;

			var note = scheduleDay.PublicNoteCollection().FirstOrDefault();
			if (!string.IsNullOrEmpty(publicNote))
			{
				if (note != null)
				{
					note.ClearScheduleNote();
					note.AppendScheduleNote(publicNote);
				}
				else
				{
					scheduleDay.CreateAndAddPublicNote(publicNote);
				}

			}
			else if (note != null)
			{
				scheduleDay.DeletePublicNote();
			}
		}
	}
}
