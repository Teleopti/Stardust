using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration.Columns;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{

	public partial class SiteControl : BaseUserControl, ISettingPage, ISiteView
	{
		private IUnitOfWork _unitOfWork;
		private SFGridColumnGridHelper<ISite> _columnGridHelper;
		private SitePresenter _presenter;

		public SiteControl()
		{
			InitializeComponent();
		}
	   
		private ReadOnlyCollection<SFGridColumnBase<ISite>> configureGrid()
		{
			// Holds he column list for the grid control
			IList<SFGridColumnBase<ISite>> gridColumns = new List<SFGridColumnBase<ISite>>();
			
			// Adds the cell models to the grid control
			addCellmodels();
			// Set the header count for the grid control
			gridControlSites.Rows.HeaderCount = 0;
			// Adds the header column for the grid control
			gridColumns.Add(new SFGridRowHeaderColumn<ISite>(string.Empty));
			gridColumns.Add(new SFGridDescriptionNameColumn<ISite>("Description", Resources.Name));
			gridColumns.Add(new SFGridNullableIntegerCellColumn<ISite>("MaxSeats", Resources.MaxSeats, 100));
			gridColumns.AppendAuditColumns();

			return new ReadOnlyCollection<SFGridColumnBase<ISite>>(gridColumns);
		}

		private void addCellmodels()
		{
			// Adds the cell models to the grid control
			gridControlSites.CellModels.Add("NullableIntegerCellModel", new NullableIntegerCellModel(gridControlSites.Model) { MinValue = 1 });
			gridControlSites.CellModels.Add("DescriptionNameCell", new DescriptionNameCellModel(gridControlSites.Model));
			gridControlSites.CellModels.Add("DescriptionShortNameCellModel",
									   new DescriptionShortNameCellModel(gridControlSites.Model));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
			
			var columns = configureGrid();
			_columnGridHelper = new SFGridColumnGridHelper<ISite>(gridControlSites, columns, new List<ISite>(),false) { AllowExtendedCopyPaste = false };
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			if (labelSubHeader2 != null) labelSubHeader2.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();

			gridControlSites.BackColor = ColorHelper.GridControlGridInteriorColor();
			gridControlSites.Properties.BackgroundColor = ColorHelper.WizardBackgroundColor();
		}
		
		public void LoadControl()
		{
			_presenter.OnPageLoad();
		}

		public void SaveChanges()
		{
			Persist();
		}

		public void Unload()
		{
		}
		
		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.OrganizationHierarchy);
		}

		public string TreeNode()
		{
			return Resources.Sites;
		}

		public void OnShow()
		{
			_presenter.OnPageLoad();
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
			_unitOfWork = value;
			_presenter = new SitePresenter(this, SiteRepository.DONT_USE_CTOR(_unitOfWork));
		}

		public void Persist()
		{}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
		}

		public ViewType ViewType
		{
			get { return ViewType.Sites; }
		}

		public void LoadSiteGrid(IList<ISite> allNotDeletedSites)
		{
			_columnGridHelper.SetSourceList(allNotDeletedSites);
			gridControlSites.ColWidths.ResizeToFit(GridRangeInfo.Table(), GridResizeToFitOptions.IncludeHeaders);
		}
	}
}
