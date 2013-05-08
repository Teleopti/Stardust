using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
    public abstract class RestrictionViewModel : IRestrictionViewModel,INotifyPropertyChanged
    {
        private readonly CommandModel _updateCommandModel;
        private readonly CommandModel _deleteCommandModel;
        private readonly ICommand _updateOnEventCommand;
        internal IRestrictionAltered ParentToCommitChanges;
        public IRestrictionBase Restriction { get; set; }
        
        public abstract string Description { get; }

        public CommandModel UpdateCommandModel
        {
            get { return _updateCommandModel; }
        }
        public CommandModel DeleteCommandModel
        {
            get { return _deleteCommandModel; }
        }
        public ICommand UpdateOnEventCommand
        {
            get { return _updateOnEventCommand; }
        }

        public ILimitationViewModel StartTimeLimits { get; protected set; }

        public ILimitationViewModel EndTimeLimits { get; protected set; }

        public ILimitationViewModel WorkTimeLimits { get; protected set; }

        public IScheduleData PersistableScheduleData { get; set; }

        public IScheduleDay ScheduleDay { get; set; }
        
        public virtual bool BelongsToPart()
        {
            return true;
        }

        protected void UpdateTimeProperties()
        {
            if (Restriction != null)
            {
                Restriction.EndTimeLimitation = (EndTimeLimitation)EndTimeLimits.Limitation;
                Restriction.StartTimeLimitation = (StartTimeLimitation)StartTimeLimits.Limitation;
                Restriction.WorkTimeLimitation = (WorkTimeLimitation)WorkTimeLimits.Limitation;
            }
        }

        //Notifies the parent that changes has been made to the model
        protected void NotifyParentForChanges()
        {
            ParentToCommitChanges.RestrictionAltered();
        }

        //Commits the changes from the gui to the underlying model
        public abstract void CommitChanges();

        protected virtual bool CanExecuteDeleteCommand()
        {
            return true;
        }

        public  bool IsValid()
        {
            return !(StartTimeLimits.Invalid || EndTimeLimits.Invalid || WorkTimeLimits.Invalid);
        }

        private void NotifyRestrictionChanged()
        {
            ParentToCommitChanges.RestrictionAltered();
        }

        protected RestrictionViewModel()
        {
            _updateCommandModel = new UpdateCommand(this);
            _deleteCommandModel = CommandModelFactory.CreateApplicationCommandModel(Remove, CanExecuteDeleteCommand, UserTexts.Resources.Delete,
                                                                                    DefinedRaptorApplicationFunctionPaths
                                                                                        .ModifyPersonRestriction);
            _updateOnEventCommand = new UpdateCommandBehavior(this);
        }

        private void Remove()
        {
            ParentToCommitChanges.RestrictionRemoved(this);
        }

        /// <summary>
        /// This Command updates the underlying object and then notifies its parent that changes are made
        /// </summary>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-01-26
        /// </remarks>
        private class UpdateCommand : CommandModel
        {
            private readonly RestrictionViewModel _model;

            public UpdateCommand(RestrictionViewModel model)
            {
                _model = model;
            }
            public override string Text
            {
                get { return UserTexts.Resources.Update; }
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                _model.CommitChanges();
                _model.NotifyRestrictionChanged();
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = _model.IsValid();
            }
        }

        /// <summary>
        /// Updates the underlying object 
        /// </summary>
        /// <remarks>
        /// For hooking up events (TextBox.TextChanged etc...)
        /// Using EventBehaviorFactory
        /// </remarks>
        private class UpdateCommandBehavior : ICommand
        {
            private readonly RestrictionViewModel _model;
            
            public UpdateCommandBehavior(RestrictionViewModel model)
            {
                _model = model;
                _model.PropertyChanged+=ModelPropertyChanged;
            }

            private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName=="IsValid")
                {
                	var handler = CanExecuteChanged;
                    if (handler!=null)
                    {
                    	handler(this, EventArgs.Empty);
                    }
                }
            }

            public event EventHandler CanExecuteChanged; //Not used
           
            public void Execute(object parameter)
            {
                if (CanExecute(parameter))
                {
                    _model.CommitChanges();
                }
            }

            public bool CanExecute(object parameter)
            {
                return _model.IsValid();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string property)
        {
        	var handler = PropertyChanged;
            if (handler!=null)
            {
            	handler(this,new PropertyChangedEventArgs(property));
            }
        }

        private delegate IRestrictionViewModel CreateModel(IRestrictionAltered altered, IRestrictionBase restriction);

        private static readonly TypeFunctionMapping<CreateModel> FunctionMap = new TypeFunctionMapping<CreateModel>
                                                                                 {
                                                                     {typeof (IStudentAvailabilityRestriction),CreateAvailableRestrictionViewModel},
                                                                     {typeof (IPreferenceRestriction),CreatePreferenceRestrictionViewModel},
                                                                     {typeof (IRotationRestriction),CreateRotationRestrictionViewModel}, 
                                                                     {typeof (IAvailabilityRestriction),CreateAvailabilityRestrictionViewModel}, 
                                                                 };

        private class TypeFunctionMapping<TDelegate> : IEnumerable
        {
            private readonly IDictionary<Type, TDelegate> _innerDictionary = new Dictionary<Type, TDelegate>();

            public void Add(Type key, TDelegate value)
            {
                _innerDictionary.Add(key,value);
            }

            public bool TryGetValue(Type key, out TDelegate value)
            {
                foreach (var keyValuePair in _innerDictionary)
                {
                    if (keyValuePair.Key.IsAssignableFrom(key))
                    {
                        value = keyValuePair.Value;
                        return true;
                    }
                }
                value = default(TDelegate);
                return false;
            }

            public IEnumerator GetEnumerator()
            {
                return _innerDictionary.GetEnumerator();
            }
        }

        private static IRestrictionViewModel CreatePreferenceRestrictionViewModel(IRestrictionAltered altered, IRestrictionBase restriction)
        {
            if (!PrincipalAuthorization.Instance().IsPermitted(
                    DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction))
                return CreateReadOnlyViewModel(restriction);

            return new PreferenceRestrictionViewModel((IPreferenceRestriction) restriction, altered);
        }

        private static RotationRestrictionViewModel CreateRotationRestrictionViewModel(IRestrictionAltered altered, IRestrictionBase restriction)
        {
            return new RotationRestrictionViewModel((IRotationRestriction)restriction);
        }

        private static AvailabilityRestrictionViewModel CreateAvailabilityRestrictionViewModel(IRestrictionAltered altered, IRestrictionBase restriction)
        {
            return new AvailabilityRestrictionViewModel((IAvailabilityRestriction)restriction);
        }

        private static IRestrictionViewModel CreateAvailableRestrictionViewModel(IRestrictionAltered altered, IRestrictionBase restriction)
        {
            if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction))
                return CreateReadOnlyViewModel(restriction);

            return new AvailableRestrictionViewModel((IStudentAvailabilityRestriction)restriction, altered);
        }

        public static IRestrictionViewModel CreateReadOnlyViewModel(IRestrictionBase restriction)
        {
            return new ReadOnlyRestrictionViewModel(restriction);
        }

        private static IRestrictionViewModel CreateViewModel(IRestrictionAltered altered, IRestrictionBase restriction)
        {
            CreateModel createModel;
            IRestrictionViewModel ret = FunctionMap.TryGetValue(restriction.GetType(), out createModel)
                                            ? createModel(altered, restriction)
                                            : null;
            return ret;
        }
        
        public static IRestrictionViewModel CreateViewModel(IRestrictionAltered altered, IScheduleDataRestriction scheduleDataRestriction)
        {
            IRestrictionViewModel ret = CreateViewModel(altered, scheduleDataRestriction.Restriction);
            if (ret != null) ret.PersistableScheduleData = scheduleDataRestriction;
            return ret;
        }
        public static IRestrictionViewModel CreateViewModel(IRestrictionAltered altered, IPreferenceDay scheduleDataRestriction)
        {
            IRestrictionViewModel ret = CreateViewModel(altered, scheduleDataRestriction.Restriction);
            if (ret != null) ret.PersistableScheduleData = scheduleDataRestriction;
            return ret;
        }

        public static IRestrictionViewModel CreateViewModel(IRestrictionAltered altered, IStudentAvailabilityDay scheduleDataRestriction)
        {
            IRestrictionViewModel ret = null;
            if(scheduleDataRestriction.RestrictionCollection.Count > 0)
                ret = CreateViewModel(altered, scheduleDataRestriction.RestrictionCollection[0]);
            if (ret != null) ret.PersistableScheduleData = scheduleDataRestriction;
            return ret;
        }
    }
}