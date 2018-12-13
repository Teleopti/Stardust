using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;
using Teleopti.Ccc.UserTexts;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
	public class ManageAlarmSituationPresenter : IDisposable
	{
		private readonly List<IRtaRule> _rules = new List<IRtaRule>();
		private readonly List<IActivity> _activities = new List<IActivity>();
		private readonly List<IRtaStateGroup> _rtaStateGroups = new List<IRtaStateGroup>();
		private readonly List<RtaMapModel> _rtaMaps = new List<RtaMapModel>();
		private readonly IList<RtaMapModel> _rtaMapModelsToRemove = new List<RtaMapModel>();
		private readonly IRtaMapRepository _rtaMapRepository;
		private readonly IMessageBrokerComposite _messageBroker;
		private IManageAlarmSituationView _manageAlarmSituationView;
		private readonly IRtaStateGroupRepository _rtaStateGroupRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRtaRuleRepository _rtaRuleRepository;

		private readonly object StateGroupLock = new object();

		public ManageAlarmSituationPresenter(IUnitOfWorkFactory unitOfWorkFactory, IRtaRuleRepository rtaRuleRepository, IRtaStateGroupRepository rtaStateGroupRepository, IActivityRepository activityRepository, IRtaMapRepository rtaMapRepository, IMessageBrokerComposite messageBroker, IManageAlarmSituationView manageAlarmSituationView)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_rtaRuleRepository = rtaRuleRepository;
			_rtaStateGroupRepository = rtaStateGroupRepository;
			_activityRepository = activityRepository;
			_rtaMapRepository = rtaMapRepository;
			_messageBroker = messageBroker;
			_manageAlarmSituationView = manageAlarmSituationView;
		}

		public void Load()
		{
			lock (StateGroupLock)
			{
				using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
				{
					_rules.Clear();
					_rules.AddRange(_rtaRuleRepository.LoadAll());
					_rtaStateGroups.Clear();
					_rtaStateGroups.AddRange(_rtaStateGroupRepository.LoadAllCompleteGraph());
					_rtaStateGroups.Add(null);
					_activities.Clear();
					_activities.AddRange(_activityRepository.LoadAll());
					_activities.Add(null);
					_rtaMaps.AddRange(
						_rtaMapRepository.LoadAllCompleteGraph().Select(m => new RtaMapModel(m)));
					_messageBroker.RegisterEventSubscription(OnRtaStateGroupEvent, typeof (IRtaStateGroup));
					_messageBroker.RegisterEventSubscription(OnActivityEvent, typeof (IActivity));
					_messageBroker.RegisterEventSubscription(OnAlarmEvent, typeof (IRtaRule));
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
						IRtaRule _rtaRule = _rules.FirstOrDefault(s => s != null && s.Id == e.Message.DomainObjectId);
						if (_rtaRule != null)
						{
							_rules.Remove(_rtaRule);
						}
					}

					if (e.Message.DomainUpdateType == DomainUpdateType.Update ||
					    e.Message.DomainUpdateType == DomainUpdateType.Insert)
					{
						IRtaRule _rtaRule = _rtaRuleRepository.Get(e.Message.DomainObjectId);
						if (_rtaRule != null) _rules.Add(_rtaRule);
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
					_rtaRuleRepository.LoadAll();
					_rtaStateGroupRepository.LoadAll();
				}
				foreach (var rtaMapModel in _rtaMapModelsToRemove)
				{
					if (rtaMapModel.Id.HasValue)
					{
						_rtaMapRepository.Remove(rtaMapModel.ContainedEntity);
					}
				}

				foreach (var map in _rtaMaps)
				{
					if (!map.Id.HasValue)
					{
						_rtaMapRepository.Add(map.ContainedEntity);
						map.UpdateAfterMerge(map.ContainedEntity);
					}
					else
					{
						uow.Reassociate(map.ContainedOriginalEntity);
						var newState = uow.Merge(map.ContainedEntity);
						map.UpdateAfterMerge(newState);
					}
				}

				uow.PersistAll();
				_rtaMapModelsToRemove.Clear(); 
				foreach (var stateGroupView in _rtaMaps)
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
			foreach (IRtaRule type in _rules)
			{
				list.Add(type.Description.Name);
			}
			e.Style.ChoiceList = list;
			e.Style.DropDownStyle = GridDropDownStyle.Exclusive;

			var rtaStateGroup = _rtaStateGroups[e.ColIndex - 1];
			var activity = _activities[e.RowIndex - 1];
			var rtaMap =
				_rtaMaps.SingleOrDefault(item => isActivityMatch(item, activity) && isStateGroupMatch(item, rtaStateGroup));
			if (rtaMap != null && rtaMap.RtaRule != null)
			{
				e.Style.CellValue = rtaMap.RtaRule.Description.Name;
				e.Style.BackColor = rtaMap.RtaRule.DisplayColor;
			}
			else
			{
				e.Style.BackColor = Color.DarkGray;
			}
		}

		private static bool isStateGroupMatch(RtaMapModel rtaMap, IRtaStateGroup rtaStateGroup)
		{
			if (rtaMap.StateGroup == null && rtaStateGroup == null)
			{
				return true;
			}
			if (rtaMap.StateGroup == null || rtaStateGroup == null)
			{
				return false;
			}
			return rtaMap.StateGroup.Equals(rtaStateGroup);
		}

		private static bool isActivityMatch(RtaMapModel rtaMap, IActivity activity)
		{
			if (rtaMap.Activity == null && activity == null)
			{
				return true;
			}
			if (rtaMap.Activity == null || activity == null)
			{
				return false;
			}
			return rtaMap.Activity.Equals(activity.Id.GetValueOrDefault());
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
			IRtaRule _rtaRule = _rules.SingleOrDefault(a => a.Description.Name == s);

			var rtaStateGroup = _rtaStateGroups[e.ColIndex - 1];
			var activity = _activities[e.RowIndex - 1];
			var rtaMap =
				_rtaMaps.SingleOrDefault(
					item => isActivityMatch(item, activity) && isStateGroupMatch(item, rtaStateGroup));

			if (rtaMap == null)
			{
				var situation = new RtaMap
				{
					StateGroup = rtaStateGroup,
					Activity = activity.Id.Value,
					RtaRule = _rtaRule
				};
				rtaMap = new RtaMapModel(situation);
				_rtaMaps.Add(rtaMap);
			}
			else
			{
				rtaMap.RtaRule = _rtaRule;
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
				_messageBroker.UnregisterSubscription(OnActivityEvent);
				_messageBroker.UnregisterSubscription(OnAlarmEvent);
				_messageBroker.UnregisterSubscription(OnRtaStateGroupEvent);
			}
			_manageAlarmSituationView = null;
		}
	}
}