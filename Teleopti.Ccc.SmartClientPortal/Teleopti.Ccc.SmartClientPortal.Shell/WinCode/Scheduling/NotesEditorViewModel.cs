using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public class NotesEditorViewModel : INotesEditorViewModel, INotesAltered, IPublicNotesAltered, INotifyPropertyChanged
    {
        private IScheduleDay _schedulePart;
        private string _scheduleNote;
        private string _originalNote;
        private bool _isAltered;

        private string _publicScheduleNote;
        private string _publicOriginalNote;
        private bool _publicIsAltered;

        public event EventHandler NotesChanged;
        public event EventHandler PublicNotesChanged;
        private readonly ICommand _changedCommandBehavior;
        private readonly CommandModel _deleteCommandModel;
        private readonly CommandModel _deletePublicNoteCommandModel;
        private readonly ICommand _changedPublicNoteCommandBehavior;

        public event PropertyChangedEventHandler PropertyChanged;
        private bool _isEnabled;

        public NotesEditorViewModel(IScheduleDay part)
        {
            _changedCommandBehavior = new ChangedCommandBehavior(this);
            _changedPublicNoteCommandBehavior = new ChangedPublicNoteCommandBehavior(this);
            _deleteCommandModel = new DeleteCommand(this);
            _deletePublicNoteCommandModel = new DeletePublicNoteCommand(this);
            Load(part);
        }

        private void OnPropertyChanged(string schedulerNote)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(schedulerNote));
        }

        public IScheduleDay SchedulePart
        {
            get { return _schedulePart; }
            private set { _schedulePart = value; }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public void Load(IScheduleDay schedulePart)
        {
            if (schedulePart != null)
            {
                if (NotesIsAltered)
                {
                    UpdateNoteInSchedulePart();
                    NotesAltered();
                }
                if (PublicNotesIsAltered)
                {
                    UpdatePublicNoteInSchedulePart();
                    PublicNotesAltered();
                }

                SchedulePart = schedulePart;
                ReadDataFromSchedulepart();
            }
            else
            {
                IsEnabled = false;
                NotesIsAltered = false;
                PublicNotesIsAltered = false;
            	_schedulePart = null;
            }
        }

        private void UpdateNoteInSchedulePart()
        {
            if(_schedulePart != null)
            {
                INote note = _schedulePart.NoteCollection().FirstOrDefault();

                if (!string.IsNullOrEmpty(ScheduleNote))
                {
                    if (note != null)
                    {
                        note.ClearScheduleNote();
                        note.AppendScheduleNote(ScheduleNote);
                    }
                    else
                    {
                        _schedulePart.CreateAndAddNote(ScheduleNote);
                    }
                }
                else
                {
                    if (note != null)
                        _schedulePart.DeleteNote();
                }
            }
        }

        /// <summary>
        /// Updates the public note in schedule part.
        /// </summary>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-12-02
        /// </remarks>
        private void UpdatePublicNoteInSchedulePart()
        {
            if (_schedulePart != null)
            {
                IPublicNote note = _schedulePart.PublicNoteCollection().FirstOrDefault();

                if (!string.IsNullOrEmpty(PublicScheduleNote))
                {
                    if (note != null)
                    {
                        note.ClearScheduleNote();
                        note.AppendScheduleNote(PublicScheduleNote);
                    }
                    else
                    {
                        _schedulePart.CreateAndAddPublicNote(PublicScheduleNote);
                    }
                }
                else
                {
                    if (note != null)
                        _schedulePart.DeletePublicNote();
                }
            }
        }

        public void NotesAltered()
        {
            if (_originalNote != ScheduleNote)
            {
                if (NotesChanged != null && NotesIsAltered) NotesChanged(this, EventArgs.Empty);
                NotesIsAltered = false;
                _originalNote = ScheduleNote;
            }
        }


        public bool NotesIsAltered
        {
            get { return _isAltered; }
            set {_isAltered = value; }     
        }

        public void NoteRemoved()
        {
            NotesIsAltered = true;
            ScheduleNote = string.Empty;
            OnPropertyChanged(nameof(ScheduleNote));  
        }

        public void PublicNotesAltered()
        {
            if (_publicOriginalNote != _publicScheduleNote)
            {
                if (PublicNotesChanged != null && PublicNotesIsAltered) PublicNotesChanged(this, EventArgs.Empty);
                PublicNotesIsAltered = false;
                _publicOriginalNote = _publicScheduleNote;
            }
        }

        public bool PublicNotesIsAltered
        {
            get { return _publicIsAltered; }
            set { _publicIsAltered = value; }
        }

        public void PublicNoteRemoved()
        {
            PublicNotesIsAltered = true;
            PublicScheduleNote = string.Empty;
            OnPropertyChanged(nameof(PublicScheduleNote));
        }

        public string ScheduleNote
        {
            get { return _scheduleNote; }
            set 
            { 
                _scheduleNote = value;
                UpdateNoteInSchedulePart();
                NotesAltered();
            }
        }

        public string PublicScheduleNote
        {
            get { return _publicScheduleNote; }
            set 
            {
                _publicScheduleNote = value;
                UpdatePublicNoteInSchedulePart();
                PublicNotesAltered();
            }
        }

        internal void ReadDataFromSchedulepart()
        {
            if (_schedulePart == null) return;

            IsEnabled =
                PrincipalAuthorization.Current_DONTUSE()
                                      .IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment,
                                                   _schedulePart.DateOnlyAsPeriod.DateOnly, _schedulePart.Person) &&
                _schedulePart.FullAccess;

            INote note = _schedulePart.NoteCollection().FirstOrDefault();
            IPublicNote publicNote = _schedulePart.PublicNoteCollection().FirstOrDefault();

            _scheduleNote = note != null ? note.GetScheduleNote(new NoFormatting()) : string.Empty;
            _publicScheduleNote = publicNote != null ? publicNote.GetScheduleNote(new NoFormatting()) : string.Empty;

            _originalNote = ScheduleNote;
            _publicOriginalNote = PublicScheduleNote;
            OnPropertyChanged(nameof(ScheduleNote));
            OnPropertyChanged(nameof(PublicScheduleNote));
        }

        public ICommand ChangedPublicNoteCommand
        {
            get { return _changedPublicNoteCommandBehavior; }
        }
        public ICommand ChangedCommand
        {
            get { return _changedCommandBehavior; }
        }

        public CommandModel DeleteCommandModel
        {
            get { return _deleteCommandModel; }
        }

        public CommandModel DeletePublicNoteCommandModel
        {
            get { return _deletePublicNoteCommandModel; }
        }


        protected virtual bool CanExecuteDeleteCommand()
        {
            return true;
        }


        private class ChangedCommandBehavior : ICommand
        {
            private readonly NotesEditorViewModel _model;

            public ChangedCommandBehavior(NotesEditorViewModel model)
            {
                _model = model;
                if (CanExecuteChanged != null) CanExecuteChanged(this, EventArgs.Empty); //FxCop
            }

            public event EventHandler CanExecuteChanged; //Not used

            public void Execute(object parameter)
            {
                if (CanExecute(parameter))
                {
                    _model.NotesIsAltered = true;
                }
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }
        }

        private class DeleteCommand : CommandModel
        {
            private readonly NotesEditorViewModel _model;

            public DeleteCommand(NotesEditorViewModel model)
            {
                _model = model;
            }

            public override string Text
            {
                get { return UserTexts.Resources.Delete; }
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                _model.NoteRemoved();
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = _model.CanExecuteDeleteCommand();
            }
        }

        private class ChangedPublicNoteCommandBehavior : ICommand
        {
            private readonly NotesEditorViewModel _model;

            public ChangedPublicNoteCommandBehavior(NotesEditorViewModel model)
            {
                _model = model;
                if (CanExecuteChanged != null) CanExecuteChanged(this, EventArgs.Empty); //FxCop
            }

            public event EventHandler CanExecuteChanged; //Not used

            public void Execute(object parameter)
            {
                if (CanExecute(parameter))
                {
                    _model.PublicNotesIsAltered = true;
                }
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }
        }

        private class DeletePublicNoteCommand : CommandModel
        {
            private readonly NotesEditorViewModel _model;

            public DeletePublicNoteCommand(NotesEditorViewModel model)
            {
                _model = model;
            }

            public override string Text
            {
                get { return UserTexts.Resources.Delete; }
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                _model.PublicNoteRemoved();
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = _model.CanExecuteDeleteCommand();
            }
        }

    }
}
