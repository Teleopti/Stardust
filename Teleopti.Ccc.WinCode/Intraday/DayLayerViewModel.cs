using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public class DayLayerViewModel : IDayLayerViewModel
    {
        private IRtaStateHolder _rtaStateHolder;
        private readonly IEventAggregator _eventAggregator;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private IDispatcherWrapper _dispatcherWrapper;
        private static CommonNameDescriptionSetting _commonNameDescriptionSetting;
        private readonly ICollection<DayLayerModel> _models = new ObservableCollection<DayLayerModel>();

        public DayLayerViewModel(IRtaStateHolder rtaStateHolder, IEventAggregator eventAggregator, IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IDispatcherWrapper dispatcherWrapper)
        {
            _eventAggregator = eventAggregator;
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
            _dispatcherWrapper = dispatcherWrapper;
            _rtaStateHolder = rtaStateHolder;
        }

        public ICollection<DayLayerModel> Models
        {
            get { return _models; }
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

				var layerViewModelCollection = new LayerViewModelCollection(_eventAggregator, new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), new ReplaceLayerInSchedule());
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
            {
                rebuildLayerViewModelCollection(model);
            }
        }

        private CommonNameDescriptionSetting getCommonNameDescriptionSetting()
        {
            if (_commonNameDescriptionSetting == null)
            {
                using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    _commonNameDescriptionSetting = _repositoryFactory.CreateGlobalSettingDataRepository(uow).FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting());
                }
            }
            return _commonNameDescriptionSetting;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int HookedEvents()
        {
            var handler = PropertyChanged;
            if (handler == null)
            {
                return 0;
            }
            int i = handler.GetInvocationList().Count();
            return i;
        }

        public void Refresh(DateTime timestamp)
        {
            foreach (var dayLayerModel in Models)
            {
	            IActualAgentState agentState;
	            if (dayLayerModel.Person.Id == null ||
	                !_rtaStateHolder.ActualAgentStates.TryGetValue((Guid) dayLayerModel.Person.Id, out agentState))
		            continue;
	            dayLayerModel.CurrentActivityDescription = agentState.Scheduled;
	            dayLayerModel.EnteredCurrentState = agentState.StateStart;
	            dayLayerModel.NextActivityDescription = agentState.ScheduledNext;
	            dayLayerModel.NextActivityStartDateTime = agentState.NextStart;
	            dayLayerModel.CurrentStateDescription = agentState.State;
	            dayLayerModel.AlarmStart = agentState.AlarmStart;

	            if (DateTime.UtcNow <= dayLayerModel.AlarmStart) continue;
				dayLayerModel.StaffingEffect = agentState.StaffingEffect;
				dayLayerModel.ColorValue = agentState.Color;
	            dayLayerModel.AlarmDescription = agentState.AlarmName;
            }
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
            {
                return;
            }
            _dispatcherWrapper.BeginInvoke((Action)(() => rebuildSchedule(model)));
        }

        private void rebuildSchedule(DayLayerModel model)
        {
            if (_rtaStateHolder == null ||
                model.Layers == null)
            {
                return;
            }
            IScheduleRange scheduleRange =
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
            }
            _rtaStateHolder = null;
            _dispatcherWrapper = null;
        }
    }
}