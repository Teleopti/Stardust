using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class PersistConflictPresenter
    {
        private readonly IPersistConflictView _view;
        private readonly PersistConflictModel _model;
        private readonly IList<string> _headers;

        public PersistConflictPresenter(IPersistConflictView view,
                                        PersistConflictModel model)
        {
            _view = view;
            _model=model;
            _headers = new List<string> { UserTexts.Resources.Name, UserTexts.Resources.Date, UserTexts.Resources.Type, UserTexts.Resources.OtherUser };
        }

        public void Initialize()
        {
            bindToModel();
            _view.SetupGridControl(_model.Data);            
        }

        public void OnUndoClientChanges()
        {
            mergeConflict(true);
            closeForm(true);
        }

        public void OnOverwriteServerChanges()
        {
            mergeConflict(false);
            closeForm(true);
        }

        public void OnCancel()
        {
            closeForm(false);
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

        private void mergeConflict(bool undoClientState)
        {
            foreach (var messState in _model.PersistConflicts)
            {
                var dbVersion = messState.DatabaseVersion;
                addToModifiedCollection(messState, undoClientState);
                if(dbVersion==null)
                {
                    var orgVersion = messState.ClientVersion.OriginalItem;
                    ((ScheduleRange)_model.ScheduleDictionary[orgVersion.Person])
                        .UnsafeSnapshotDelete(orgVersion.Id.Value, undoClientState); 
                }
                else
                {
                    ((ScheduleRange)_model.ScheduleDictionary[dbVersion.Person])
                        .UnsafeSnapshotUpdate(dbVersion, undoClientState);  
                }                
                messState.RemoveFromCollection();
            }
        }

        private void addToModifiedCollection(IPersistConflict messState, bool undoClientState)
        {
            if(undoClientState)
            {
                _model.ModifiedData.Add(messState.ClientVersion.OriginalItem);
                if(messState.DatabaseVersion!=null)
                    _model.ModifiedData.Add(messState.DatabaseVersion);
            }
        }

        private void closeForm(bool allConflictsSolved)
        {
            _view.CloseForm(allConflictsSolved);
        }

        private void bindToModel()
        {
            _model.PersistConflicts.ForEach(messageState => _model.Data.Add(createConflictData(messageState)));
        }

        private static PersistConflictData createConflictData(IPersistConflict messageState)
        {
            var orgClientItem = messageState.ClientVersion.OriginalItem;
            var ret = new PersistConflictData
                        {
                            Name = formatName(orgClientItem.Person.Name),
                            Date = new DateOnly(orgClientItem.Period.StartDateTime),
                            ConflictType = typeString(orgClientItem),
                            LastModifiedName = string.Empty
                        };
            if(messageState.DatabaseVersion == null)
            {
                ret.LastModifiedName = UserTexts.Resources.Deleted;                 
            }
            else
            {
                var change = messageState.DatabaseVersion as IChangeInfo;
                if(change!=null)
                {
                    ret.LastModifiedName = formatName(change.UpdatedBy.Name);
                }
            }
            return ret;
        }

        private static string typeString(IScheduleParameters conflict)
        {
            if (conflict is IPersonAssignment)
                return UserTexts.Resources.Shift;
            if (conflict is IPersonAbsence)
                return UserTexts.Resources.Absence; 
            if (conflict is IPersonDayOff)
                return UserTexts.Resources.DayOff;
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
