using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using log4net;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.ClientProxies;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.PropertyPageAndWizard;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.Win.Payroll.Forms.PayrollExportPages;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Payroll.PayrollExportPages;
using Teleopti.Common.UI.SmartPartControls.SmartParts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Payroll
{
    public partial class PayrollExportNavigator : AbstractNavigator
    {
        private TreeNode _lastNode;
        private readonly PortalSettings _portalSettings;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private const string Assembly = "Teleopti.Ccc.SmartParts";
        private const string PayrollExports = ".PayrollExportsSmartPart";
        private const string ClassPrefix = "Teleopti.Ccc.SmartParts.Payroll";
        private static BackgroundWorker _payrollBackgroundWorker;
        private readonly IComponentContext _componentContext;

        public PayrollExportNavigator()
        {
            InitializeComponent();
            treeViewMain.Sorted = true;
            if (!DesignMode)
            {
                SetTexts();
                SetColors();

                EntityEventAggregator.EntitiesNeedsRefresh += entitiesNeedsRefresh;
                splitContainer1.SplitterMoved += splitContainer1_SplitterMoved;
            }

            if (_payrollBackgroundWorker==null)
            {
                _payrollBackgroundWorker = new BackgroundWorker();
                _payrollBackgroundWorker.WorkerSupportsCancellation = false;
                _payrollBackgroundWorker.WorkerReportsProgress = false;
                _payrollBackgroundWorker.DoWork += payrollBackgroundWorker_DoWork;
                _payrollBackgroundWorker.RunWorkerCompleted += payrollBackgroundWorker_RunWorkerCompleted;
            }
        }

        public PayrollExportNavigator(PortalSettings portalSettings, IRepositoryFactory repositoryFactory,IUnitOfWorkFactory unitOfWorkFactory,
             IComponentContext componentContext)
            : this()
        {
            _portalSettings = portalSettings;
            _repositoryFactory = repositoryFactory;
            _unitOfWorkFactory = unitOfWorkFactory;
            _componentContext = componentContext;
        }

        private void SetColors()
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
                        IDeleteTag dTag = export as IDeleteTag;

                        if (export == null) continue;
                        if (dTag != null && dTag.IsDeleted)
                        {
                            foundNodes = treeViewMain.Nodes.Find(export.Id.ToString(), true);
                            if (foundNodes.Length > 0) foundNodes[0].Tag = export;
                        }
                        else
                        {
                            TreeNode payrollExportNode = GetPayrollExportNode(export);
                            treeViewMain.Nodes.Add(payrollExportNode);
                            payrollExportNode.EnsureVisible();
                        }
                    }
                }
            }
        }


        #endregion

        //Dont know about this but, this might need to be standardized
        //But it works for now, we need to go on with the Sprint
        #region uow_Stuff


        private ICollection<IPayrollExport> loadPayrollExportsCollection(IUnitOfWork uow)
        {
            IPayrollExportRepository payrollExportRepository = _repositoryFactory.CreatePayrollExportRepository(uow);
            ICollection<IPayrollExport> payrollExports = payrollExportRepository.LoadAll();
            return payrollExports;
        }

        #endregion

        private void loadPayrollExportsTree(TreeNodeCollection nodes, IUnitOfWork uow)
        {
            nodes.Clear();

            ICollection<IPayrollExport> payrollExports = loadPayrollExportsCollection(uow);
            foreach (IPayrollExport payrollExport in payrollExports)
            {
                addPayrollExportToTreeview(nodes, payrollExport);
            }
        }

        /// <summary>
        /// Adds the payroll export to treeview.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="payrollExport">The payroll export.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2009-03-20
        /// </remarks>
        private static void addPayrollExportToTreeview(TreeNodeCollection nodes, IPayrollExport payrollExport)
        {
            TreeNode aNode = GetPayrollExportNode(payrollExport);
            nodes.Add(aNode);
        }

        private static TreeNode GetPayrollExportNode(IPayrollExport payrollExport)
        {
            TreeNode aNode = new TreeNode(payrollExport.Name);
            aNode.Name = payrollExport.Id.ToString();
            aNode.Tag = payrollExport;
            aNode.ImageIndex = 0;
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
                FakeNodeClickColor();
            }
            
            CheckLastNode();
            splitContainer1.SplitterDistance = splitContainer1.Height - _portalSettings.PayrollActionPaneHeight;
        }

        private void payrollBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed) return;
            if (e.Error!=null)
            {
                LogManager.GetLogger(GetType()).Error("An error occured when trying to run a payroll export.",e.Error);
                ViewBase.ShowErrorMessage(
                    string.Concat(UserTexts.Resources.CommunicationErrorEndPoint, Environment.NewLine,
                                  Environment.NewLine, e.Error.Message), UserTexts.Resources.ErrorMessage);
            }
        }

        private void payrollBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            IPayrollExport payrollExport = e.Argument as IPayrollExport;
            if (payrollExport == null) return;

            SdkAuthentication sdkAuthentication = new SdkAuthentication();
            sdkAuthentication.SetSdkAuthenticationHeader();

            PayrollExportDto payrollExportDto;
            using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                IPersonRepository personRepository = _repositoryFactory.CreatePersonRepository(uow);
                TeleoptiPrincipal.Current.GetPerson(personRepository);
                
                IPayrollExportRepository payrollExportRepository = _repositoryFactory.CreatePayrollExportRepository(uow);
                payrollExport = payrollExportRepository.Load(payrollExport.Id.GetValueOrDefault());
                
                payrollExportDto = new PayrollExportAssembler(payrollExportRepository, personRepository, null, new DateTimePeriodAssembler()).DomainEntityToDto(payrollExport);
                payrollExportDto.PersonCollection.Clear();

				PersonAssembler personAssembler = new PersonAssembler(null, new WorkflowControlSetAssembler(new ShiftCategoryAssembler(null), new DayOffAssembler(null), new ActivityAssembler(null), new AbsenceAssembler(null)), new PersonAccountUpdaterDummy());
                personAssembler.IgnorePersonPeriods = true;
            }
            
            var proxy = new Proxy();
            try
            {
                proxy.Open();
                proxy.CreateServerPayrollExport(payrollExportDto);
            }
            finally
            {
                ((IDisposable)proxy).Dispose();
            }
        }

        /// <summary>
        /// Fakes the color of a mouse clicked click.
        /// i couldn´t find a way to get the node to have the selected look from the code
        /// if you know a way feel fre to remove this
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-10-13
        /// </remarks>
        private void FakeNodeClickColor()
        {
            treeViewMain.Nodes[treeViewMain.Nodes[0].Index].BackColor = ColorHelper.SelectedTreeViewNodeBackColor();
            treeViewMain.Nodes[treeViewMain.Nodes[0].Index].ForeColor = ColorHelper.SelectedTreeViewNodeForeColor();

        }
        /// <summary>
        /// Unfakes the color of a mouse clicked click.
        /// i couldn´t find a way to get the node to have the selected look from the code
        /// if you know a way feel fre to remove this
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-10-13
        /// </remarks>
        private void UnFakeClickColor()
        {

            treeViewMain.Nodes[treeViewMain.Nodes[0].Index].BackColor = ColorHelper.StandardTreeBackgroundColor();
            treeViewMain.Nodes[treeViewMain.Nodes[0].Index].ForeColor = Color.Black;

        }

        /// <summary>
        /// Loads the smart part.
        /// </summary>
        /// <param name="payrollExport">The payroll export.</param>
        /// <param name="smartPartId">The smart part id.</param>
        /// <param name="smartPartHeaderTitle">The smart part header title.</param>
        /// <param name="smartPartName">Name of the smart part.</param>
        /// <param name="row">The row.</param>
        /// <param name="col">The col.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 9/25/2008
        /// </remarks>
        private static void LoadSmartPart(Guid payrollExport, int smartPartId, string smartPartHeaderTitle, string smartPartName, int row, int col)
        {
            SmartPartInformation smartPartInfo = new SmartPartInformation();
            smartPartInfo.ContainingAssembly = Assembly;
            smartPartInfo.SmartPartName = smartPartName;
            smartPartInfo.SmartPartHeaderTitle = smartPartHeaderTitle;
            smartPartInfo.GridColumn = col;
            smartPartInfo.GridRow = row;
            smartPartInfo.SmartPartId = smartPartId.ToString(CultureInfo.CurrentCulture);  // this need to be unique

            // Create SmartPart Parameters  [optional]
            IList<SmartPartParameter> parameters = new List<SmartPartParameter>();
            SmartPartParameter parameter = new SmartPartParameter("PayrollExport", payrollExport);

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


        /// <summary>
        /// Gets the level of the node clicked in the tree
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-05-19
        /// </remarks>
        private void SetNodeFromPosition(MouseEventArgs e)
        {
            TreeNode tn = treeViewMain.GetNodeAt(e.X, e.Y);
            _lastNode = tn;
            CheckLastNode();
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
            using (PayrollExportWizardPages pewp = new PayrollExportWizardPages(_repositoryFactory,_unitOfWorkFactory))
            {
				pewp.Initialize(PropertyPagesHelper.GetPayrollExportPages(true, _componentContext), new LazyLoadingManagerWrapper());
                using (Wizard wizard = new Wizard(pewp))
                {
                    pewp.Saved += pewp_Saved;

                    result = wizard.ShowDialog(this);
                    aggregateRootObject = pewp.AggregateRootObject;
                }
            }
            if (result == DialogResult.OK)
            {
                RunExport(aggregateRootObject);
            }
        }

        void pewp_Saved(object sender, AfterSavedEventArgs e)
        {
            AbstractPropertyPages<IPayrollExport> pepp = (AbstractPropertyPages<IPayrollExport>)sender;
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
            SetNodeFromPosition(e);
            //note i undo the fake paint
            if (treeViewMain.Nodes.Count > 0)
            {
                if (treeViewMain.Nodes[0].BackColor != ColorHelper.StandardTreeBackgroundColor()) UnFakeClickColor();
            }
        }

        private void toolStripMenuItemProperties_Click(object sender, EventArgs e)
        {
            CheckLastNode();
            if (_lastNode == null)
                return;
            
            IPayrollExport payrollExport = (IPayrollExport)_lastNode.Tag;
            using (PayrollExportPropertiesPages pepp = new PayrollExportPropertiesPages(payrollExport,_repositoryFactory,_unitOfWorkFactory))
            {
				pepp.Initialize(PropertyPagesHelper.GetPayrollExportPages(false, _componentContext), new LazyLoadingManagerWrapper());
                using (PropertiesPages pp = new PropertiesPages(pepp))
                {
                    pp.ShowDialog(this);
                }

            }
        }

        private void CheckLastNode()
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
            CheckLastNode();
            if(_lastNode == null)
                return;
            
            IPayrollExport payrollExport = (IPayrollExport)_lastNode.Tag;
            string questionString = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.QuestionDelete, "\"", payrollExport.Name);
            if (ViewBase.ShowYesNoMessage(this, questionString, UserTexts.Resources.Delete) == DialogResult.Yes)
            {
                removePayrollExport(payrollExport);
            }
        }

        private void removePayrollExport(IPayrollExport payrollExport)
        {
            IEnumerable<IRootChangeInfo> changes;
            using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                IPayrollExportRepository payrollExportRepository = _repositoryFactory.CreatePayrollExportRepository(uow);
                payrollExport = payrollExportRepository.Load(payrollExport.Id.Value);
                payrollExportRepository.Remove(payrollExport);
                changes = uow.PersistAll();
            }
            Main.EntityEventAggregator.TriggerEntitiesNeedRefresh(ParentForm, changes);
        }

        private void treeViewMain_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            _lastNode = e.Node;

            IPayrollExport payrollExport = _lastNode.Tag as IPayrollExport;
            if (payrollExport != null)
            {
                //with no item selected these are not viable
                toolStripMenuItemRunExport.Enabled = true;
                toolStripMenuItemContextRunExport.Enabled = true;
                SmartPartEnvironment.SmartPartWorkspace.GridSize = GridSizeType.TwoByTwo;
                LoadSmartPart(payrollExport.Id.Value, 1, UserTexts.Resources.PayrollExport, ClassPrefix + PayrollExports, 0, 0);
            }
        
        }

        /// <summary>
        /// Run the selected export.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2009-03-27
        /// </remarks>
        private void toolStripMenuItemRunExport_Click(object sender, EventArgs e)
        {
            RunExport((IPayrollExport)_lastNode.Tag);
        }
       private void toolStripMenuItemContextRunExport_Click(object sender, EventArgs e)
        {
            RunExport((IPayrollExport)_lastNode.Tag);
            
        }
        private void RunExport(IPayrollExport payrollExport)
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
