using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;
using System.Collections.Specialized;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public class ManageAlarmSituationPresenter : IDisposable
    {
        private IList<IAlarmType> _alarmTypes;
        private IList<IActivity> _activities;
        private IList<IRtaStateGroup> _rtaStateGroups;
        private IList<IStateGroupActivityAlarm> _stateGroupActivityAlarms;
        private readonly IList<IStateGroupActivityAlarm> _stateGroupActivityAlarmsToAdd = new List<IStateGroupActivityAlarm>();
        private readonly IList<IStateGroupActivityAlarm> _stateGroupActivityAlarmsToRemove = new List<IStateGroupActivityAlarm>();
        private readonly IStateGroupActivityAlarmRepository _stateGroupActivityAlarmRepository;
        private readonly IMessageBroker _messageBroker;
        private readonly IManageAlarmSituationView _manageAlarmSituationView;
        private readonly IRtaStateGroupRepository _rtaStateGroupRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IAlarmTypeRepository _alarmTypeRepository;

        public ManageAlarmSituationPresenter(IAlarmTypeRepository alarmTypeRepository, IRtaStateGroupRepository rtaStateGroupRepository, IActivityRepository activityRepository, IStateGroupActivityAlarmRepository stateGroupActivityAlarmRepository, IMessageBroker messageBroker, IManageAlarmSituationView manageAlarmSituationView)
        {
            _alarmTypeRepository = alarmTypeRepository;
            _rtaStateGroupRepository = rtaStateGroupRepository;
            _activityRepository = activityRepository;
            _stateGroupActivityAlarmRepository = stateGroupActivityAlarmRepository;
            _messageBroker = messageBroker;
            _manageAlarmSituationView = manageAlarmSituationView;
        }

        public void Load()
        {
            _alarmTypes = _alarmTypeRepository.LoadAll();
            _rtaStateGroups = _rtaStateGroupRepository.LoadAllCompleteGraph();
            _rtaStateGroups.Add(null);
            _activities = _activityRepository.LoadAll();
            _activities.Add(null);
            _stateGroupActivityAlarms = _stateGroupActivityAlarmRepository.LoadAllCompleteGraph();
            _messageBroker.RegisterEventSubscription(OnRtaStateGroupEvent, typeof(IRtaStateGroup));
            _messageBroker.RegisterEventSubscription(OnActivityEvent, typeof(IActivity));
            _messageBroker.RegisterEventSubscription(OnAlarmEvent, typeof(IAlarmType));
        }

        public void OnAlarmEvent(object sender, EventMessageArgs e)
        {
            if (e.Message.DomainUpdateType == DomainUpdateType.Update ||
                e.Message.DomainUpdateType == DomainUpdateType.Delete)
            {
                IAlarmType alarmType = _alarmTypes.FirstOrDefault(s => s!=null && s.Id == e.Message.DomainObjectId);
                if (alarmType!= null)
                {
                    _alarmTypes.Remove(alarmType);
                }
            }

            if (e.Message.DomainUpdateType == DomainUpdateType.Update ||
                e.Message.DomainUpdateType == DomainUpdateType.Insert)
            {
                IAlarmType alarmType = _alarmTypeRepository.Get(e.Message.DomainObjectId);
                if (alarmType != null) _alarmTypes.Add(alarmType);
            }
            _manageAlarmSituationView.RefreshGrid();
        }

        public void OnActivityEvent(object sender, EventMessageArgs e)
        {
            if (e.Message.DomainUpdateType == DomainUpdateType.Update ||
                e.Message.DomainUpdateType == DomainUpdateType.Delete)
            {
                IActivity activity = _activities.FirstOrDefault(s => s!=null && s.Id == e.Message.DomainObjectId);
                if (activity != null)
                {
                    _activities.Remove(activity);
                }
            }

            if (e.Message.DomainUpdateType == DomainUpdateType.Update ||
                e.Message.DomainUpdateType == DomainUpdateType.Insert)
            {
                IActivity activity = _activityRepository.Get(e.Message.DomainObjectId);
                if (activity != null) _activities.Add(activity);
            }
            _manageAlarmSituationView.RefreshGrid();
        }

        public void OnRtaStateGroupEvent(object sender, EventMessageArgs e)
        {
            if (e.Message.DomainUpdateType == DomainUpdateType.Update ||
                e.Message.DomainUpdateType == DomainUpdateType.Delete)
            {
				IRtaStateGroup stateGroup = _rtaStateGroups.FirstOrDefault(s => s != null && s.Id == e.Message.DomainObjectId);
                if (stateGroup != null)
                {
                    _rtaStateGroups.Remove(stateGroup);
                }
            }

            if (e.Message.DomainUpdateType == DomainUpdateType.Update ||
                e.Message.DomainUpdateType == DomainUpdateType.Insert)
            {
                IRtaStateGroup stateGroup = _rtaStateGroupRepository.Get(e.Message.DomainObjectId);
                if (stateGroup != null) _rtaStateGroups.Add(stateGroup);
            }
            _manageAlarmSituationView.RefreshGrid();
        }

		public void OnSave()
		{
			foreach (var stateGroupActivityAlarm in _stateGroupActivityAlarmsToRemove)
			{
				if (stateGroupActivityAlarm.Id.HasValue)
				{
					_stateGroupActivityAlarmRepository.Remove(stateGroupActivityAlarm);
				}
			}

			foreach (var stateGroupActivityAlarm in _stateGroupActivityAlarmsToAdd)
			{
				_stateGroupActivityAlarmRepository.Add(stateGroupActivityAlarm);
			}

			_stateGroupActivityAlarmsToAdd.Clear();
			_stateGroupActivityAlarmsToRemove.Clear();
		}

        public int RowCount
        {
            get { return _activities.Count; }
        }

        public int ColCount
        {
            get { return _rtaStateGroups.Count; }
        }

        /// <summary>
        /// event returns the expected rowcount
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridRowColCountEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-11-18
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public void QueryRowCount(object sender, GridRowColCountEventArgs e)
        {
            e.Count = _activities.Count;
            e.Handled = true;
        }

        /// <summary>
        /// event returns the expected count.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridRowColCountEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-11-18
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public void QueryColCount(object sender, GridRowColCountEventArgs e)
        {
            e.Count =  _rtaStateGroups.Count;
            e.Handled = true;
        }

        /// <summary>
        /// event formats the cells
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-11-18
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public void QueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColIndex < 0)
                return; // Bad index
            if (e.RowIndex == 0 && e.ColIndex == 0)
                return;
            if (e.RowIndex == 0)
                QueryHeader(e);
            else if (e.ColIndex == 0)
                QueryRowHeader(e);
            else
            {
                QueryCombo(e);
            }

            e.Handled = true;
        }

        /// <summary>
        /// adds a combobox in the grid and initialize the values and selects the value to show if 
        /// it is created before
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-11-18
        /// </remarks>
        private void QueryCombo(GridQueryCellInfoEventArgs e)
        {
            e.Style.CellType = "ComboBox";
            var list = new StringCollection();
            list.Add(string.Empty); // Unknown item
            foreach (IAlarmType type in _alarmTypes)
            {
                list.Add(type.Description.Name);
            }
            e.Style.ChoiceList = list;
            e.Style.DropDownStyle = GridDropDownStyle.Exclusive;

            var rtaStateGroup = _rtaStateGroups[e.ColIndex - 1];
            var activity = _activities[e.RowIndex - 1];
            IStateGroupActivityAlarm stateGroupActivityAlarm =
                _stateGroupActivityAlarms.SingleOrDefault(item => isActivityMatch(item,activity) && isStateGroupMatch(item,rtaStateGroup));
            if (stateGroupActivityAlarm != null && stateGroupActivityAlarm.AlarmType != null)
            {
                e.Style.CellValue = stateGroupActivityAlarm.AlarmType.Description.Name;
                e.Style.BackColor = stateGroupActivityAlarm.AlarmType.DisplayColor;
            }
            else
            {
                e.Style.BackColor = Color.DarkGray;
            }
        }

		private static bool isStateGroupMatch(IStateGroupActivityAlarm stateGroupActivityAlarm, IRtaStateGroup rtaStateGroup)
		{
			if (stateGroupActivityAlarm.StateGroup == null && rtaStateGroup == null)
			{
				return true;
			}
			if (stateGroupActivityAlarm.StateGroup == null || rtaStateGroup == null)
			{
				return false;
			}
			return stateGroupActivityAlarm.StateGroup.Equals(rtaStateGroup);
		}

		private static bool isActivityMatch(IStateGroupActivityAlarm stateGroupActivityAlarm, IActivity activity)
		{
			if (stateGroupActivityAlarm.Activity == null && activity == null)
			{
				return true;
			}
			if (stateGroupActivityAlarm.Activity == null || activity == null)
			{
				return false;
			}
			return stateGroupActivityAlarm.Activity.Equals(activity);
		}

        /// <summary>
        /// set the rowheadername
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-11-18
        /// </remarks>
        private void QueryRowHeader(GridQueryCellInfoEventArgs e)
        {
            IActivity activity = _activities[e.RowIndex - 1];
            if (activity == null)
            {
                e.Style.Text = Resources.NoScheduledActivity ;
            }
            else
            {
                e.Style.Text = activity.Name; 
            }
        }

        /// <summary>
        /// sets the column header
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-11-18
        /// </remarks>
        private void QueryHeader(GridQueryCellInfoEventArgs e)
        {
            IRtaStateGroup stateGroup = _rtaStateGroups[e.ColIndex - 1];
            if (stateGroup == null)
            {
                e.Style.Text = Resources.NoStateGroupPresent;
            }
            else
            {
                e.Style.Text = stateGroup.Name;
            }
        }

        /// <summary>
        /// Saves the selected value of the cell
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Grid.GridSaveCellInfoEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-11-18
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public void SaveCellInfo(object sender, GridSaveCellInfoEventArgs e)
        {
            if (e.ColIndex == 0 || e.RowIndex == 0)
            {
                // Row or column delete
                return;
            }

            string s = e.Style.Text;
            IAlarmType alarmType = _alarmTypes.SingleOrDefault(a => a.Description.Name == s);

            var rtaStateGroup = _rtaStateGroups[e.ColIndex - 1];
            var activity = _activities[e.RowIndex - 1];
            IStateGroupActivityAlarm stateGroupActivityAlarm =
                _stateGroupActivityAlarms.SingleOrDefault(
                    item => isActivityMatch(item,activity) && isStateGroupMatch(item,rtaStateGroup));

            if (stateGroupActivityAlarm == null)
            {
                stateGroupActivityAlarm = new StateGroupActivityAlarm(rtaStateGroup, activity)
	                {
		                AlarmType = alarmType
	                };
	            _stateGroupActivityAlarms.Add(stateGroupActivityAlarm);
				_stateGroupActivityAlarmsToAdd.Add(stateGroupActivityAlarm);
            }
			//else if (stateGroupActivityAlarm != null && alarmType == null)
			//{
			//	_stateGroupActivityAlarms.Remove(stateGroupActivityAlarm);
			//	_stateGroupActivityAlarmsToRemove.Add(stateGroupActivityAlarm);
			//	_stateGroupActivityAlarmsToAdd.Remove(stateGroupActivityAlarm);
			//}
            else
            {
	            stateGroupActivityAlarm.AlarmType = alarmType;
            }

	        e.Handled = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual dispose method
        /// </summary>
        /// <param name="disposing">
        /// If set to <c>true</c>, explicitly called.
        /// If set to <c>false</c>, implicitly called from finalizer.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseManagedResources();

            }
            ReleaseUnmanagedResources();
        }

        /// <summary>
        /// Releases the unmanaged resources.
        /// </summary>
        protected virtual void ReleaseUnmanagedResources()
        {
        }

        /// <summary>
        /// Releases the managed resources.
        /// </summary>
        protected virtual void ReleaseManagedResources()
        {
            if (_messageBroker != null)
            {
                _messageBroker.UnregisterEventSubscription(OnActivityEvent);
                _messageBroker.UnregisterEventSubscription(OnAlarmEvent);
                _messageBroker.UnregisterEventSubscription(OnRtaStateGroupEvent);
            }
        }
    }
}