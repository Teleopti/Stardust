using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday
{
    public class DayLayerViewModel : IDayLayerViewModel
    {
        private IRtaStateHolder _rtaStateHolder;
        private readonly IEventAggregator _eventAggregator;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private IDispatcherWrapper _dispatcherWrapper;
        private static CommonNameDescriptionSetting _commonNameDescriptionSetting;
	    private readonly DayLayerModelComparer _customComparer;
	    private CollectionViewSource _collectionViewSource;
		public ListCollectionView ModelEditable { get; private set; }

		public ICollection<DayLayerModel> Models { get; private set; }

	    public DayLayerViewModel(IRtaStateHolder rtaStateHolder, IEventAggregator eventAggregator, IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IDispatcherWrapper dispatcherWrapper)
        {
            _eventAggregator = eventAggregator;
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
            _dispatcherWrapper = dispatcherWrapper;
            _rtaStateHolder = rtaStateHolder;
			if (rtaStateHolder != null)
				_rtaStateHolder.AgentstateUpdated += rtaStateHolderOnAgentstateUpdated;
			Models = new ObservableCollection<DayLayerModel>();
		    _collectionViewSource = new CollectionViewSource {Source = Models};
		    ModelEditable = _collectionViewSource.View as ListCollectionView;

		    
			if (ModelEditable == null) return;
		    _customComparer = new DayLayerModelComparer();
		    ModelEditable.CustomSort = _customComparer;
        }
		
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public void CreateModels(IEnumerable<IPerson> people, IDateOnlyPeriodAsDateTimePeriod period)
        {
            Models.Clear();

            var commonNameDescription = getCommonNameDescriptionSetting();
            foreach (var person in people)
            {
                ITeam team = null;
                IPersonPeriod currentPersonPeriod = person.PersonPeriods(period.DateOnlyPeriod).FirstOrDefault();
                if (currentPersonPeriod != null)
                    team = currentPersonPeriod.Team;

				var layerViewModelCollection = new LayerViewModelCollection(_eventAggregator, new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), new ReplaceLayerInSchedule(), PrincipalAuthorization.Current());
                var model = new DayLayerModel(person, period.Period(), team, layerViewModelCollection, commonNameDescription);

                rebuildLayerViewModelCollection(model);

                Models.Add(model);
            }

            _rtaStateHolder.SchedulingResultStateHolder.Schedules.PartModified += OnScheduleModified;
        }

	    public void OnScheduleModified(object sender, ModifyEventArgs e)
        {
            var model = Models.FirstOrDefault(m => m.Person.Equals(e.ModifiedPerson));
	        if (model != null)
		        rebuildLayerViewModelCollection(model);
        }

	    private CommonNameDescriptionSetting getCommonNameDescriptionSetting()
	    {
		    if (_commonNameDescriptionSetting == null)
			    using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
				    _commonNameDescriptionSetting =
					    _repositoryFactory.CreateGlobalSettingDataRepository(uow)
					                      .FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting());

		    return _commonNameDescriptionSetting;
	    }

	    public event PropertyChangedEventHandler PropertyChanged;

        public int HookedEvents()
        {
            var handler = PropertyChanged;
	        if (handler == null)
		        return 0;
	        
			var i = handler.GetInvocationList().Count();
            return i;
        }

		public void InitializeRows()
		{
			foreach (var dayLayerModel in Models)
			{
				var agentState = getActualAgentState(dayLayerModel);
				if (agentState == null) continue;
				updateAgentState(dayLayerModel, agentState);
			}
		}

	    public void RefreshElapsedTime(DateTime timestamp)
	    {
		    foreach (var dayLayerModel in Models)
		    {
			    var agentState = getActualAgentState(dayLayerModel);
			    if (agentState == null) continue;
				ModelEditable.EditItem(dayLayerModel);
				dayLayerModel.EnteredCurrentState = agentState.StateStartTime ?? DateTime.MinValue;
				ModelEditable.CommitEdit();
			    if (DateTime.UtcNow <= dayLayerModel.RuleStartTime) continue;
			    
				ModelEditable.EditItem(dayLayerModel);
			    dayLayerModel.StaffingEffect = agentState.StaffingEffect ?? 0;
				dayLayerModel.ColorValue = agentState.RuleColor ?? 0;
			    dayLayerModel.AlarmDescription = agentState.RuleName;
			    ModelEditable.CommitEdit();
		    }
	    }

	    private void rtaStateHolderOnAgentstateUpdated(object sender, CustomEventArgs<AgentStateReadModel> customEventArgs)
		{
			var agentState = customEventArgs.Value;
			var dayLayerModel = Models.FirstOrDefault(m => m.Person.Id == agentState.PersonId);
			if (dayLayerModel == null) return;
			ModelEditable.Dispatcher.BeginInvoke(new Action(() => updateAgentState(dayLayerModel, agentState)));
		}

		private void updateAgentState(DayLayerModel dayLayerModel, AgentStateReadModel agentStateReadModel)
		{

			ModelEditable.EditItem(dayLayerModel);
			dayLayerModel.CurrentActivityDescription = agentStateReadModel.Activity;
			dayLayerModel.EnteredCurrentState = agentStateReadModel.StateStartTime ?? DateTime.MinValue;
			dayLayerModel.NextActivityDescription = agentStateReadModel.NextActivity;
			dayLayerModel.NextActivityStartDateTime = agentStateReadModel.NextActivityStartTime ?? DateTime.MinValue;
			dayLayerModel.CurrentStateDescription = agentStateReadModel.StateName;
			dayLayerModel.RuleStartTime = agentStateReadModel.RuleStartTime ?? DateTime.MinValue;
			dayLayerModel.HasAlarm = agentStateReadModel.RuleName != null;
			dayLayerModel.StaffingEffect = agentStateReadModel.StaffingEffect ?? 0;
			dayLayerModel.ColorValue = agentStateReadModel.RuleColor ?? 0;
			dayLayerModel.AlarmDescription = agentStateReadModel.RuleName;
			
			ModelEditable.CommitEdit();
		}

		private AgentStateReadModel getActualAgentState(DayLayerModel dayLayerModel)
		{
			AgentStateReadModel agentStateReadModel;
			if (dayLayerModel.Person.Id == null ||
				!_rtaStateHolder.ActualAgentStates.TryGetValue((Guid)dayLayerModel.Person.Id, out agentStateReadModel))
				return null;
			return agentStateReadModel;
		}

        public void RefreshProjection(IPerson person)
        {
            var model = Models.FirstOrDefault(m => m.Person.Equals(person));
            if (model == null) return;

            rebuildLayerViewModelCollection(model);
        }

        private void rebuildLayerViewModelCollection(DayLayerModel model)
        {
	        if (_dispatcherWrapper == null ||
	            _rtaStateHolder == null ||
	            model.Layers == null)
		        return;
	        _dispatcherWrapper.BeginInvoke((Action)(() => rebuildSchedule(model)));
        }

        private void rebuildSchedule(DayLayerModel model)
        {
	        if (_rtaStateHolder == null ||
	            model.Layers == null)
		        return;
	        
			var scheduleRange =
                _rtaStateHolder.SchedulingResultStateHolder.
                    Schedules[model.Person];
            model.Layers.AddFromProjection(scheduleRange, model.Period);
            model.ScheduleStartDateTime = model.Layers.Count > 0
                                              ? model.Layers.Min(
                                                  l => l.Period.StartDateTime)
                                              : DateTime.MaxValue;
        }

        public void UnregisterMessageBrokerEvent()
        {
            if (_rtaStateHolder != null)
            {
                _rtaStateHolder.SchedulingResultStateHolder.Schedules.PartModified -= OnScheduleModified;
	            _rtaStateHolder.AgentstateUpdated -= rtaStateHolderOnAgentstateUpdated;
            }
            _rtaStateHolder = null;
            _dispatcherWrapper = null;
        }
    }
}