using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Views
{
	public enum ViewType
	{
		GeneralView,
		RolesView,
		SkillsView,
		PeoplePeriodView,
		SchedulePeriodView,
		EmptyView,
		PersonRotationView,
		ExternalLogOnView,
		PersonalAccountGridView,
		PersonAvailabilityView
	};

	public class GridConstructor : Component
	{
		private FilteredPeopleHolder _filteredPeopleHolder;
		private IToggleManager _toggleManager;
		private readonly IBusinessRuleConfigProvider _businessRuleConfigProvider;
		private readonly IConfigReader _configReader;

		public GridConstructor(FilteredPeopleHolder filteredPeopleHolder, IToggleManager toggleManager, IBusinessRuleConfigProvider businessRuleConfigProvider, IConfigReader configReader)
		{
			_filteredPeopleHolder = filteredPeopleHolder;
			_toggleManager = toggleManager;
			_businessRuleConfigProvider = businessRuleConfigProvider;
			_configReader = configReader;

			_readOnly =
				 !PrincipalAuthorization.Current_DONTUSE().IsPermitted(
					  DefinedRaptorApplicationFunctionPaths.AllowPersonModifications);
		}

		private Dictionary<ViewType, GridViewBase> _viewCache = new Dictionary<ViewType, GridViewBase>();

		public void FlushCache()
		{
			if (_viewCache != null)
			{
				foreach (var value in _viewCache.Values)
					value.Dispose();
				_viewCache.Clear();
			}
		}

		public void Sort(IEnumerable<Tuple<IPerson, int>> sorting)
		{
			_sorting = sorting;
			View.PerformSort(_sorting);
		}

		public event EventHandler GridViewChanged;

		public event EventHandler GridViewChanging;

		public ViewType CurrentView { get; private set; }

		public void BuildGridView(ViewType type)
		{
			GridViewChanging?.Invoke(_view, EventArgs.Empty);

			// Sets the current view
			CurrentView = type;

			// Cache view (If not).
			IsCached = _viewCache.ContainsKey(type);
			if (!IsCached) cacheGridView(type);
			else View.SetFilteredPerson(_filteredPeopleHolder);

			_view = _viewCache[type];

			_view.Grid.Properties.BackgroundColor = ColorHelper.GridControlGridExteriorColor();
			
			GridViewChanged?.Invoke(_view, EventArgs.Empty);
			_view.PerformSort(_sorting);

			if (!IsCached)
			{
				_view.Grid.BeginUpdate();

				_view.ClearView();
				_view.CreateHeaders();
				if (!_readOnly)
					_view.CreateContextMenu();
				_view.PrepareView();
				_view.MergeHeaders();
				_view.Grid.EndUpdate();
			}
		}

		public bool IsCached { get; set; }

		private void cacheGridView(ViewType type)
		{
			GridViewBase view;

			switch (type)
			{
				case ViewType.GeneralView:

					view = new GeneralGridView(new GridControl(), _filteredPeopleHolder);
					break;

				case ViewType.RolesView:
					view = new RolesGridView(new GridControl(), _filteredPeopleHolder);
					break;

				case ViewType.PeoplePeriodView:
					view = new PeoplePeriodGridView(new GridControl(), _filteredPeopleHolder);
					break;

				case ViewType.SkillsView:
					view = new PersonSkillGridView(new GridControl(), _filteredPeopleHolder); // TODO: Change this to SkillsGridView.
					break;

				case ViewType.SchedulePeriodView:
					var toggleView = new SchedulePeriodGridView(new GridControl(), _filteredPeopleHolder);
					toggleView.Toggle78424 = _toggleManager.IsEnabled(Toggles.SchedulePeriod_HideChineseMonth_78424);
					view = toggleView;
					break;

				case ViewType.EmptyView:
					view = new EmptyGridView(new GridControl(), _filteredPeopleHolder, false);
					break;

				case ViewType.PersonRotationView:
					view = new RotationBaseGridView<PersonRotationModelParent,
						 PersonRotationModelChild, IPersonRotation, IRotation>(new GridControl(), _filteredPeopleHolder,
						 _filteredPeopleHolder.PersonRotationParentAdapterCollection, ViewType.PersonRotationView, _toggleManager, _businessRuleConfigProvider, _configReader );
					break;

				case ViewType.PersonAvailabilityView:
					view = new RotationBaseGridView<PersonAvailabilityModelParent,
						 PersonAvailabilityModelChild, IPersonAvailability, IAvailabilityRotation>(new GridControl(), _filteredPeopleHolder,
						 _filteredPeopleHolder.PersonAvailabilityParentAdapterCollection, ViewType.PersonAvailabilityView, _toggleManager, _businessRuleConfigProvider, _configReader);
					break;

				case ViewType.ExternalLogOnView:
					view = new ExternalLogOnGridView(new GridControl(), _filteredPeopleHolder);
					break;

				case ViewType.PersonalAccountGridView:
					view = new PersonalAccountGridView(new GridControl(), _filteredPeopleHolder);
					break;

				default:
					throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(ViewType));
			}
			
			if (_viewCache.TryGetValue(type,out var val))
			{
				val?.Dispose();
				_viewCache.Remove(type);
			}
			_viewCache.Add(type, view);
		}

		private GridViewBase _view;

		private readonly bool _readOnly;
		private IEnumerable<Tuple<IPerson, int>> _sorting;

		public GridViewBase View => _view;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_filteredPeopleHolder != null)
				{
					_filteredPeopleHolder.Dispose();
					_filteredPeopleHolder = null;
				}
				
				if (_viewCache != null)
				{
					foreach (var gridViewBase in _viewCache.Values)
					{
						gridViewBase.Dispose();
					}
					_viewCache.Clear();
					_viewCache = null;
				}
				_view = null;
				
				_toggleManager = null;
			}
		}

		public static TabPageAdv WrapWithTabPage(GridControl grid, string label)
		{
			var wrappedTabPage = new TabPageAdv(label);
			grid.Dock = DockStyle.Fill;
			wrappedTabPage.Controls.Add(grid);
			wrappedTabPage.ImageIndex = 0;
			wrappedTabPage.TabIndex = 1;
			wrappedTabPage.Text = label;
			wrappedTabPage.ThemesEnabled = false;

			return wrappedTabPage;
		}

		public static void WrapWithTabPageExternal(GridControl grid, TabPageAdv wrappedTabPageExternal, TableLayoutPanel tableLayoutPanel1)
		{
			if (grid == null) throw new ArgumentNullException(nameof(grid));
			if (wrappedTabPageExternal == null) throw new ArgumentNullException(nameof(wrappedTabPageExternal));
			if (tableLayoutPanel1 == null) throw new ArgumentNullException(nameof(tableLayoutPanel1));
			
			tableLayoutPanel1.Controls.Add(grid, 0, 1);
			grid.Dock = DockStyle.Fill;
			grid.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
			wrappedTabPageExternal.Controls.Add(tableLayoutPanel1);
			tableLayoutPanel1.Dock = DockStyle.Fill;
			wrappedTabPageExternal.ImageIndex = 0;
			wrappedTabPageExternal.ThemesEnabled = false;
		}

		public GridViewBase FindGrid(ViewType type)
		{
			if (_viewCache.TryGetValue(type, out var val))
				return val;
			return null;
		}
	}
}
