using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Persisters;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public class PersistConflictPresenter
    {
        private readonly IPersistConflictView _view;
        private readonly PersistConflictModel _model;
	    private readonly IMessageQueueRemoval _messageQueueRemoval;
	    private readonly IList<string> _headers;

        public PersistConflictPresenter(IPersistConflictView view,
                                        PersistConflictModel model,
																				IMessageQueueRemoval messageQueueRemoval)
        {
            _view = view;
            _model=model;
	        _messageQueueRemoval = messageQueueRemoval;
	        _headers = new List<string> { UserTexts.Resources.Name, UserTexts.Resources.Date, UserTexts.Resources.Type, UserTexts.Resources.OtherUser };
        }

        public void Initialize()
        {
            bindToModel();
            _view.SetupGridControl(_model.Data);            
        }

        public void OnDiscardMyChanges()
        {
            mergeConflict(true);
			closeForm();
        }

        public void OnOverwriteServerChanges()
        {
            mergeConflict(false);
			closeForm();
        }

        public void OnCancel()
        {
            closeForm();
        }

        public string OnQueryCellInfo(int rowIndex, int colIndex)
        {
            if (rowIndex == 0 && colIndex > 0 )
                return _headers[colIndex-1];
            if (rowIndex > 0 && colIndex > 0)
            {
                if (colIndex == 1)
                    return _model.Data[rowIndex - 1].Name;
                if (colIndex == 2)
                    return _model.Data[rowIndex - 1].Date.ToShortDateString();
                if (colIndex == 3)
                    return _model.Data[rowIndex - 1].ConflictType;
                if (colIndex == 4)
                    return _model.Data[rowIndex - 1].LastModifiedName;
            }
            return string.Empty;
        }

        private void mergeConflict(bool discardMyChanges)
        {
            foreach (var messState in _model.PersistConflicts)
            {
				if (discardMyChanges)
					addToModifiedCollection(messState);

				var databaseVersion = messState.DatabaseVersion;
				var originalVersion = messState.ClientVersion.OriginalItem;

				if (databaseVersion == null)
                {
					var scheduleRange = ((ScheduleRange)_model.ScheduleDictionary[originalVersion.Person]);
					scheduleRange.SolveConflictBecauseOfExternalDeletion(originalVersion.Id.GetValueOrDefault(), discardMyChanges); 
                }
                else
                {
					var scheduleRange = ((ScheduleRange)_model.ScheduleDictionary[databaseVersion.Person]);
					// I inserted a person assignment, and so did someone else
					if (databaseVersion is IPersonAssignment && messState.ClientVersion.OriginalItem == null)
					{
						scheduleRange.SolveConflictBecauseOfExternalInsert(databaseVersion, discardMyChanges);
					}
					else
					{
						scheduleRange.SolveConflictBecauseOfExternalUpdate(databaseVersion, discardMyChanges);
					}
                }

							_messageQueueRemoval.Remove(messState);
            }
        }

        private void addToModifiedCollection(PersistConflict messState)
        {
			if (messState.ClientVersion.OriginalItem != null)
				_model.ModifiedDataResult.Add(messState.ClientVersion.OriginalItem);
			if (messState.DatabaseVersion != null)
				_model.ModifiedDataResult.Add(messState.DatabaseVersion);
        }

        private void closeForm()
        {
            _view.CloseForm();
        }

        private void bindToModel()
        {
            _model.PersistConflicts.ForEach(messageState => _model.Data.Add(createConflictData(messageState)));
        }

        private static PersistConflictData createConflictData(PersistConflict messageState)
        {
            var clientVersion = messageState.ClientVersion.OriginalItem;
	        if (clientVersion == null)
				clientVersion = messageState.ClientVersion.CurrentItem;
            var conflictData = new PersistConflictData
                        {
                            Name = formatName(clientVersion.Person.Name),
                            Date = new DateOnly(clientVersion.Period.StartDateTime),
                            ConflictType = typeString(clientVersion),
                            LastModifiedName = string.Empty
                        };
            if(messageState.DatabaseVersion == null)
            {
                conflictData.LastModifiedName = UserTexts.Resources.Deleted;                 
            }
            else
            {
                var change = messageState.DatabaseVersion as IChangeInfo;
                if(change!=null)
                {
                    conflictData.LastModifiedName = formatName(change.UpdatedBy.Name);
                }
            }
            return conflictData;
        }

        private static string typeString(IScheduleParameters conflict)
        {
            if (conflict is IPersonAssignment)
                return UserTexts.Resources.Shift;
            if (conflict is IPersonAbsence)
                return UserTexts.Resources.Absence; 
            if (conflict is INote)
                return UserTexts.Resources.Note;
            if (conflict is IStudentAvailabilityDay)
                return UserTexts.Resources.StudentAvailability;
	        if (conflict is IPublicNote)
		        return UserTexts.Resources.PublicNoteColon;
	        if (conflict is IAgentDayScheduleTag)
		        return UserTexts.Resources.Tags;
	        if (conflict is IPreferenceDay)
		        return UserTexts.Resources.Preference;
			return UserTexts.Resources.Unknown + "(" + conflict.GetType() + ")";
        }

        private static string formatName(Name name)
        {
            return name.ToString(NameOrderOption.FirstNameLastName);
        }
    }
}
