﻿using System;
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
	            PrincipalAuthorization.Instance().IsPermitted(
	                DefinedRaptorApplicationFunctionPaths.UnderConstruction);
	        toolStripMenuItemCopyTo.Visible =
	            PrincipalAuthorization.Instance().IsPermitted(
	                DefinedRaptorApplicationFunctionPaths.UnderConstruction);
	        toolStripMenuItemActionSkillImportForecast.Visible =
	            PrincipalAuthorization.Instance().IsPermitted(
	                DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile);
	        toolStripMenuItemSkillsImportForecast.Visible =
	            PrincipalAuthorization.Instance().IsPermitted(
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
					var repository = _repositoryFactory.CreateWorkloadRepository(uow);
					foreach (Guid guid in e.UpdatedIds)
					{
						TreeNode[] foundNodes = treeViewSkills.Nodes.Find(guid.ToString(), true);
						bool isWorkloadSelected = false;
						if (foundNodes.Length > 0)
						{
							isWorkloadSelected = foundNodes[0].IsSelected;
							foundNodes[0].Remove();
						}

						IWorkload workload = repository.Get(guid);
						if (workload == null) continue;

						var model = CreateWorkloadModel(workload);
						if(model.IsDeleted)
						{
							foundNodes = treeViewSkills.Nodes.Find(workload.Skill.Id.ToString(), true);
							if(foundNodes.Length>0) foundNodes[0].Tag = CreateSkillModel(workload.Skill);

							continue;
						}

						TreeNode workloadNode = getWorkLoadNode(model);
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
						var repository = _repositoryFactory.CreateSkillRepository(uow);
						foreach (Guid guid in e.UpdatedIds)
						{
							TreeNode[] foundNodes = treeViewSkills.Nodes.Find(guid.ToString(), true);
							bool isSkillSelected = false;
							if (foundNodes.Length > 0)
							{
								isSkillSelected = foundNodes[0].IsSelected;
								foundNodes[0].Remove();
							}

							ISkill skill = repository.Get(guid);
							if (skill==null) continue;

							var model = CreateSkillModel(skill);
							if (model.IsDeleted || model.IsChild) continue;

							TreeNode skillNode = GetSkillNode(model);
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

		private void reloadSkillFromNode(TreeNode node,IUnitOfWork uow, bool force)
		{
			var skill = node.Tag as SkillModel;
			if (skill == null) return;
			if (force || !skill.WorkloadModels.Any())
			{
				var repository = _repositoryFactory.CreateSkillRepository(uow);
				var updatedSkill = repository.Get(skill.Id);
				node.Tag = CreateSkillModel(updatedSkill);
			}
		}

		#endregion

		//Dont know about this but, this might need to be standardized
		//But it works for now, we need to go on with the Sprint
		#region uow_Stuff 

		private ICollection<SkillTypeModel> loadSkillTypeCollection(IUnitOfWork uow)
		{
			ISkillTypeRepository skillTypeRep = _repositoryFactory.CreateSkillTypeRepository(uow);
			ICollection<ISkillType> skillTypes = skillTypeRep.FindAll();
			return skillTypes.Select(s => new SkillTypeModel{Id = s.Id.GetValueOrDefault(),Name = s.Description.Name,ForecastSource = s.ForecastSource.ToString(),IsSkillTypePhone = s is ISkillTypePhone}).ToList();
		}

		private ICollection<SkillModel> loadSkillCollection(IUnitOfWork uow)
		{
			ISkillRepository skillRep = _repositoryFactory.CreateSkillRepository(uow);
			ICollection<ISkill> skills = skillRep.FindAllWithWorkloadAndQueues();
			skills = skills.Except(skills.OfType<IChildSkill>().Cast<ISkill>()).ToList();
			return skills.Select(CreateSkillModel).ToList();
		}

		private static SkillModel CreateSkillModel(ISkill skill)
		{
			return new SkillModel
			       	{
			       		Id = skill.Id.GetValueOrDefault(),
			       		HasWorkloads = skill.WorkloadCollection.Any(),
			       		Name = skill.Name,
			       		SkillTypeId = skill.SkillType.Id.GetValueOrDefault(),
			       		IsMultisite = skill is IMultisiteSkill,
			       		IsChild = skill is IChildSkill,
			       		IsDeleted = ((IDeleteTag)skill).IsDeleted,
			       		WorkloadModels = skill.WorkloadCollection.Select(CreateWorkloadModel).ToList()
			       	};
		}

		private static WorkloadModel CreateWorkloadModel(IWorkload workload)
		{
			return new WorkloadModel
			       	{
			       		Id = workload.Id.GetValueOrDefault(),
			       		Name = workload.Name,
			       		Queues =
			       			workload.QueueSourceCollection.Select(q => new QueueModel {Id = q.Id.GetValueOrDefault(), Name = q.Name}).
			       			ToList(),
			       		IsDeleted = ((IDeleteTag) workload).IsDeleted
			       	};
		}

		private void toolStripMenuItemActionSkillDelete_Click(object sender, EventArgs e)
		{
			var skill = (SkillModel) _lastActionNode.Tag;
			removeSkill(skill);
		}

		private void removeSkill(SkillModel skillModel)
		{
			string questionString = string.Format(CultureInfo.CurrentCulture, Resources.QuestionDeleteTheSkillTwoParameters, "\"", skillModel.Name);
			if (ViewBase.ShowYesNoMessage(questionString, Resources.Delete) == DialogResult.Yes)
			{
				IEnumerable<IRootChangeInfo> changes;
				using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
				{
					ISkillRepository skillRep = _repositoryFactory.CreateSkillRepository(uow);
					IWorkloadRepository workloadRep = _repositoryFactory.CreateWorkloadRepository(uow);
					var skill = skillRep.Get(skillModel.Id); //To solve Bug when removing MultisiteSkill, Get instead of Load as Load didn't enable the inheritance --> IMultisiteSkill
					
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
        private void removeWorkload(WorkloadModel workloadModel)
		{
			string questionString = string.Format(CultureInfo.CurrentCulture, Resources.QuestionDeleteTheWorkloadTwoParameters, "\"", workloadModel.Name);

			if (ViewBase.ShowYesNoMessage(questionString, Resources.Delete) == DialogResult.Yes)
			{

                try
                {
                    IEnumerable<IRootChangeInfo> changes;
                    using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                    {
						var repository = _repositoryFactory.CreateWorkloadRepository(uow);
						var workload = repository.Get(workloadModel.Id);
						repository.Remove(workload);
                        changes = uow.PersistAll();
                    }
                    EntityEventAggregator.TriggerEntitiesNeedRefresh(ParentForm, changes);
                }
                catch (OptimisticLockException)
                {
                    string templateMessage = string.Concat(UserTexts.Resources.SomeoneElseHaveChanged, " {0}{1}{0} ", UserTexts.Resources.YourChangesWillBeDiscardedReloading);
                    string message = string.Format(CultureInfo.CurrentCulture, templateMessage, "\"", workloadModel.Name);
                    
                    ViewBase.ShowInformationMessage(this, message, UserTexts.Resources.SaveError);
                }
			}
		}
		
		#endregion

		private void loadSkillsTree(TreeNodeCollection skillNodes,IUnitOfWork uow)
		{
			skillNodes.Clear();

			TreeNode skillNode;

			foreach (SkillTypeModel type in loadSkillTypeCollection(uow))// Program.CommonState.SkillTypes)
			{
				skillNode = new TreeNode
								{
									Name = type.Id.ToString(),
									Text = Resources.ResourceManager.GetString(type.Name),
									ImageKey = type.ForecastSource
								};
				skillNode.SelectedImageKey = skillNode.ImageKey;
				skillNode.Tag = type;
				skillNodes.Add(skillNode);
			}

			ICollection<SkillModel> skills = loadSkillCollection(uow);
			foreach (SkillModel aSkill in skills) //Program.CommonState.Skills)
			{
				if (aSkill.IsDeleted) continue;
				skillNode = GetSkillNode(aSkill);
				skillNodes[aSkill.SkillTypeId.ToString()].Nodes.Add(skillNode);
				reloadWorkloadNodes(skillNode);
			}
		}

		private static void reloadWorkloadNodes(TreeNode skillNode)
		{
			skillNode.Nodes.Clear();
			var aSkill = (SkillModel) skillNode.Tag;
			
			foreach (WorkloadModel workload in aSkill.WorkloadModels)
			{
				if (workload.IsDeleted) continue;

				TreeNode workLoadNode = getWorkLoadNode(workload);
				skillNode.Nodes.Add(workLoadNode);
				reloadQueueSourceNodes(workLoadNode);
			}
		}

		private static TreeNode getWorkLoadNode(WorkloadModel workload)
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
			var source = (WorkloadModel)workLoadNode.Tag;
			TreeNode ctiQueueSourceNode;
			foreach (QueueModel queueSource in source.Queues)
			{
				ctiQueueSourceNode = new TreeNode(queueSource.Name) {Name = queueSource.Id.ToString()};
				workLoadNode.Nodes.Add(ctiQueueSourceNode);
				ctiQueueSourceNode.Tag = queueSource;
			}
		}

		private static TreeNode GetSkillNode(SkillModel aSkill)
        {

            if (!aSkill.IsMultisite)
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
            else
            {
                var skillNode = new TreeNode(aSkill.Name)
                {
                    Name = aSkill.Id.ToString(),
                    ImageIndex = 9,
                    SelectedImageIndex = 9,
                    Tag = aSkill
                };
                return skillNode;
            }



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
			SkillTypeModel st = getSkillType(node);
			var skillType = GetInitializedSkillType(st);
			using(var swp = new SkillWizardPages(skillType,_repositoryFactory,_unitOfWorkFactory))
			{
				swp.Initialize(PropertyPagesHelper.GetSkillPages(true, swp, skillType), new LazyLoadingManagerWrapper());
				using (var wizard = new Wizard(swp))
				{
					swp.Saved += swp_AfterSave;
					wizard.ShowDialog(this);
				}
			}
		}
        
		private SkillTypeModel getSkillType(TreeNode node)
		{
			if (node == null)
			{
				node = treeViewSkills.SelectedNode ?? treeViewSkills.Nodes[0];
			}

			node = findAncestorNodeOfType(node, typeof(SkillTypeModel));
			return (SkillTypeModel)node.Tag;
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
			if (!skill.WorkloadCollection.Any())
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
				TreeNode[] foundNodes = treeViewSkills.Nodes.Find(skill.Id.GetValueOrDefault().ToString(), true);
				if (foundNodes.Length > 0) foundNodes[0].Tag = CreateSkillModel(skill); //Just to make sure you can open forecast afterwards
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
			if (e.Node.Tag is SkillModel)
			{
				var skill = _lastActionNode.Tag as SkillModel;
				if (skill != null)
				{
					SmartPartEnvironment.SmartPartWorkspace.GridSize = GridSizeType.TwoByTwo;
					loadSmartPart(skill.Id, 1, Resources.SkillValidationSmartPart, classPrefix + validation, 0, 0);
					loadSmartPart(skill.Id, 2, Resources.DetailedForecastSmartPart, classPrefix + detailed, 0, 1);
					loadSmartPart(skill.Id, 3, Resources.LongtermForecast, classPrefix + budget, 1, 0);
					loadSmartPart(skill.Id, 4, Resources.TemplatesSmartpart, classPrefix + template, 1, 1);
				}
			}
			setMenu(_lastActionNode.Tag);
		}

		private void treeViewSkills_MouseDown(object sender, MouseEventArgs e)
		{
			if (treeViewSkills.Nodes.Count == 0) return;
			if (e.Button == MouseButtons.Right)
			{
				_lastContextMenuNode = getNodeFromPosition(e);
				if (_lastContextMenuNode == null) return;

				setContextMenu(_lastContextMenuNode.Tag);
			}
			//note i undo the fake paint
			if (treeViewSkills.Nodes[0].BackColor != ColorHelper.StandardTreeBackgroundColor()) unFakeClickColor();
			if (getNodeFromPosition(e) != null)
				treeViewSkills.SelectedNode = getNodeFromPosition(e);
		}

		private void setMenu(object nodeEntity)
		{
			if (nodeEntity is SkillTypeModel)
			{
				toggleToolstrips(toolStripSkillTypes);
			}
			else if (nodeEntity is SkillModel)
			{
				workloadOnSkillCheck();
				multisiteSkillOnSkillCheck();
				toggleToolstrips(toolStripSkills);
			}
			else if (nodeEntity is WorkloadModel)
			{
				toggleToolstrips(toolStripWorkload);
			}
			else if (nodeEntity is QueueModel)
			{
				toggleToolstrips(toolStripQueues);
			}
			else
			{
				toggleToolstrips(toolStripSkillTypes);
			}
		}

		private void setContextMenu(object nodeEntity)
		{
			var skillTypeModel = nodeEntity as SkillTypeModel;
			if (skillTypeModel!=null)
			{
				treeViewSkills.ContextMenuStrip = contextMenuStripSkillTypes;
				if (skillTypeModel.IsSkillTypePhone)
				{
					toggleExportMenu();
				}
			}
			else if (nodeEntity is SkillModel)
			{
				workloadOnSkillCheck();
				multisiteSkillOnSkillCheck();
				treeViewSkills.ContextMenuStrip = contextMenuStripSkills;
			}
			else if (nodeEntity is WorkloadModel)
			{
				treeViewSkills.ContextMenuStrip = contextMenuStripWorkloads;
			}
			else if (nodeEntity is QueueModel)
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
			var exportContextItemsVisible = PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ExportForecastToOtherBusinessUnit);
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
			var multisiteSkill = _lastActionNode.Tag as SkillModel;
			toolStripButtonManageMultisiteDistributions.Visible = (multisiteSkill != null && multisiteSkill.IsMultisite);
			toolStripMenuItemManageMultisiteDistributions.Visible = (multisiteSkill != null && multisiteSkill.IsMultisite);
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
			node = findAncestorNodeOfType(node, typeof(SkillModel));

			var s = (SkillModel)node.Tag;

			var skill = GetInitializedSkill(s);

			using(var wwp = new WorkloadWizardPages(skill,_repositoryFactory,_unitOfWorkFactory))
			{
				wwp.Initialize(PropertyPagesHelper.GetWorkloadPages(), new LazyLoadingManagerWrapper());
				using (var wizard = new Wizard(wwp))
				{
					if (wizard.ShowDialog(this) == DialogResult.Cancel)
					{}
				}
			}     
		}

		private ISkill GetInitializedSkill(SkillModel skillModel)
		{
			using(var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var repository = _repositoryFactory.CreateSkillRepository(unitOfWork);
				var skill = repository.Get(skillModel.Id);
				LazyLoadingManager.Initialize(skill.WorkloadCollection);
				LazyLoadingManager.Initialize(skill.SkillType);
				LazyLoadingManager.Initialize(skill.TemplateWeekCollection);
				return skill;
			}
		}

		private IWorkload GetInitializedWorkload(WorkloadModel workloadModel)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var repository = _repositoryFactory.CreateWorkloadRepository(unitOfWork);
				var workload = repository.Get(workloadModel.Id);
                return repository.LoadWorkload(workload);
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
			var skill = (SkillModel)_lastContextMenuNode.Tag;
			removeSkill(skill);
		}

		void toolStripMenuItemActionWorkloadDelete_Click(object sender, EventArgs e)
		{
			var workload = (WorkloadModel) _lastActionNode.Tag;
			removeWorkload(workload);
		}

		private void toolStripMenuItemDeleteWorkload_Click(object sender, EventArgs e)
		{
			var workload = (WorkloadModel)_lastContextMenuNode.Tag;
			removeWorkload(workload);
		}

		private void toolStripMenuItemRemoveQueue_Click(object sender, EventArgs e)
		{
			var workload = (WorkloadModel)_lastContextMenuNode.Parent.Tag;
			var queueSource = (QueueModel)_lastContextMenuNode.Tag;
			removeQueueSource(workload, queueSource);
		}

		private void toolStripMenuItemActionQueueSourceDelete_Click(object sender, EventArgs e)
		{
			var workload = (WorkloadModel) _lastActionNode.Parent.Tag;
			var queueSource = (QueueModel) _lastActionNode.Tag;
			removeQueueSource(workload, queueSource);
		}

		private void removeQueueSource(WorkloadModel workloadModel, QueueModel queueSourceModel)
		{
			IEnumerable<IRootChangeInfo> changes = new List<IRootChangeInfo>();
			using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var repository = _repositoryFactory.CreateWorkloadRepository(uow);
				var workload = repository.Get(workloadModel.Id);
				if (workload != null)
				{
					var queueSource = workload.QueueSourceCollection.FirstOrDefault(q => q.Id == queueSourceModel.Id);
					if (queueSource != null)
					{
						workload.RemoveQueueSource(queueSource);
						changes = uow.PersistAll();
					}
				}
			}
			EntityEventAggregator.TriggerEntitiesNeedRefresh(ParentForm, changes);
		}


		private void toolStripMenuItemWorkloadProperties_Click(object sender, EventArgs e)
		{
			var w = (WorkloadModel)_lastContextMenuNode.Tag;
			showWorkloadProperties(w);
		}

		void toolStripMenuItemActionWorkloadProperties_Click(object sender, EventArgs e)
		{
			var w = (WorkloadModel) _lastActionNode.Tag;
			showWorkloadProperties(w);
		}

		private void showWorkloadProperties(WorkloadModel workloadModel)
		{
			var workload = GetInitializedWorkload(workloadModel);
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
			var skillModel = (SkillModel)_lastActionNode.Tag;
			if (skillModel == null) return;

            try
            {
            	var skill = GetInitializedSkill(skillModel);
                using (var spp = new SkillPropertiesPages(skill, _repositoryFactory, _unitOfWorkFactory))
                {
                    var pages = PropertyPagesHelper.GetSkillPages(false, spp);
                    if (skillModel.IsMultisite) PropertyPagesHelper.AddMultisiteSkillPages(pages);
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
			SkillModel skillModel = null;
			if (_lastActionNode != null)
			{
				skillModel = _lastActionNode.Tag as SkillModel;
			}
			if (skillModel == null && _lastContextMenuNode != null)
			{
				skillModel = _lastContextMenuNode.Tag as SkillModel;
			}
			if (skillModel == null)
			{
				WorkloadModel workloadModel = null;
				if (_lastActionNode != null)
				{
					workloadModel = _lastActionNode.Tag as WorkloadModel;
					if (workloadModel!=null)
					{
						skillModel = _lastActionNode.Parent.Tag as SkillModel;
					}
				}
				if (workloadModel == null && _lastContextMenuNode != null)
				{
					workloadModel = _lastContextMenuNode.Tag as WorkloadModel;
					if (workloadModel!=null)
					{
						skillModel = _lastContextMenuNode.Parent.Tag as SkillModel;
					}
				}
				if (skillModel == null)
					return;
			}
			if (!skillModel.HasWorkloads) return;

			OpenWizard(skillModel);
		}

		private void OpenWizard(SkillModel skillModel)
		{
			var skill = GetInitializedSkill(skillModel);
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
		private void showPrepareWorkload(WorkloadModel workloadModel)
		{
			Cursor = Cursors.WaitCursor;
			if (workloadModel != null)
			{
				var workload = GetInitializedWorkload(workloadModel);
				var forecastWorkflow = new ForecastWorkflow(workload);
				forecastWorkflow.Show();
			}
			Cursor = Cursors.Default;
		}

		void toolStripMenuItemActionWorkloadPrepareForecast_Click(object sender, EventArgs e)
		{
			var w = _lastActionNode.Tag as WorkloadModel;
			showPrepareWorkload(w);
		}

		private void toolStripMenuItemWorkloadPrepareWorkload_Click(object sender, EventArgs e)
		{
			var w = _lastContextMenuNode.Tag as WorkloadModel;
			showPrepareWorkload(w);
		}

		private void toolStripMenuItemManageDayTemplates_Click(object sender, EventArgs e)
		{
			var skillModel = _lastActionNode.Tag as SkillModel;
			if (skillModel != null)
			{
				var skill = GetInitializedSkill(skillModel);
				var templateTool = new SkillDayTemplates(skill);
				templateTool.Show(this);
			}
		}

		private void toolStripMenuItemActionSkillNewMultisiteSkill_Click(object sender, EventArgs e)
		{
			var skillType = getSkillType(_lastActionNode);
			createMultisiteSkill(skillType);
		}

		private void toolStripMenuItemMultisiteSkillNew_Click(object sender, EventArgs e)
		{
			var skillType = getSkillType(_lastContextMenuNode);
			createMultisiteSkill(skillType);
		}

		private void toolStripMenuItemActionSkillTypeNewMultisiteSkill_Click(object sender, EventArgs e)
		{
			var skillType = getSkillType(_lastActionNode);
			createMultisiteSkill(skillType);
		}

		private void toolStripMenuItemSkillTypesMultisiteSkillNew_Click(object sender, EventArgs e)
		{
			var skillType = getSkillType(_lastContextMenuNode);
			createMultisiteSkill(skillType);
		}

		private ISkillType GetInitializedSkillType(SkillTypeModel skillTypeModel)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var repsoitory = _repositoryFactory.CreateSkillTypeRepository(uow);
				return repsoitory.Get(skillTypeModel.Id);
			}
		}

		private void createMultisiteSkill(SkillTypeModel skillTypeModel)
		{
            var culture = TeleoptiPrincipal.Current.Regional.Culture;
			var skillType = GetInitializedSkillType(skillTypeModel);

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
			var skillModel = _lastActionNode.Tag as SkillModel;
			if (skillModel != null && skillModel.IsMultisite)
			{
				var skill = GetInitializedSkill(skillModel);
				var templateTool = new MultisiteDayTemplates((IMultisiteSkill)skill);
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
			var workloadModel = _lastActionNode.Tag as WorkloadModel;
			var workload = GetInitializedWorkload(workloadModel);
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
            settingProvider.TransformSerializableToSelectionModels().ForEach(
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
            node = findAncestorNodeOfType(node, typeof(SkillModel));
            var skillModel = (SkillModel)node.Tag;
            if (!skillModel.HasWorkloads)
            {
                ViewBase.ShowWarningMessage("No workload available.", Resources.ImportError);
                return;
            }
        	var skill = GetInitializedSkill(skillModel);
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

		private class SkillModel
		{
			public Guid Id { get; set; }
			public string Name { get; set; }
			public bool HasWorkloads { get; set; }
			public bool IsMultisite { get; set; }
			public IList<WorkloadModel> WorkloadModels { get; set; }
			public Guid SkillTypeId { get; set; }
			public bool IsDeleted { get; set; }

			public bool IsChild { get; set; }
		}

		private class SkillTypeModel
		{
			public Guid Id { get; set; }
			public string Name { get; set; }
			public string ForecastSource { get; set; }
			public bool IsSkillTypePhone { get; set; }
		}

		private class WorkloadModel
		{
			public Guid Id { get; set; }
			public string Name { get; set; }
			public IList<QueueModel> Queues { get; set; }
			public bool IsDeleted { get; set; }
		}

		private class QueueModel
		{
			public Guid Id { get; set; }
			public string Name { get; set; }
		}
	}
}
