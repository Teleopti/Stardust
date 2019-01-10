using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Main;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.PayrollExportPages;
using Teleopti.Ccc.Win.Main;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll
{
	public partial class PayrollExportNavigator : AbstractNavigator
	{
		private TreeNode _lastNode;
		private readonly PortalSettings _portalSettings;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		//private const string Assembly = "Teleopti.Ccc.Win";
		private const string PayrollExports = ".PayrollExportsSmartPart";
		private const string ClassPrefix = "Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts.Payroll";
		private static BackgroundWorker _payrollBackgroundWorker;
		private readonly IComponentContext _componentContext;
		private readonly IPayrollResultRepository _payrollResultRepository;
		private readonly IStardustSender _stardustSender;
		private readonly IApplicationInsights _applicationInsights;


		public PayrollExportNavigator(PortalSettings portalSettings, IRepositoryFactory repositoryFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IComponentContext componentContext, 
			IPayrollResultRepository payrollResultRepository,
			IStardustSender stardustSender, IApplicationInsights applicationInsights)
		{
			InitializeComponent();

			if (DesignMode) return;

			SetTexts();
			setColors();

			EntityEventAggregator.EntitiesNeedsRefresh += entitiesNeedsRefresh;
			splitContainer1.SplitterMoved += splitContainer1_SplitterMoved;


			treeViewMain.Sorted = true;
			if (_payrollBackgroundWorker == null)
			{
				_payrollBackgroundWorker = new BackgroundWorker();
				_payrollBackgroundWorker.WorkerSupportsCancellation = false;
				_payrollBackgroundWorker.WorkerReportsProgress = false;
				_payrollBackgroundWorker.DoWork += payrollBackgroundWorker_DoWork;
				_payrollBackgroundWorker.RunWorkerCompleted += payrollBackgroundWorker_RunWorkerCompleted;
			}
			_portalSettings = portalSettings;
			_repositoryFactory = repositoryFactory;
			_unitOfWorkFactory = unitOfWorkFactory;
			_componentContext = componentContext;
			
			_payrollResultRepository = payrollResultRepository;
			_stardustSender = stardustSender;
			_applicationInsights = applicationInsights;
		}

		private void setColors()
		{
			BackColor = ColorHelper.StandardTreeBackgroundColor();
			treeViewMain.BackColor = ColorHelper.StandardTreeBackgroundColor();
			contextMenuStripPayrollExport.BackColor = ColorHelper.StandardContextMenuColor();
			toolStripPayrollExport.BackColor = ColorHelper.StandardTreeBackgroundColor();
			splitContainer1.BackColor = ColorHelper.PortalTreeViewSeparator();
			splitContainer1.Panel2.BackColor = ColorHelper.StandardTreeBackgroundColor();

		}

		#region Event handling when entities are updated

		private void entitiesNeedsRefresh(object sender, EntitiesUpdatedEventArgs e)
		{
			using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				if (typeof(IPayrollExport).IsAssignableFrom(e.EntityType))
				{
					foreach (Guid guid in e.UpdatedIds)
					{
						TreeNode[] foundNodes = treeViewMain.Nodes.Find(guid.ToString(), true);
						if (foundNodes.Length > 0) foundNodes[0].Remove();

						IPayrollExport export = _repositoryFactory.CreatePayrollExportRepository(uow).Get(guid);
						var dTag = export as IDeleteTag;

						if (export == null) continue;
						if (dTag != null && dTag.IsDeleted)
						{
							foundNodes = treeViewMain.Nodes.Find(export.Id.ToString(), true);
							if (foundNodes.Length > 0) foundNodes[0].Tag = export;
						}
						else
						{
							TreeNode payrollExportNode = getPayrollExportNode(export);
							treeViewMain.Nodes.Add(payrollExportNode);
							payrollExportNode.EnsureVisible();
						}
					}
				}
			}
		}


		#endregion

		private IEnumerable<IPayrollExport> loadPayrollExportsCollection(IUnitOfWork uow)
		{
			IPayrollExportRepository payrollExportRepository = _repositoryFactory.CreatePayrollExportRepository(uow);
			var payrollExports = payrollExportRepository.LoadAll();
			return payrollExports;
		}

		private void loadPayrollExportsTree(TreeNodeCollection nodes, IUnitOfWork uow)
		{
			nodes.Clear();

			IEnumerable<IPayrollExport> payrollExports = loadPayrollExportsCollection(uow);
			foreach (IPayrollExport payrollExport in payrollExports)
			{
				addPayrollExportToTreeview(nodes, payrollExport);
			}
		}

		private static void addPayrollExportToTreeview(TreeNodeCollection nodes, IPayrollExport payrollExport)
		{
			TreeNode aNode = getPayrollExportNode(payrollExport);
			nodes.Add(aNode);
		}

		private static TreeNode getPayrollExportNode(IPayrollExport payrollExport)
		{
			var aNode = new TreeNode(payrollExport.Name)
			{
				Name = payrollExport.Id.ToString(),
				Tag = payrollExport,
				ImageIndex = 0
			};
			return aNode;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				loadPayrollExportsTree(treeViewMain.Nodes, uow);
				treeViewMain.ExpandAll();
			}

			if (treeViewMain.Nodes.Count > 0)
			{
				treeViewMain.SelectedNode = treeViewMain.Nodes[0];
				fakeNodeClickColor();
			}

			checkLastNode();
			splitContainer1.SplitterDistance = splitContainer1.Height - _portalSettings.PayrollActionPaneHeight;
		}

		private void payrollBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (IsDisposed) return;
			if (e.Error != null)
			{
				LogManager.GetLogger(GetType()).Error("An error occured when trying to run a payroll export.", e.Error);
				ViewBase.ShowErrorMessage(
					 string.Concat(UserTexts.Resources.CommunicationErrorEndPoint, Environment.NewLine,
										Environment.NewLine, e.Error.Message), UserTexts.Resources.ErrorMessage);
			}
		}

		private void payrollBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var payrollExport = e.Argument as IPayrollExport;
			if (payrollExport == null) return;

			_applicationInsights.TrackEvent("Ran selected payroll in Payroll Integration Module.");
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var personRepository = _repositoryFactory.CreatePersonRepository(uow);
				var exportingPersonDomain =	personRepository.Get(TeleoptiPrincipal.CurrentPrincipal.PersonId);

				var payrollExportRepository = _repositoryFactory.CreatePayrollExportRepository(uow);
				payrollExport = payrollExportRepository.Load(payrollExport.Id.GetValueOrDefault());

				IPayrollResult payrollResult = new PayrollResult(payrollExport, exportingPersonDomain, DateTime.UtcNow);

				payrollResult.PayrollExport = payrollExport;
				_payrollResultRepository.Add(payrollResult);

				uow.PersistAll();
				var payrollResultId = payrollResult.Id.GetValueOrDefault();
				var personId = ((ITeleoptiPrincipalForLegacy) TeleoptiPrincipal.CurrentPrincipal).UnsafePerson.Id.GetValueOrDefault(Guid.Empty);
				var message = new RunPayrollExportEvent
				{
					PayrollExportId = payrollExport.Id.GetValueOrDefault(Guid.Empty),
					OwnerPersonId = personId,
					ExportStartDate = payrollExport.Period.StartDate.Date,
					ExportEndDate = payrollExport.Period.EndDate.Date,
					PayrollExportFormatId = payrollExport.PayrollFormatId,
					PayrollResultId = payrollResultId,
					InitiatorId = personId,
					LogOnBusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.GetValueOrDefault()
				};
				if (payrollExport.Persons == null || payrollExport.Persons.Count == 0)
				{
					message.ExportPersonIdCollection = new Collection<Guid>();
				}
				if (payrollExport.Persons != null && payrollExport.Persons.Count > 0)
				{
					message.ExportPersonIdCollection =
						payrollExport.Persons.Select(p => p.Id.GetValueOrDefault()).ToList();
				}
				try
				{
					_stardustSender.Send(message);
				}
				catch
				{
					ViewBase.ShowErrorMessage("An error occured when trying to run a payroll export.", "");
				}

			}
		}

		
		private void fakeNodeClickColor()
		{
			treeViewMain.Nodes[treeViewMain.Nodes[0].Index].BackColor = ColorHelper.SelectedTreeViewNodeBackColor();
			treeViewMain.Nodes[treeViewMain.Nodes[0].Index].ForeColor = ColorHelper.SelectedTreeViewNodeForeColor();

		}
		
		private void unFakeClickColor()
		{
			treeViewMain.Nodes[treeViewMain.Nodes[0].Index].BackColor = ColorHelper.StandardTreeBackgroundColor();
			treeViewMain.Nodes[treeViewMain.Nodes[0].Index].ForeColor = Color.Black;
		}

		private void LoadSmartPart(Guid payrollExport, int smartPartId, string smartPartHeaderTitle, string smartPartName, int row, int col)
		{
			var smartPartInfo = new SmartPartInformation();
			smartPartInfo.ContainingAssembly = GetType().Assembly;
			smartPartInfo.SmartPartName = smartPartName;
			smartPartInfo.SmartPartHeaderTitle = smartPartHeaderTitle;
			smartPartInfo.GridColumn = col;
			smartPartInfo.GridRow = row;
			smartPartInfo.SmartPartId = smartPartId.ToString(CultureInfo.CurrentCulture);  // this need to be unique

			// Create SmartPart Parameters  [optional]
			IList<SmartPartParameter> parameters = new List<SmartPartParameter>();
			var parameter = new SmartPartParameter("PayrollExport", payrollExport);

			parameters.Add(parameter);

			try
			{
				// Invoke SmartPart
				SmartPartInvoker.ShowSmartPart(smartPartInfo, parameters);
			}
			catch (FileLoadException)
			{
				//TODO:need to log exception
			}
			catch (FileNotFoundException)
			{
				//TODO:need to log exception
			}
		}

		private void setNodeFromPosition(MouseEventArgs e)
		{
			TreeNode tn = treeViewMain.GetNodeAt(e.X, e.Y);
			_lastNode = tn;
			checkLastNode();
		}

		private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (_portalSettings != null)
				_portalSettings.PayrollActionPaneHeight = splitContainer1.Height - splitContainer1.SplitterDistance;
		}

		private void toolStripMenuItemNew_Click(object sender, EventArgs e)
		{
			DialogResult result;
			IPayrollExport aggregateRootObject;
			using (var pewp = new PayrollExportWizardPages(_repositoryFactory, _unitOfWorkFactory))
			{
				pewp.Initialize(PropertyPagesHelper.GetPayrollExportPages(true, _componentContext), new LazyLoadingManagerWrapper());
				using (var wizard = new Wizard(pewp))
				{
					pewp.Saved += pewp_Saved;

					result = wizard.ShowDialog(this);
					aggregateRootObject = pewp.AggregateRootObject;
				}
			}
			if (result == DialogResult.OK)
			{
				runExport(aggregateRootObject);
			}
		}

		void pewp_Saved(object sender, AfterSavedEventArgs e)
		{
			var pepp = (AbstractPropertyPages<IPayrollExport>)sender;
			pepp.LoadAggregateRootWorkingCopy();
			IPayrollExport payrollExport = pepp.AggregateRootObject;
			addPayrollExportToTreeview(treeViewMain.Nodes, payrollExport);
		}

		private void toolStripMenuItemNew2_Click(object sender, EventArgs e)
		{
			toolStripMenuItemNew_Click(sender, e);
		}

		private void treeViewMain_MouseDown(object sender, MouseEventArgs e)
		{
			setNodeFromPosition(e);
			//note i undo the fake paint
			if (treeViewMain.Nodes.Count > 0)
			{
				if (treeViewMain.Nodes[0].BackColor != ColorHelper.StandardTreeBackgroundColor()) unFakeClickColor();
			}
		}

		private void toolStripMenuItemProperties_Click(object sender, EventArgs e)
		{
			checkLastNode();
			if (_lastNode == null)
				return;

			var payrollExport = (IPayrollExport)_lastNode.Tag;
			using (var pepp = new PayrollExportPropertiesPages(payrollExport, _repositoryFactory, _unitOfWorkFactory))
			{
				pepp.Initialize(PropertyPagesHelper.GetPayrollExportPages(false, _componentContext), new LazyLoadingManagerWrapper());
				using (var pp = new PropertiesPages(pepp))
				{
					pp.ShowDialog(this);
				}

			}
		}

		private void checkLastNode()
		{
			if (_lastNode == null)
			{
				//with no item selected these are not viable
				toolStripMenuItemProperties.Enabled = false;
				toolStripMenuItemDelete.Enabled = false;
				toolStripMenuItemRunExport.Enabled = false;
				toolStripMenuItemContextRunExport.Enabled = false;
				return;
			}
			toolStripMenuItemProperties.Enabled = true;
			toolStripMenuItemDelete.Enabled = true;

			toolStripMenuItemRunExport.Enabled = true;
			toolStripMenuItemContextRunExport.Enabled = true;
		}
		private void toolStripMenuItemProperties2_Click(object sender, EventArgs e)
		{
			toolStripMenuItemProperties_Click(sender, e);
		}

		private void toolStripMenuItemDelete2_Click(object sender, EventArgs e)
		{
			toolStripMenuItemDelete_Click(sender, e);
		}

		private void toolStripMenuItemDelete_Click(object sender, EventArgs e)
		{
			checkLastNode();
			if (_lastNode == null)
				return;

			var payrollExport = (IPayrollExport)_lastNode.Tag;
			string questionString = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.QuestionDelete, "\"", payrollExport.Name);
			if (ViewBase.ShowYesNoMessage(this, questionString, UserTexts.Resources.Delete) == DialogResult.Yes)
			{
				removePayrollExport(payrollExport);
			}
		}

		private void removePayrollExport(IPayrollExport payrollExport)
		{
			IEnumerable<IRootChangeInfo> changes;
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var payrollExportRepository = _repositoryFactory.CreatePayrollExportRepository(uow);
				payrollExport = payrollExportRepository.Load(payrollExport.Id.GetValueOrDefault());
				payrollExportRepository.Remove(payrollExport);
				changes = uow.PersistAll();
			}
			Main.EntityEventAggregator.TriggerEntitiesNeedRefresh(ParentForm, changes);
		}

		private void treeViewMain_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			_lastNode = e.Node;

			var payrollExport = _lastNode.Tag as IPayrollExport;
			if (payrollExport == null) return;
			//with no item selected these are not viable
			toolStripMenuItemRunExport.Enabled = true;
			toolStripMenuItemContextRunExport.Enabled = true;
			LoadSmartPart(payrollExport.Id.GetValueOrDefault(), 1, UserTexts.Resources.PayrollExport, ClassPrefix + PayrollExports, 0, 0);
		}

		private void toolStripMenuItemRunExport_Click(object sender, EventArgs e)
		{
			runExport((IPayrollExport)_lastNode.Tag);
		}
		private void toolStripMenuItemContextRunExport_Click(object sender, EventArgs e)
		{
			runExport((IPayrollExport)_lastNode.Tag);

		}
		private void runExport(IPayrollExport payrollExport)
		{
			if (_payrollBackgroundWorker.IsBusy)
			{
				ViewBase.ShowInformationMessage(this, UserTexts.Resources.SystemIsBusyWithBackgroundRequest, UserTexts.Resources.PleaseTryAgainLater);
				return;
			}

			_payrollBackgroundWorker.RunWorkerAsync(payrollExport);
		}
	}
}
