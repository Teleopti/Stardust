using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Common.PropertyPageAndWizard;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.Forecasting.Forms.ExportPages;
using Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Forecasting;
using Teleopti.Ccc.WinCode.Forecasting.ExportPages;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast;
using Teleopti.Ccc.WinCode.Forecasting.SkillPages;
using Teleopti.Ccc.WinCode.Forecasting.WorkloadPages;
using Teleopti.Common.UI.SmartPartControls.SmartParts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
	public partial class ForecasterNavigator : AbstractNavigator
	{
		private TreeNode _lastActionNode;
		private TreeNode _lastContextMenuNode;
		private readonly PortalSettings _portalSettings;
		private readonly IQuickForecastViewFactory _quickForecastViewFactory;
	    private readonly IJobHistoryViewFactory _jobHistoryViewFactory;
	    private readonly IImportForecastViewFactory _importForecastViewFactory;
	    private readonly ISendCommandToSdk _sendCommandToSdk;
	    private readonly IRepositoryFactory _repositoryFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private const string assembly = "Teleopti.Ccc.SmartParts";
		private const string classPrefix = "Teleopti.Ccc.SmartParts.Forecasting";
		private const string detailed = ".DetailedSmartPart";
		private const string validation = ".ValidationSmartPart";
		private const string budget = ".BudgetsSmartPart";
		private const string template = ".TemplateSmartPart";
        private readonly IGracefulDataSourceExceptionHandler _dataSourceExceptionHandler = new GracefulDataSourceExceptionHandler();

		public ForecasterNavigator()
		{
			InitializeComponent();
			treeViewSkills.Sorted = true;
			if (!DesignMode)
			{
				SetTexts();
				setColors();

				EntityEventAggregator.EntitiesNeedsRefresh += entitiesNeedsRefresh;
			}

			setVisibility();
		}

	    private void setVisibility()
	    {
	        toolStripMenuItemQuickForecast.Visible =
	            TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(
	                DefinedRaptorApplicationFunctionPaths.UnderConstruction);
	        toolStripMenuItemCopyTo.Visible =
	            TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(
	                DefinedRaptorApplicationFunctionPaths.UnderConstruction);
	        toolStripMenuItemActionSkillImportForecast.Visible =
	            TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(
	                DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile);
	        toolStripMenuItemSkillsImportForecast.Visible =
	            TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(
	                DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile);
	    }

	    public ForecasterNavigator(PortalSettings portalSettings, 
            IRepositoryFactory repositoryFactory, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            IQuickForecastViewFactory quickForecastViewFactory, 
            IJobHistoryViewFactory jobHistoryViewFactory,
            IImportForecastViewFactory importForecastViewFactory,
            ISendCommandToSdk sendCommandToSdk)
            : this()
        {
            _portalSettings = portalSettings;
            _quickForecastViewFactory = quickForecastViewFactory;
            _jobHistoryViewFactory = jobHistoryViewFactory;
            _importForecastViewFactory = importForecastViewFactory;
            _sendCommandToSdk = sendCommandToSdk;
            _repositoryFactory = repositoryFactory;
            _unitOfWorkFactory = unitOfWorkFactory;
            splitContainer1.SplitterDistance = splitContainer1.Height - _portalSettings.ForecasterActionPaneHeight;
        }

	    private void setColors()
		{
			BackColor = ColorHelper.StandardTreeBackgroundColor();
			treeViewSkills.BackColor = ColorHelper.StandardTreeBackgroundColor();
			contextMenuStripQueues.BackColor = ColorHelper.StandardContextMenuColor();
			contextMenuStripSkills.BackColor = ColorHelper.StandardContextMenuColor();
			contextMenuStripSkillTypes.BackColor = ColorHelper.StandardContextMenuColor();
			contextMenuStripWorkloads.BackColor = ColorHelper.StandardContextMenuColor();
			splitContainer1.BackColor = ColorHelper.PortalTreeViewSeparator();
			splitContainer1.Panel2.BackColor = ColorHelper.StandardTreeBackgroundColor();
		   
		}

		#region Event handling when entities are updated

		private void entitiesNeedsRefresh(object sender, EntitiesUpdatedEventArgs e)
		{
			using(IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				if (typeof (IWorkload).IsAssignableFrom(e.EntityType))
				{
					foreach (Guid guid in e.UpdatedIds)
					{
						TreeNode[] foundNodes = treeViewSkills.Nodes.Find(guid.ToString(), true);
						bool isWorkloadSelected = false;
						if (foundNodes.Length > 0)
						{
							isWorkloadSelected = foundNodes[0].IsSelected;
							foundNodes[0].Remove();
						}

						IWorkload workload = _repositoryFactory.CreateWorkloadRepository(uow).Get(guid);
						var dTag = workload as IDeleteTag;
						if (workload == null) continue;
						if(dTag != null && dTag.IsDeleted)
						{
							foundNodes = treeViewSkills.Nodes.Find(workload.Skill.Id.ToString(), true);
							if(foundNodes.Length>0) foundNodes[0].Tag = workload.Skill;
							//Hmm, these need to be initialized, here
							LazyLoadingManager.Initialize(workload.Skill.WorkloadCollection);
							LazyLoadingManager.Initialize(workload.Skill.SkillType.ForecastSource);
							continue;
						}

						TreeNode workloadNode = getWorkLoadNode(workload);
						LazyLoadingManager.Initialize(workload.Skill.WorkloadCollection);
						foundNodes = treeViewSkills.Nodes.Find(workload.Skill.Id.ToString(), true);
						if (foundNodes.Length > 0)
						{
							reloadSkillFromNode(foundNodes[0],uow, false);
							reloadQueueSourceNodes(workloadNode);
							foundNodes[0].Nodes.Add(workloadNode);
							workloadNode.EnsureVisible();
						}
						if (isWorkloadSelected)
							treeViewSkills.SelectedNode = workloadNode;
					}
				}
				else if (typeof (ISkill).IsAssignableFrom(e.EntityType))
				{
					if (e.UpdatedIds.Count() > 3)
					{
						//Reload all instead
						loadSkillsTree(treeViewSkills.Nodes, uow);
					}
					else
					{
						foreach (Guid guid in e.UpdatedIds)
						{
							TreeNode[] foundNodes = treeViewSkills.Nodes.Find(guid.ToString(), true);
							bool isSkillSelected = false;
							if (foundNodes.Length > 0)
							{
								isSkillSelected = foundNodes[0].IsSelected;
								foundNodes[0].Remove();
							}

							ISkill skill = _repositoryFactory.CreateSkillRepository(uow).Get(guid);
							var dTag = skill as IDeleteTag;
							if (skill == null || (dTag != null && dTag.IsDeleted) || skill is IChildSkill) continue;

							TreeNode skillNode = GetSkillNode(skill);
							foundNodes = treeViewSkills.Nodes.Find(skill.SkillType.Id.ToString(), false);
							if (foundNodes.Length > 0)
							{
								reloadWorkloadNodes(skillNode);
								foundNodes[0].Nodes.Add(skillNode);
								skillNode.EnsureVisible();
							}
							if (isSkillSelected)
								treeViewSkills.SelectedNode = skillNode;
						}
					}
				}
			}
		}

		private static void reloadSkillFromNode(TreeNode node,IUnitOfWork uow, bool force)
		{
			var skill = node.Tag as ISkill;
			if (skill == null) return;
			if (force || !LazyLoadingManager.IsInitialized(skill.WorkloadCollection))
			{
				uow.Reassociate(skill);
				uow.Refresh(skill);
				LazyLoadingManager.Initialize(skill.WorkloadCollection);
			}
		}

		#endregion

		//Dont know about this but, this might need to be standardized
		//But it works for now, we need to go on with the Sprint
		#region uow_Stuff 

		private ICollection<ISkillType> loadSkillTypeCollection(IUnitOfWork uow)
		{
			ISkillTypeRepository skillTypeRep = _repositoryFactory.CreateSkillTypeRepository(uow);
			ICollection<ISkillType> skillTypes = skillTypeRep.FindAll();
			return skillTypes;
		}

		private ICollection<ISkill> loadSkillCollection(IUnitOfWork uow)
		{
			ISkillRepository skillRep = _repositoryFactory.CreateSkillRepository(uow);
			ICollection<ISkill> skills = skillRep.FindAllWithWorkloadAndQueues();
			skills = skills.Except(skills.OfType<IChildSkill>().Cast<ISkill>()).ToList();
			return skills;
		}

		private void toolStripMenuItemActionSkillDelete_Click(object sender, EventArgs e)
		{
			var skill = (ISkill) _lastActionNode.Tag;
			removeSkill(skill);
		}

		private void removeSkill(ISkill skill)
		{
			string questionString = string.Format(CultureInfo.CurrentCulture, Resources.QuestionDeleteTheSkillTwoParameters, "\"", skill.Name);
			if (ViewBase.ShowYesNoMessage(questionString, Resources.Delete) == DialogResult.Yes)
			{
				IEnumerable<IRootChangeInfo> changes;
				using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
				{
					ISkillRepository skillRep = _repositoryFactory.CreateSkillRepository(uow);
					IWorkloadRepository workloadRep = _repositoryFactory.CreateWorkloadRepository(uow);
					skill = skillRep.Get(skill.Id.Value); //To solve Bug when removing MultisiteSkill, Get instead of Load as Load didn't enable the inheritance --> IMultisiteSkill
					
					IMultisiteSkill multisiteSkill = skill as IMultisiteSkill;
					if (multisiteSkill != null)
					{
						foreach (IChildSkill childSkill in multisiteSkill.ChildSkills)
						{
							skillRep.Remove(childSkill);
						}
					}

					// Remove skill´s workloads
					foreach (IWorkload workload in skill.WorkloadCollection)
					{
						workloadRep.Remove(workload);
					}

					skillRep.Remove(skill);
					changes = uow.PersistAll();
				}
				EntityEventAggregator.TriggerEntitiesNeedRefresh(ParentForm, changes);
			}
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Win.Common.ViewBase.ShowInformationMessage(System.Windows.Forms.IWin32Window,System.String,System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Win.Common.ViewBase.ShowInformationMessage(System.String,System.String)")]
        private void removeWorkload(IWorkload workload)
		{
			string questionString = string.Format(CultureInfo.CurrentCulture, Resources.QuestionDeleteTheWorkloadTwoParameters, "\"", workload.Name);

			if (ViewBase.ShowYesNoMessage(questionString, Resources.Delete) == DialogResult.Yes)
			{

                try
                {
                    IEnumerable<IRootChangeInfo> changes;
                    using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                    {
                        uow.Reassociate(workload);
                        uow.Reassociate(workload.Skill);
                        _repositoryFactory.CreateWorkloadRepository(uow).Remove(workload);
                        changes = uow.PersistAll();
                    }
                    EntityEventAggregator.TriggerEntitiesNeedRefresh(ParentForm, changes);
                }
                catch (OptimisticLockException)
                {
                    string templateMessage = string.Concat(UserTexts.Resources.SomeoneElseHaveChanged, " {0}{1}{0} ", UserTexts.Resources.YourChangesWillBeDiscardedReloading);
                    string message = string.Format(CultureInfo.CurrentCulture, templateMessage, "\"", workload.Name);
                    
                    ViewBase.ShowInformationMessage(this, message, UserTexts.Resources.SaveError);
                    EntityEventAggregator.TriggerEntitiesNeedRefresh(ParentForm, new List<IAggregateRoot> {workload});
                }
			}
		}
		
		#endregion

		private void loadSkillsTree(TreeNodeCollection skillNodes,IUnitOfWork uow)
		{
			skillNodes.Clear();

			TreeNode skillNode;

			foreach (ISkillType type in loadSkillTypeCollection(uow))// Program.CommonState.SkillTypes)
			{
				skillNode = new TreeNode
								{
									Name = type.Id.ToString(),
									Text = Resources.ResourceManager.GetString(type.Description.Name),
									ImageKey = type.ForecastSource.ToString()
								};
				skillNode.SelectedImageKey = skillNode.ImageKey;
				skillNode.Tag = type;
				skillNodes.Add(skillNode);
			}

			ICollection<ISkill> skills = loadSkillCollection(uow);
			foreach (ISkill aSkill in skills) //Program.CommonState.Skills)
			{
				var dTag = aSkill as IDeleteTag;
				if (dTag != null && dTag.IsDeleted) continue;
				skillNode = GetSkillNode(aSkill);
				skillNodes[aSkill.SkillType.Id.ToString()].Nodes.Add(skillNode);
				reloadWorkloadNodes(skillNode);
			}
		}

		private static void reloadWorkloadNodes(TreeNode skillNode)
		{
			skillNode.Nodes.Clear();
			var aSkill = (ISkill) skillNode.Tag;
			
			foreach (IWorkload workload in aSkill.WorkloadCollection)
			{
				if (((IDeleteTag)workload).IsDeleted) continue;

				TreeNode workLoadNode = getWorkLoadNode(workload);
				skillNode.Nodes.Add(workLoadNode);
				reloadQueueSourceNodes(workLoadNode);
				if (!LazyLoadingManager.IsInitialized(workload.QueueSourceCollection))
				LazyLoadingManager.Initialize(workload.QueueSourceCollection);
			}
		}

		private static TreeNode getWorkLoadNode(IWorkload workload)
		{
			var workLoadNode = new TreeNode(workload.Name)
								{
									Name = workload.Id.ToString(),
									ImageIndex = 7,
									SelectedImageIndex = 7,
									Tag = workload
								};
			return workLoadNode;
		}

		private static void reloadQueueSourceNodes(TreeNode workLoadNode)
		{
			workLoadNode.Nodes.Clear();
			var source = (IWorkload)workLoadNode.Tag;
			TreeNode ctiQueueSourceNode;
			foreach (IQueueSource queueSource in source.QueueSourceCollection)
			{
				ctiQueueSourceNode = new TreeNode(queueSource.Name) {Name = queueSource.Id.ToString()};
				workLoadNode.Nodes.Add(ctiQueueSourceNode);
				ctiQueueSourceNode.Tag = queueSource;
			}
		}

		private static TreeNode GetSkillNode(ISkill aSkill)
		{
			var skillNode = new TreeNode(aSkill.Name)
								{
									Name = aSkill.Id.ToString(),
									ImageIndex = 6,
									SelectedImageIndex = 6,
									Tag = aSkill
								};
			return skillNode;
		}

		private void fakeNodeClickColor()
		{
			treeViewSkills.Nodes[treeViewSkills.Nodes[0].Index].BackColor = ColorHelper.SelectedTreeViewNodeBackColor();
			treeViewSkills.Nodes[treeViewSkills.Nodes[0].Index].ForeColor = ColorHelper.SelectedTreeViewNodeForeColor();

		}

		private void unFakeClickColor()
		{
			treeViewSkills.Nodes[treeViewSkills.Nodes[0].Index].BackColor = ColorHelper.StandardTreeBackgroundColor();
			treeViewSkills.Nodes[treeViewSkills.Nodes[0].Index].ForeColor = Color.Black;
		}

		private void toolStripMenuItemActionSkillTypeNewSkill_Click(object sender, EventArgs e)
		{
			queryPersonForNewSkill(_lastActionNode);
		}

		private void toolStripMenuItemSkillsNew_Click(object sender, EventArgs e)
		{
			queryPersonForNewSkill(_lastContextMenuNode);
		}

		private void toolStripMenuItemActionSkillNewSkill_Click(object sender, EventArgs e)
		{
			queryPersonForNewSkill(_lastActionNode);
		}

		private void toolStripMenuItemSkillTypesSkillNew_Click(object sender, EventArgs e)
		{
			queryPersonForNewSkill(_lastContextMenuNode);
		}

		private void queryPersonForNewSkill(TreeNode node)
		{
			ISkillType st = getSkillType(node);
			using(var swp = new SkillWizardPages(st,_repositoryFactory,_unitOfWorkFactory))
			{
				swp.Initialize(PropertyPagesHelper.GetSkillPages(true, swp, st), new LazyLoadingManagerWrapper());
				using (var wizard = new Wizard(swp))
				{
					swp.Saved += swp_AfterSave;
					wizard.ShowDialog(this);
				}
			}
		}
        
		private ISkillType getSkillType(TreeNode node)
		{
			if (node == null)
			{
				node = treeViewSkills.SelectedNode ?? treeViewSkills.Nodes[0];
			}

			node = findAncestorNodeOfType(node, typeof(ISkillType));
			return (ISkillType)node.Tag;
		}

		private void swp_AfterSave(object sender, AfterSavedEventArgs e)
		{
			var skillPropertyPages = (AbstractPropertyPages<ISkill>)sender;
			skillPropertyPages.LoadAggregateRootWorkingCopy();
			ISkill skill = skillPropertyPages.AggregateRootObject;
			InitializeWorkloadCollectionForSkill(skill, skillPropertyPages);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
		private void InitializeWorkloadCollectionForSkill(ISkill skill, IAbstractPropertyPages skillPropertyPages)
		{
			if (skill.WorkloadCollection.Count() == 0)
			{
				if (MessageBox.Show(string.Concat(Resources.QuestionWouldYouLikeToCreateWorkloadQuestionMark, "  "),
					Resources.Skill,
					MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2,
					(RightToLeft == RightToLeft.Yes) ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0) == DialogResult.Yes)
				{
					//Hide the other window first
					if (skillPropertyPages!=null) skillPropertyPages.Owner.Visible = false;

					DialogResult result;
					do
					{
						using (var wwp = new WorkloadWizardPages(skill,_repositoryFactory,_unitOfWorkFactory))
						{
							wwp.Initialize(PropertyPagesHelper.GetWorkloadPages(), new LazyLoadingManagerWrapper());
							using (var wizard = new Wizard(wwp))
							{
								result = wizard.ShowDialog(this);
							}
						}
					} while (
						result != DialogResult.Cancel &&
						MessageBox.Show(
							string.Concat(Resources.QuestionCreateAnotherWorkload, "  "),
							Resources.SkillWizard,
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question,
							MessageBoxDefaultButton.Button2,
						(RightToLeft == RightToLeft.Yes) ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0)
						== DialogResult.Yes);
				}
				TreeNode[] foundNodes = treeViewSkills.Nodes.Find(skill.Id.Value.ToString(), true);
				if (foundNodes.Length > 0) foundNodes[0].Tag = skill; //Just to make sure you can open forecast afterwards
			}
		}

		private static TreeNode findAncestorNodeOfType(TreeNode startingNode, Type typeOfObject)
		{
			while (!typeOfObject.IsInstanceOfType(startingNode.Tag))
			{
				startingNode = startingNode.Parent;
			}
			return startingNode;
		}

		#region treeViewSkills

		private static void loadSmartPart(Guid skill, int smartPartId,string smartPartHeaderTitle, 
										 string smartPartName, int row, int col)
		{
			var smartPartInfo = new SmartPartInformation
									{
										ContainingAssembly = assembly,
										SmartPartName = smartPartName,
										SmartPartHeaderTitle = smartPartHeaderTitle,
										GridColumn = col,
										GridRow = row,
										SmartPartId = smartPartId.ToString(CultureInfo.CurrentCulture)
									};

			// Create SmartPart Parameters  [optional]
			IList<SmartPartParameter> parameters = new List<SmartPartParameter>();
			var parameter = new SmartPartParameter("Skill", skill);

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

		private void treeViewSkills_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			_lastActionNode = e.Node;
			
			//to display smartpart
			if (typeof(ISkill).IsInstanceOfType(e.Node.Tag))
			{
				var skill = _lastActionNode.Tag as ISkill;
				if (skill != null)
				{
					SmartPartEnvironment.SmartPartWorkspace.GridSize = GridSizeType.TwoByTwo;
					loadSmartPart(skill.Id.Value, 1, Resources.SkillValidationSmartPart, classPrefix + validation, 0, 0);
					loadSmartPart(skill.Id.Value, 2, Resources.DetailedForecastSmartPart, classPrefix + detailed, 0, 1);
					loadSmartPart(skill.Id.Value, 3, Resources.BudgetForecasting, classPrefix + budget, 1, 0);
					loadSmartPart(skill.Id.Value, 4, Resources.TemplatesSmartpart, classPrefix + template, 1, 1);
				}
			}
			setMenu(_lastActionNode.Tag as IEntity);
		}

		private void treeViewSkills_MouseDown(object sender, MouseEventArgs e)
		{
			if (treeViewSkills.Nodes.Count == 0) return;
			if (e.Button == MouseButtons.Right)
			{
				_lastContextMenuNode = getNodeFromPosition(e);
				if (_lastContextMenuNode == null) return;

				setContextMenu(_lastContextMenuNode.Tag as IEntity);
			}
			//note i undo the fake paint
			if (treeViewSkills.Nodes[0].BackColor != ColorHelper.StandardTreeBackgroundColor()) unFakeClickColor();
			if (getNodeFromPosition(e) != null)
				treeViewSkills.SelectedNode = getNodeFromPosition(e);
		}

		private void setMenu(IEntity nodeEntity)
		{
			if (nodeEntity is ISkillType)
			{
				toggleToolstrips(toolStripSkillTypes);
			}
			else if (nodeEntity is ISkill)
			{
				workloadOnSkillCheck();
				multisiteSkillOnSkillCheck();
				toggleToolstrips(toolStripSkills);
			}
			else if (nodeEntity is IWorkload)
			{
				toggleToolstrips(toolStripWorkload);
			}
			else if (nodeEntity is IQueueSource)
			{
				toggleToolstrips(toolStripQueues);
			}
			else
			{
				toggleToolstrips(toolStripSkillTypes);
			}
		}

		private void setContextMenu(IEntity nodeEntity)
		{
			if (nodeEntity is ISkillType)
			{
				treeViewSkills.ContextMenuStrip = contextMenuStripSkillTypes;
				if (nodeEntity is ISkillTypePhone)
				{
					toggleExportMenu();
				}
			}
			else if (nodeEntity is ISkill)
			{
				workloadOnSkillCheck();
				multisiteSkillOnSkillCheck();
				treeViewSkills.ContextMenuStrip = contextMenuStripSkills;
			}
			else if (nodeEntity is IWorkload)
			{
				treeViewSkills.ContextMenuStrip = contextMenuStripWorkloads;
			}
			else if (nodeEntity is IQueueSource)
			{
				treeViewSkills.ContextMenuStrip = contextMenuStripQueues;
			}
			else
			{
				treeViewSkills.ContextMenuStrip = contextMenuStripSkillTypes;
			}
		}

		private void toggleExportMenu()
		{
			var exportContextItemsVisible = TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ExportForecastToOtherBusinessUnit);
			toolStripMenuItemExport.Visible = exportContextItemsVisible;
			toolStripMenuItemJobHistory.Visible = exportContextItemsVisible;
			toolStripSeparatorExport.Visible = exportContextItemsVisible;
		}

		private TreeNode getNodeFromPosition(MouseEventArgs e)
		{
			return treeViewSkills.GetNodeAt(e.X, e.Y);
		}

		private void toggleToolstrips(ToolStrip showThis)
		{
			toolStripSkillTypes.Visible = false;
			toolStripSkills.Visible = false;
			toolStripWorkload.Visible = false;
			toolStripQueues.Visible = false ;
			showThis.Visible = true;
		}

		private void multisiteSkillOnSkillCheck()
		{
			var multisiteSkill = _lastActionNode.Tag as IMultisiteSkill;
			toolStripButtonManageMultisiteDistributions.Visible = (multisiteSkill != null);
			toolStripMenuItemManageMultisiteDistributions.Visible = (multisiteSkill != null);
		}

		private void workloadOnSkillCheck()
		{
			toolStripMenuCreateForecast.Enabled = (_lastActionNode.Nodes.Count > 0);
		}

		#endregion



		private void toolStripMenuItemActionSkillAddNewWorkload_Click(object sender, EventArgs e)
		{
			//queryPersonForNewSkill(_lastActionNode);
			queryPersonForNewWorkload(_lastActionNode);
		}

		private void toolStripMenuItemWorkloadSkillNew_Click(object sender, EventArgs e)
		{
			queryPersonForNewSkill(_lastContextMenuNode);
		}

		void toolStripMenuItemActionWorkloadNewSkill_Click(object sender, System.EventArgs e)
		{
			queryPersonForNewSkill(_lastActionNode);
		}

		private void toolStripMenuItemWorkloadNew_Click(object sender, EventArgs e)
		{
			queryPersonForNewWorkload(_lastContextMenuNode);
		}

		void toolStripMenuItemActionWorkloadNewWorkload_Click(object sender, EventArgs e)
		{
			queryPersonForNewWorkload(_lastActionNode);
		}

		private void toolStripMenuItemActionQueueSourceNewWorkload_Click(object sender, EventArgs e)
		{
			queryPersonForNewWorkload(_lastActionNode);
		}

		private void queryPersonForNewWorkload(TreeNode node)
		{
			node = findAncestorNodeOfType(node, typeof(ISkill));

			var s = (ISkill)node.Tag;

			using(var wwp = new WorkloadWizardPages(s,_repositoryFactory,_unitOfWorkFactory))
			{
				wwp.Initialize(PropertyPagesHelper.GetWorkloadPages(), new LazyLoadingManagerWrapper());
				using (var wizard = new Wizard(wwp))
				{
					if (wizard.ShowDialog(this) == DialogResult.Cancel)
					{
						if (s.WorkloadCollection.Contains(wwp.AggregateRootObject))
						{
							s.RemoveWorkload(wwp.AggregateRootObject);
						}
					}
				}
			}     
		}


		private void toolStripMenuItemNewWorkload_Click(object sender, EventArgs e)
		{
			queryPersonForNewWorkload(_lastActionNode);
		}

		private void toolStripMenuItemQueueWorkloadNew_Click(object sender, EventArgs e)
		{
			queryPersonForNewWorkload(_lastActionNode);
		}

		private void toolStripMenuItemSkillsDelete_Click(object sender, EventArgs e)
		{
			var skill = (ISkill)_lastContextMenuNode.Tag;
			removeSkill(skill);
		}

		void toolStripMenuItemActionWorkloadDelete_Click(object sender, EventArgs e)
		{
			var workload = (IWorkload) _lastActionNode.Tag;
			removeWorkload(workload);
		}

		private void toolStripMenuItemDeleteWorkload_Click(object sender, EventArgs e)
		{
			var workload = (IWorkload)_lastContextMenuNode.Tag;
			removeWorkload(workload);
		}

		private void toolStripMenuItemRemoveQueue_Click(object sender, EventArgs e)
		{
			var workload = (IWorkload)_lastContextMenuNode.Parent.Tag;
			var queueSource = (IQueueSource)_lastContextMenuNode.Tag;
			removeQueueSource(workload, queueSource);
		}

		private void toolStripMenuItemActionQueueSourceDelete_Click(object sender, EventArgs e)
		{
			var workload = (IWorkload) _lastActionNode.Parent.Tag;
			var queueSource = (IQueueSource) _lastActionNode.Tag;
			removeQueueSource(workload, queueSource);
		}

		private void removeQueueSource(IWorkload workload, IQueueSource queueSource)
		{
			IEnumerable<IRootChangeInfo> changes;
			using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(workload);
				workload.RemoveQueueSource(queueSource);
				changes = uow.PersistAll();
			}
			EntityEventAggregator.TriggerEntitiesNeedRefresh(ParentForm, changes);
		}


		private void toolStripMenuItemWorkloadProperties_Click(object sender, EventArgs e)
		{
			var w = (IWorkload)_lastContextMenuNode.Tag;
			showWorkloadProperties(w);
		}

		void toolStripMenuItemActionWorkloadProperties_Click(object sender, EventArgs e)
		{
			var w = (IWorkload) _lastActionNode.Tag;
			showWorkloadProperties(w);
		}

		private void showWorkloadProperties(IWorkload workload)
		{
			using (var wpp = new WorkloadPropertiesPages(workload,_repositoryFactory,_unitOfWorkFactory))
			{
				wpp.Initialize(PropertyPagesHelper.GetWorkloadPages(), new LazyLoadingManagerWrapper());
				using (var propertiesPages = new PropertiesPages(wpp))
				{
					propertiesPages.ShowDialog(this);
				}
			}
		}


		private void toolStripMenuItemActionSkillProperties_Click(object sender, EventArgs e)
		{
			showSkillProperties();
		}

		private void toolStripMenuItemSkillsProperties_Click(object sender, EventArgs e)
		{
			showSkillProperties();
		}

		private void showSkillProperties()
		{
			var skill = _lastActionNode.Tag as Skill ?? _lastActionNode.Tag as MultisiteSkill;
		   
			if (skill == null) return;

            try
            {
                using (var spp = new SkillPropertiesPages(skill, _repositoryFactory, _unitOfWorkFactory))
                {
                    var pages = PropertyPagesHelper.GetSkillPages(false, spp);
                    if (skill is IMultisiteSkill) PropertyPagesHelper.AddMultisiteSkillPages(pages);
                    spp.Initialize(pages, new LazyLoadingManagerWrapper());
                    using (var propertiesPages = new PropertiesPages(spp))
                    {
                        propertiesPages.ShowDialog(this);
                    }
                }
            }
            catch (DataSourceException exception)
            {

                using (var view = new SimpleExceptionHandlerView(exception,
                                                                    Resources.OpenForecaster,
                                                                    Resources.ServerUnavailable))
                {
                    view.ShowDialog();
                }
                return;
            }           
		}

		private void toolStripMenuCreateForecast_Click(object sender, EventArgs e)
		{
			ISkill skill = null;
			if (_lastActionNode != null)
			{
				skill = _lastActionNode.Tag as ISkill;
			}
			if (skill == null && _lastContextMenuNode != null)
			{
				skill = _lastContextMenuNode.Tag as ISkill;
			}
			if (skill == null)
			{
				IWorkload workload = null;
				if (_lastActionNode != null)
				{
					workload = _lastActionNode.Tag as IWorkload;
				}
				if (workload == null && _lastContextMenuNode != null)
				{
					workload = _lastContextMenuNode.Tag as IWorkload;
				}
				if (workload == null)
					return;
				skill = workload.Skill;
			}
			if (skill.WorkloadCollection.Count() == 0) return;

			OpenWizard(skill);
		}

		private void OpenWizard(ISkill skill)
		{
			using (var openForecast = new OpenScenarioForPeriod(new OpenPeriodForecasterMode()))
			{
				if (openForecast.ShowDialog() != DialogResult.Cancel)
				{
					LogPointOutput.LogInfo("Forecast.LoadAndOpenSkill:openWizard", "Started");
					startForecaster(openForecast.SelectedPeriod, openForecast.Scenario, skill);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void startForecaster(DateOnlyPeriod selectedPeriod, IScenario scenario, ISkill skill)
		{
			Cursor = Cursors.WaitCursor;
			var forecaster = new Forecaster(skill, selectedPeriod, scenario, true);
			forecaster.Show();
			Cursor = Cursors.Default;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void showPrepareWorkload(IWorkload workload)
		{
			Cursor = Cursors.WaitCursor;
			if (workload != null)
			{
				var forecastWorkflow = new ForecastWorkflow(workload);
				forecastWorkflow.Show();
			}
			Cursor = Cursors.Default;
		}

		void toolStripMenuItemActionWorkloadPrepareForecast_Click(object sender, EventArgs e)
		{
			var w = _lastActionNode.Tag as IWorkload;
			showPrepareWorkload(w);
		}

		private void toolStripMenuItemWorkloadPrepareWorkload_Click(object sender, EventArgs e)
		{
			var w = _lastContextMenuNode.Tag as IWorkload;
			showPrepareWorkload(w);
		}

		private void toolStripMenuItemManageDayTemplates_Click(object sender, EventArgs e)
		{
			var skill = _lastActionNode.Tag as ISkill;
			if (skill != null)
			{
				var templateTool = new SkillDayTemplates(skill);
				templateTool.Show(this);
			}
		}

		private void toolStripMenuItemActionSkillNewMultisiteSkill_Click(object sender, EventArgs e)
		{
			ISkillType skillType = getSkillType(_lastActionNode);
			createMultisiteSkill(skillType);
		}

		private void toolStripMenuItemMultisiteSkillNew_Click(object sender, EventArgs e)
		{
			ISkillType skillType = getSkillType(_lastContextMenuNode);
			createMultisiteSkill(skillType);
		}

		private void toolStripMenuItemActionSkillTypeNewMultisiteSkill_Click(object sender, EventArgs e)
		{
			ISkillType skillType = getSkillType(_lastActionNode);
			createMultisiteSkill(skillType);
		}

		private void toolStripMenuItemSkillTypesMultisiteSkillNew_Click(object sender, EventArgs e)
		{
			ISkillType skillType = getSkillType(_lastContextMenuNode);
			createMultisiteSkill(skillType);
		}

		private void createMultisiteSkill(ISkillType skillType)
		{
            var culture = TeleoptiPrincipal.Current.Regional.Culture;
			IMultisiteSkill skill = new MultisiteSkill(
				Resources.LessThanSkillNameGreaterThan,
                string.Format(culture, Resources.SkillCreatedDotParameter0, DateTime.Now),
				Color.FromArgb(0), skillType.DefaultResolution, skillType);
			SkillWizardPages.SetSkillDefaultSettings(skill);

			DialogResult result;
			using (var swp = new SkillWizardPages(skill,_repositoryFactory,_unitOfWorkFactory))
			{
				//swp.Initialize(PropertyPagesHelper.GetSkillPages(true, swp));
				swp.Initialize(PropertyPagesHelper.GetSkillPages(true, swp, skillType), new LazyLoadingManagerWrapper());
				using (var wizard = new Wizard(swp))
				{
					swp.Saved += multisiteSkillSaved;
					result = wizard.ShowDialog(this);
				}
			}
			if (result == DialogResult.Cancel) return;

			//Run the ordinary workload flow
			InitializeWorkloadCollectionForSkill(skill, null);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
		private void multisiteSkillSaved(object sender, AfterSavedEventArgs e)
		{
			using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				IMultisiteSkill skill = new MultisiteSkillRepository(uow).Get(e.SavedAggregateRoot.Id.Value);
				if (skill.ChildSkills.Count == 0)
				{
					if (MessageBox.Show(
							string.Concat(Resources.QuestionCreateSubSkill, "  "),
							Resources.MultisiteSkillWizard,
							MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2,
							(RightToLeft == RightToLeft.Yes)
								? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
								: 0) == DialogResult.Yes)
					{
						//Hide the other window first
						var skillPropertyPages = (AbstractPropertyPages<ISkill>) sender;
						skillPropertyPages.Owner.Visible = false;

						int numberOfSkillsAdded = 0;
						DialogResult result;
						do
						{
							numberOfSkillsAdded++;
							IChildSkill childSkill = new ChildSkill(
								Resources.LessThanSkillNameGreaterThan,
								string.Format(CultureInfo.CurrentUICulture,
								              Resources.SkillCreatedDotParameter0, DateTime.Now),
								skill.DisplayColor,
								skill.DefaultResolution,
								skill.SkillType)
							                         	{
							                         		MidnightBreakOffset = skill.MidnightBreakOffset
							                         	};

							SkillWizardPages.SetSkillDefaultSettings(childSkill);
							childSkill.TimeZone = skill.TimeZone;
							childSkill.Activity = skill.Activity;
							childSkill.SetParentSkill(skill);
							using (var swp = new SkillWizardPages(childSkill, _repositoryFactory, _unitOfWorkFactory))
							{
								swp.Initialize(PropertyPagesHelper.GetSkillPages(false, swp, true), new LazyLoadingManagerWrapper());
								using (var wizard = new Wizard(swp))
								{
									result = wizard.ShowDialog(this);
								}
							}

							if (result == DialogResult.Cancel)
							{
								numberOfSkillsAdded--;
							}
						} while (
							result != DialogResult.Cancel &&
							MessageBox.Show(
								string.Concat(Resources.QuestionCreateAnotherSubSkill, "  "),
								Resources.MultisiteSkillWizard,
								MessageBoxButtons.YesNo,
								MessageBoxIcon.Question,
								MessageBoxDefaultButton.Button2,
								(RightToLeft == RightToLeft.Yes)
									? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
									: 0)
							== DialogResult.Yes);

						if (numberOfSkillsAdded > 0)
						{
							//Show wizard page for default distributions
							using (var spp = new MultisiteSkillPropertiesPages(skill, _repositoryFactory, _unitOfWorkFactory))
							{
								spp.Initialize(PropertyPagesHelper.GetMultisiteSkillDistributionPages(), new LazyLoadingManagerWrapper());
								using (var properties = new PropertiesPages(spp))
								{
									properties.ShowDialog(this);
								}
							}
						}
					}
				}
			}
		}
		
		private void toolStripMenuItemManageDistribution_Click(object sender, EventArgs e)
		{
			var skill = _lastActionNode.Tag as IMultisiteSkill;
			if (skill != null)
			{
				var templateTool = new MultisiteDayTemplates(skill);
				templateTool.Show(this);
			}
		}

		private void toolStripMenuItemQuickForecast_Click(object sender, EventArgs e)
		{
			var view = _quickForecastViewFactory.Create();
			((Control) view).Show();
		}

		private void toolStripMenuItemCopyTo_Click(object sender, EventArgs e)
		{
			var workload = _lastActionNode.Tag as IWorkload;

			var model = new CopyToSkillModel(workload);
			using(var view = new CopyToSkillView(model))
			{
				view.ShowDialog();
			}
		}

		public override void RefreshNavigator()
		{
			checkIfTreeIsLoaded();
		}

		private void checkIfTreeIsLoaded()
		{
			if (treeViewSkills.Nodes.Count > 0)
				return;
			try
			{
				using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
				{
					loadSkillsTree(treeViewSkills.Nodes, uow);
				}
			}
			catch (DataSourceException exception)
			{

				using (var view = new SimpleExceptionHandlerView(exception,
																	Resources.OpenForecaster,
																	Resources.ServerUnavailable))
				{
					view.ShowDialog();
				}
				return;
			}


			treeViewSkills.SelectedNode = treeViewSkills.Nodes[0];

			fakeNodeClickColor();

			
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void toolStripMenuItemExport_Click(object sender, EventArgs e)
		{
			using (var model = new ExportMultisiteSkillToSkillCommandModel())
			{
                var settingProvider = new ExportAcrossBusinessUnitsSettingsProvider(_unitOfWorkFactory, _repositoryFactory);
			    _dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(
                    () => initializeExportAcrossBusinessUnitsWizard(model, settingProvider));
                using (var pages = new ExportAcrossBusinessUnitsWizardPages(model, settingProvider))
				{
					pages.Initialize(PropertyPagesHelper.GetExportAcrossBusinessUnitsPages());
					using (var wizard = new WizardNoRoot<ExportMultisiteSkillToSkillCommandModel>(pages))
					{
						if (wizard.ShowDialog(this) == DialogResult.OK)
						{
							var dto = model.TransformToDto();
                            try
                            {
                                pages.SaveSettings();
                                var statusDialog =
                                    new JobStatusView(new JobStatusModel { JobStatusId = Guid.NewGuid()});
                                statusDialog.Show(this);
                                statusDialog.SetJobStatusId(_sendCommandToSdk.ExecuteCommand(dto).AffectedId.GetValueOrDefault());
                            }
                            catch (OptimisticLockException)
                            {
                                ViewBase.ShowErrorMessage(string.Concat(Resources.SomeoneChangedTheSameDataBeforeYouDot,
                                                                        Resources.YourChangesWillBeDiscarded), Resources.PleaseTryAgainLater);
                            }
                            catch (DataSourceException exception)
                            {
                                _dataSourceExceptionHandler.DataSourceExceptionOccurred(exception);
                            }
						}
					}
				}
			}
		}

        private static void initializeExportAcrossBusinessUnitsWizard(ExportMultisiteSkillToSkillCommandModel model, IExportAcrossBusinessUnitsSettingsProvider settingProvider)
        {
            var savedSettings = settingProvider.ExportAcrossBusinessUnitsSettings;
            settingProvider.TransformSerilizableToSelectionModels().ForEach(
                model.MultisiteSkillSelectionModels.Add);
            if (!savedSettings.Period.Equals(new DateOnlyPeriod()))
                model.Period = new DateOnlyPeriodDto(savedSettings.Period);
        }

	    private void toolStripMenuItemJobHistory_Click(object sender, EventArgs e)
	    {
	        _dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() => _jobHistoryViewFactory.Create());
	    }

        private void importForecast(TreeNode node)
        {
            node = findAncestorNodeOfType(node, typeof(ISkill));
            var skill = (ISkill)node.Tag;
            if (skill.WorkloadCollection.Count() == 0)
            {
                ViewBase.ShowWarningMessage("No workload available.", Resources.ImportError);
                return;
            }
            _importForecastViewFactory.Create(skill);
        }

        private void toolStripMenuItemSkillsImportForecast_Click(object sender, EventArgs e)
        {
            importForecast(_lastContextMenuNode);
        }

        private void toolStripMenuItemActionSkillImportForecast_Click(object sender, EventArgs e)
        {
            importForecast(_lastActionNode);
        }
	}
}
