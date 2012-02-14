using System;
using System.Windows.Input;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Ccc.WinCode.Common.Models;
using Teleopti.Ccc.WinCode.Common.Time.Timeline;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Collection;


namespace Teleopti.Ccc.WpfControls.UndoRedo.ViewModels
{
    public class UndoRedoViewModel : DataModel
    {
        //This will set the minimum of the timeLine
        private TimeSpan _minimumTimeSpan = TimeSpan.FromMinutes(5);
        
        private CommandModel _redoCommandModel;
        private CommandModel _undoCommandModel;
        private CommandModel _undoAllCommandModel;
        private TimelineControlViewModel _timelineControlViewModel;
        private DateTimePeriodViewModel _dateTimePeriodViewModel;
        public MementoViewModelCollection UndoMementos { get; private set; }
        public MementoViewModelCollection RedoMementos { get; private set; }
        public IUndoRedoContainer Container
        {
            get;
            private set;
        }

        public CommandModel RedoCommandModel
        {
            get { return _redoCommandModel; }
        }
        public CommandModel UndoCommandModel
        {
            get { return _undoCommandModel; }
        }
        public CommandModel UndoAllCommandModel
        {
            get { return _undoAllCommandModel; }
        }

        public DateTimePeriodViewModel DateTimePeriodViewModel
        {
            get { return _dateTimePeriodViewModel; }
            set
            {
                _dateTimePeriodViewModel = value;
            }
        }

        public TimelineControlViewModel TimelineControlViewModel
        {
            get { return _timelineControlViewModel; }
            private set
            {
                _timelineControlViewModel = value;
                SendPropertyChanged("TimelineControlViewModel");
            }
        }

        internal void Undo()
        {
            if (UndoMementos.Count > 0) UndoMementos[0].IsUndoing = true;
            Container.Undo();
        }


        internal void UndoAll()
        {
            UndoMementos.ForEach(m => m.IsUndoing = true);
            Container.UndoAll();
        }

        internal void Redo()
        {
            if (RedoMementos.Count > 0) RedoMementos[0].IsUndoing = true;
            Container.Redo();
        }

        public UndoRedoViewModel()
        {

        }

        public UndoRedoViewModel(IUndoRedoContainer container,IEventAggregator eventAggregator,ICreateLayerViewModelService createLayerViewModelService)
        {
          
            DateTimePeriodViewModel = new DateTimePeriodViewModel();
            DateTimePeriodViewModel.Interval = _minimumTimeSpan;
            DateTimePeriodViewModel.Start = DateTime.UtcNow;
            TimelineControlViewModel = new TimelineControlViewModel(eventAggregator,createLayerViewModelService) { Period = DateTimePeriodViewModel.DateTimePeriod };
            UndoMementos = new MementoViewModelCollection();
            RedoMementos = new MementoViewModelCollection();
            Container = container;

            UndoRedoContainer c = container as UndoRedoContainer;
            if (c!=null) c.ChangedHandler+=new EventHandler(containerChanged);
           
            _undoCommandModel = new UndoCommandModel(this);
            _redoCommandModel = new RedoCommandModel(this);
            _undoAllCommandModel = new UndoAllCommandModel(this);
            CreateViewModels();
            
        }

        private void containerChanged(object sender, EventArgs e)
        {
            CreateViewModels();
        }

        private void CreateViewModels()
        {
            UndoMementos.Clear();
            RedoMementos.Clear();
            Container.RedoCollection().ForEach(RedoMementos.AddMemento);
            Container.UndoCollection().ForEach(UndoMementos.AddMemento);
            if (UndoMementos.Earliest < RedoMementos.Earliest)
                DateTimePeriodViewModel.End = UndoMementos.Earliest;
            else DateTimePeriodViewModel.End = RedoMementos.Earliest;
            TimelineControlViewModel.Period = DateTimePeriodViewModel.DateTimePeriod;
        }

    }

    public class UndoCommandModel : CommandModel
    {
        private UndoRedoViewModel _target;

        public UndoCommandModel(UndoRedoViewModel target)
        {
            _target = target;
        }

        public override string Text
        {
            get { return "xxUndoCommandModel"; }
           
        }

        public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _target.Container.CanUndo();
        }

        public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
        {
            _target.Undo();
        }
    }

    public class UndoAllCommandModel : CommandModel
    {
        private UndoRedoViewModel _target;

        public UndoAllCommandModel(UndoRedoViewModel target)
        {
            _target = target;
        }

        public override string Text
        {
            get { return "xxUndoAllCommandModel"; }
          
        }

        public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _target.Container.CanUndo();
        }

        public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
        {
            _target.UndoAll();
        }
    }

    public class RedoCommandModel : CommandModel
    {
        private UndoRedoViewModel _target;

        public RedoCommandModel(UndoRedoViewModel target)
        {
            _target = target;
        }

        public override string Text
        {
            get
            {
                return "xxRedoCommandModel";
            }
            
        }

        public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _target.Container.CanRedo();
            e.CanExecute = _target.Container.CanRedo();
        }

        public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
        {
            _target.Redo();
        }
    }

}