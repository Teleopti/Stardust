using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Ccc.WinCode.Settings;
using Teleopti.Interfaces.Domain;
using System.Collections.Specialized;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
	public class ManageAlarmSituationPresenter : IDisposable
	{
		private readonly List<IAlarmType> _alarmTypes = new List<IAlarmType>();
		private readonly List<IActivity> _activities = new List<IActivity>();
		private readonly List<IRtaStateGroup> _rtaStateGroups = new List<IRtaStateGroup>();
		private readonly List<StateGroupActivityAlarmModel> _stateGroupActivityAlarms = new List<StateGroupActivityAlarmModel>();
		private readonly IList<StateGroupActivityAlarmModel> _stateGroupActivityAlarmsToRemove = new List<StateGroupActivityAlarmModel>();
		private readonly IStateGroupActivityAlarmRepository _stateGroupActivityAlarmRepository;
		private readonly IMessageBroker _messageBroker;
		private IManageAlarmSituationView _manageAlarmSituationView;
		private readonly IRtaStateGroupRepository _rtaStateGroupRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IAlarmTypeRepository _alarmTypeRepository;

		private readonly object StateGroupLock = new object();

		public ManageAlarmSituationPresenter(IUnitOfWorkFactory unitOfWorkFactory, IAlarmTypeRepository alarmTypeRepository, IRtaStateGroupRepository rtaStateGroupRepository, IActivityRepository activityRepository, IStateGroupActivityAlarmRepository stateGroupActivityAlarmRepository, IMessageBroker messageBroker, IManageAlarmSituationView manageAlarmSituationView)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_alarmTypeRepository = alarmTypeRepository;
			_rtaStateGroupRepository = rtaStateGroupRepository;
			_activityRepository = activityRepository;
			_stateGroupActivityAlarmRepository = stateGroupActivityAlarmRepository;
			_messageBroker = messageBroker;
			_manageAlarmSituationView = manageAlarmSituationView;
		}

		public void Load()
		{
			lock (StateGroupLock)
			{
				using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
				{
					_alarmTypes.Clear();
					_alarmTypes.AddRange(_alarmTypeRepository.LoadAll());
					_rtaStateGroups.Clear();
					_rtaStateGroups.AddRange(_rtaStateGroupRepository.LoadAllCompleteGraph());
					_rtaStateGroups.Add(null);
					_activities.Clear();
					_activities.AddRange(_activityRepository.LoadAll());
					_activities.Add(null);
					_stateGroupActivityAlarms.AddRange(
						_stateGroupActivityAlarmRepository.LoadAllCompleteGraph().Select(m => new StateGroupActivityAlarmModel(m)));
					_messageBroker.RegisterEventSubscription(OnRtaStateGroupEvent, typeof (IRtaStateGroup));
					_messageBroker.RegisterEventSubscription(OnActivityEvent, typeof (IActivity));
					_messageBroker.RegisterEventSubscription(OnAlarmEvent, typeof (IAlarmType));
				}
			}
		}

		public void OnAlarmEvent(object sender, EventMessageArgs e)
		{
			lock (StateGroupLock)
			{
				using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
				{
					if (e.Message.DomainUpdateType == DomainUpdateType.Update ||
					    e.Message.DomainUpdateType == DomainUpdateType.Delete)
					{
						IAlarmType alarmType = _alarmTypes.FirstOrDefault(s => s != null && s.Id == e.Message.DomainObjectId);
						if (alarmType != null)
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
			}
		}

		public void OnActivityEvent(object sender, EventMessageArgs e)
		{
			lock (StateGroupLock)
			{
				using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
				{
					if (e.Message.DomainUpdateType == DomainUpdateType.Update ||
					    e.Message.DomainUpdateType == DomainUpdateType.Delete)
					{
						IActivity activity = _activities.FirstOrDefault(s => s != null && s.Id == e.Message.DomainObjectId);
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
			}
		}

		public void OnRtaStateGroupEvent(object sender, EventMessageArgs e)
		{
			lock (StateGroupLock)
			{
				using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
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
			}
		}

		public void OnSave()
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				using (uow.DisableFilter(QueryFilter.Deleted))
				{
					_activityRepository.LoadAll();
					_alarmTypeRepository.LoadAll();
					_rtaStateGroupRepository.LoadAll();
				}
				foreach (var stateGroupActivityAlarm in _stateGroupActivityAlarmsToRemove)
				{
					if (stateGroupActivityAlarm.Id.HasValue)
					{
						_stateGroupActivityAlarmRepository.Remove(stateGroupActivityAlarm.ContainedEntity);
					}
				}

				foreach (var stateGroupActivityAlarm in _stateGroupActivityAlarms)
				{
					if (!stateGroupActivityAlarm.Id.HasValue)
					{
						_stateGroupActivityAlarmRepository.Add(stateGroupActivityAlarm.ContainedEntity);
						stateGroupActivityAlarm.UpdateAfterMerge(stateGroupActivityAlarm.ContainedEntity);
					}
					else
					{
						uow.Reassociate(stateGroupActivityAlarm.ContainedOriginalEntity);
						var newState = uow.Merge(stateGroupActivityAlarm.ContainedEntity);
						stateGroupActivityAlarm.UpdateAfterMerge(newState);
					}
				}

				uow.PersistAll();
				_stateGroupActivityAlarmsToRemove.Clear(); 
				foreach (var stateGroupView in _stateGroupActivityAlarms)
					stateGroupView.ResetRtaStateGroupState(null);
				
			}
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
			e.Count = _rtaStateGroups.Count;
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
				QueryCombo(e);

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
			if (e.ColIndex > _rtaStateGroups.Count) return;
			if (e.RowIndex > _activities.Count) return;

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
			var stateGroupActivityAlarm =
				_stateGroupActivityAlarms.SingleOrDefault(item => isActivityMatch(item, activity) && isStateGroupMatch(item, rtaStateGroup));
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

		private static bool isStateGroupMatch(StateGroupActivityAlarmModel stateGroupActivityAlarm, IRtaStateGroup rtaStateGroup)
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

		private static bool isActivityMatch(StateGroupActivityAlarmModel stateGroupActivityAlarm, IActivity activity)
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
			if (_activities.Count < e.RowIndex) return;

			IActivity activity = _activities[e.RowIndex - 1];
			if (activity == null)
			{
				e.Style.Text = Resources.NoScheduledActivity;
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
			if (_rtaStateGroups.Count < e.ColIndex) return;

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

			if (e.ColIndex > _rtaStateGroups.Count) return;
			if (e.RowIndex > _activities.Count) return;

			string s = e.Style.Text;
			IAlarmType alarmType = _alarmTypes.SingleOrDefault(a => a.Description.Name == s);

			var rtaStateGroup = _rtaStateGroups[e.ColIndex - 1];
			var activity = _activities[e.RowIndex - 1];
			var stateGroupActivityAlarm =
				_stateGroupActivityAlarms.SingleOrDefault(
					item => isActivityMatch(item, activity) && isStateGroupMatch(item, rtaStateGroup));

			if (stateGroupActivityAlarm == null)
			{
				var situation = new StateGroupActivityAlarm(rtaStateGroup, activity)
					{
						AlarmType = alarmType
					};
				stateGroupActivityAlarm = new StateGroupActivityAlarmModel(situation);
				_stateGroupActivityAlarms.Add(stateGroupActivityAlarm);
			}
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
			_manageAlarmSituationView = null;
		}
	}
}