using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Common.PropertyPageAndWizard;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.Forecasting.Forms.ExportPages;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Forecasting;
using Teleopti.Ccc.WinCode.Forecasting.ExportPages;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast;
using Teleopti.Ccc.WinCode.Forecasting.QuickForecastPages;
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
		private readonly IJobHistoryViewFactory _jobHistoryViewFactory;
	    private readonly IImportForecastViewFactory _importForecastViewFactory;
	    private readonly ISendCommandToSdk _sendCommandToSdk;
		private readonly IToggleManager _toggleManager;
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
				EntityEventAggregator.EntitiesNeedsRefresh += entitiesNeedsRefresh;
			}		
		}

	    private void setVisibility()
	    {
		    toolStripMenuItemCopyTo.Visible = _toggleManager.IsEnabled(Toggles.Forecast_CopySettingsToWorkflow_11112);
			var instance = PrincipalAuthorization.Instance();
			toolStripMenuItemActionSkillImportForecast.Visible =
	            instance.IsPermitted(
	                DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile);
	        toolStripMenuItemSkillsImportForecast.Visible =
	            instance.IsPermitted(
	                DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile);
	    }

	    public ForecasterNavigator(PortalSettings portalSettings, 
            IRepositoryFactory repositoryFactory, 
            IUnitOfWorkFactory unitOfWorkFactory, 
            IJobHistoryViewFactory jobHistoryViewFactory,
            IImportForecastViewFactory importForecastViewFactory,
            ISendCommandToSdk sendCommandToSdk,
			 IToggleManager toggleManager)
            : this()
        {
		    _jobHistoryViewFactory = jobHistoryViewFactory;
            _importForecastViewFactory = importForecastViewFactory;
            _sendCommandToSdk = sendCommandToSdk;
		    _toggleManager = toggleManager;
            _repositoryFactory = repositoryFactory;
            _unitOfWorkFactory = unitOfWorkFactory;
            splitContainer1.SplitterDistance = splitContainer1.Height - portalSettings.ForecasterActionPaneHeight;

			setVisibility();
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

						var model = createWorkloadModel(workload);
						if(model.IsDeleted)
						{
							foundNodes = treeViewSkills.Nodes.Find(workload.Skill.Id.ToString(), true);
							if(foundNodes.Length>0) foundNodes[0].Tag = createSkillModel(workload.Skill);

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

							var model = createSkillModel(skill);
							if (model.IsDeleted || model.IsChild) continue;

							TreeNode skillNode = getSkillNode(model);
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
			var skill = node.Tag as skillModel;
			if (skill == null) return;
			if (force || !skill.WorkloadModels.Any())
			{
				var repository = _repositoryFactory.CreateSkillRepository(uow);
				var updatedSkill = repository.Get(skill.Id);
				node.Tag = createSkillModel(updatedSkill);
			}
		}

		#endregion

		//Dont know about this but, this might need to be standardized
		//But it works for now, we need to go on with the Sprint
		#region uow_Stuff 

		private ICollection<skillTypeModel> loadSkillTypeCollection(IUnitOfWork uow)
		{
			ISkillTypeRepository skillTypeRep = _repositoryFactory.CreateSkillTypeRepository(uow);
			ICollection<ISkillType> skillTypes = skillTypeRep.FindAll();
			return skillTypes.Select(s => new skillTypeModel{Id = s.Id.GetValueOrDefault(),Name = s.Description.Name,ForecastSource = s.ForecastSource.ToString(),IsSkillTypePhone = s is ISkillTypePhone}).ToList();
		}

		private ICollection<skillModel> loadSkillCollection(IUnitOfWork uow)
		{
			ISkillRepository skillRep = _repositoryFactory.CreateSkillRepository(uow);
			ICollection<ISkill> skills = skillRep.FindAllWithWorkloadAndQueues();
			skills = skills.Except(skills.OfType<IChildSkill>()).ToList();
			return skills.Select(createSkillModel).ToList();
		}

		private static skillModel createSkillModel(ISkill skill)
		{
			return new skillModel
			       	{
			       		Id = skill.Id.GetValueOrDefault(),
			       		HasWorkloads = skill.WorkloadCollection.Any(),
			       		Name = skill.Name,
			       		SkillTypeId = skill.SkillType.Id.GetValueOrDefault(),
			       		IsMultisite = skill is IMultisiteSkill,
			       		IsChild = skill is IChildSkill,
			       		IsDeleted = ((IDeleteTag)skill).IsDeleted,
			       		WorkloadModels = skill.WorkloadCollection.Select(createWorkloadModel).ToList()
			       	};
		}

		private static workloadModel createWorkloadModel(IWorkload workload)
		{
			return new workloadModel
			       	{
			       		Id = workload.Id.GetValueOrDefault(),
			       		Name = workload.Name,
			       		Queues =
			       			workload.QueueSourceCollection.Select(q => new QueueModel {Id = q.Id.GetValueOrDefault(), Name = q.Name}).
			       			ToList(),
			       		IsDeleted = ((IDeleteTag) workload).IsDeleted
			       	};
		}

		private void toolStripMenuItemActionSkillDeleteClick(object sender, EventArgs e)
		{
			var skill = (skillModel) _lastActionNode.Tag;
			removeSkill(skill);
		}

		private void removeSkill(skillModel skillModel)
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
        private void removeWorkload(workloadModel workloadModel)
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
                    string templateMessage = string.Concat(Resources.SomeoneElseHaveChanged, " {0}{1}{0} ", Resources.YourChangesWillBeDiscardedReloading);
                    string message = string.Format(CultureInfo.CurrentCulture, templateMessage, "\"", workloadModel.Name);
                    
                    ViewBase.ShowInformationMessage(this, message, Resources.SaveError);
                }
			}
		}
		
		#endregion

		private void loadSkillsTree(TreeNodeCollection skillNodes,IUnitOfWork uow)
		{
			skillNodes.Clear();

			TreeNode skillNode;

			foreach (skillTypeModel type in loadSkillTypeCollection(uow))// Program.CommonState.SkillTypes)
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

			ICollection<skillModel> skills = loadSkillCollection(uow);
			foreach (skillModel aSkill in skills) //Program.CommonState.Skills)
			{
				if (aSkill.IsDeleted) continue;
				skillNode = getSkillNode(aSkill);
				skillNodes[aSkill.SkillTypeId.ToString()].Nodes.Add(skillNode);
				reloadWorkloadNodes(skillNode);
			}
		}

		private static void reloadWorkloadNodes(TreeNode skillNode)
		{
			skillNode.Nodes.Clear();
			var aSkill = (skillModel) skillNode.Tag;
			
			foreach (workloadModel workload in aSkill.WorkloadModels)
			{
				if (workload.IsDeleted) continue;

				TreeNode workLoadNode = getWorkLoadNode(workload);
				skillNode.Nodes.Add(workLoadNode);
				reloadQueueSourceNodes(workLoadNode);
			}
		}

		private static TreeNode getWorkLoadNode(workloadModel workload)
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
			var source = (workloadModel)workLoadNode.Tag;
			TreeNode ctiQueueSourceNode;
			foreach (QueueModel queueSource in source.Queues)
			{
				ctiQueueSourceNode = new TreeNode(queueSource.Name) {Name = queueSource.Id.ToString()};
				workLoadNode.Nodes.Add(ctiQueueSourceNode);
				ctiQueueSourceNode.Tag = queueSource;
			}
		}

		private static TreeNode getSkillNode(skillModel aSkill)
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

		private void toolStripMenuItemActionSkillTypeNewSkillClick(object sender, EventArgs e)
		{
			queryPersonForNewSkill(_lastActionNode);
		}

		private void toolStripMenuItemSkillsNewClick(object sender, EventArgs e)
		{
			queryPersonForNewSkill(_lastContextMenuNode);
		}

		private void toolStripMenuItemActionSkillNewSkillClick(object sender, EventArgs e)
		{
			queryPersonForNewSkill(_lastActionNode);
		}

		private void toolStripMenuItemSkillTypesSkillNewClick(object sender, EventArgs e)
		{
			queryPersonForNewSkill(_lastContextMenuNode);
		}

		private void queryPersonForNewSkill(TreeNode node)
		{
			skillTypeModel st = getSkillType(node);
			var skillType = getInitializedSkillType(st);
			using(var swp = new SkillWizardPages(skillType,_repositoryFactory,_unitOfWorkFactory))
			{
				swp.Initialize(PropertyPagesHelper.GetSkillPages(true, swp, skillType), new LazyLoadingManagerWrapper());
				using (var wizard = new Wizard(swp))
				{
					swp.Saved += swpAfterSave;
					wizard.ShowDialog(this);
				}
			}
		}
        
		private skillTypeModel getSkillType(TreeNode node)
		{
			if (node == null)
			{
				node = treeViewSkills.SelectedNode ?? treeViewSkills.Nodes[0];
			}

			node = findAncestorNodeOfType(node, typeof(skillTypeModel));
			return (skillTypeModel)node.Tag;
		}

		private void swpAfterSave(object sender, AfterSavedEventArgs e)
		{
			var skillPropertyPages = (AbstractPropertyPages<ISkill>)sender;
			skillPropertyPages.LoadAggregateRootWorkingCopy();
			ISkill skill = skillPropertyPages.AggregateRootObject;
			initializeWorkloadCollectionForSkill(skill, skillPropertyPages);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
		private void initializeWorkloadCollectionForSkill(ISkill skill, IAbstractPropertyPages skillPropertyPages)
		{
			if (!skill.WorkloadCollection.Any())
			{
				if (ViewBase.ShowYesNoMessage(Resources.QuestionWouldYouLikeToCreateWorkloadQuestionMark, Resources.Skill) == DialogResult.Yes)
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
						ViewBase.ShowYesNoMessage(Resources.QuestionCreateAnotherWorkload, Resources.SkillWizard)
						== DialogResult.Yes);
				}
				TreeNode[] foundNodes = treeViewSkills.Nodes.Find(skill.Id.GetValueOrDefault().ToString(), true);
				if (foundNodes.Length > 0) foundNodes[0].Tag = createSkillModel(skill); //Just to make sure you can open forecast afterwards
			}
		}

		private static TreeNode findAncestorNodeOfType(TreeNode startingNode, Type typeOfObject)
		{
            if (startingNode != null)
            {
                while (!typeOfObject.IsInstanceOfType(startingNode.Tag))
                {
                    startingNode = startingNode.Parent;
                }
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

		private void treeViewSkillsBeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			_lastActionNode = e.Node;
			
			//to display smartpart
			if (e.Node.Tag is skillModel)
			{
				var skill = _lastActionNode.Tag as skillModel;
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

		private void treeViewSkillsMouseDown(object sender, MouseEventArgs e)
		{
			if (treeViewSkills.Nodes.Count == 0) return;
			if (e.Button == MouseButtons.Right)
			{
				_lastContextMenuNode = getNodeFromPosition(e);
				if (_lastContextMenuNode == null)
				{
                    setContextMenu(null);
				    return;
				}

				setContextMenu(_lastContextMenuNode.Tag);
			}

			if (getNodeFromPosition(e) != null)
				treeViewSkills.SelectedNode = getNodeFromPosition(e);
		}

		private void setMenu(object nodeEntity)
		{
			if (nodeEntity is skillTypeModel)
			{
				toggleToolstrips(toolStripSkillTypes);
			}
			else if (nodeEntity is skillModel)
			{
				workloadOnSkillCheck();
				multisiteSkillOnSkillCheck();
				toggleToolstrips(toolStripSkills);
			}
			else if (nodeEntity is workloadModel)
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
			var skillTypeModel = nodeEntity as skillTypeModel;
			if (skillTypeModel!=null)
			{
				treeViewSkills.ContextMenuStrip = contextMenuStripSkillTypes;
				if (skillTypeModel.IsSkillTypePhone)
				{
					toggleExportMenu();
				}
			}
			else if (nodeEntity is skillModel)
			{
				workloadOnSkillCheck();
				multisiteSkillOnSkillCheck();
				treeViewSkills.ContextMenuStrip = contextMenuStripSkills;
			}
			else if (nodeEntity is workloadModel)
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
			var instance = PrincipalAuthorization.Instance();
			var directExportEnabled = instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.ExportForecastToOtherBusinessUnit);
			var fileExportEnabled = instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.ExportForecastFile);
			var fileImportEnabled = instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile);
			toolStripMenuItemExport.Visible = directExportEnabled || fileExportEnabled;
			toolStripMenuItemJobHistory.Visible = directExportEnabled || fileImportEnabled;
			toolStripSeparatorExport.Visible = directExportEnabled || fileExportEnabled;
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
			var multisiteSkill = _lastActionNode.Tag as skillModel;
			toolStripButtonManageMultisiteDistributions.Visible = (multisiteSkill != null && multisiteSkill.IsMultisite);
			toolStripMenuItemManageMultisiteDistributions.Visible = (multisiteSkill != null && multisiteSkill.IsMultisite);
		}

		private void workloadOnSkillCheck()
		{
			toolStripMenuCreateForecast.Enabled = (_lastActionNode.Nodes.Count > 0);
		}

		#endregion



		private void toolStripMenuItemActionSkillAddNewWorkloadClick(object sender, EventArgs e)
		{
			queryPersonForNewWorkload(_lastActionNode);
		}

		private void toolStripMenuItemWorkloadSkillNewClick(object sender, EventArgs e)
		{
			queryPersonForNewSkill(_lastContextMenuNode);
		}

		void toolStripMenuItemActionWorkloadNewSkillClick(object sender, EventArgs e)
		{
			queryPersonForNewSkill(_lastActionNode);
		}

		private void toolStripMenuItemWorkloadNewClick(object sender, EventArgs e)
		{
			queryPersonForNewWorkload(_lastContextMenuNode);
		}

		void toolStripMenuItemActionWorkloadNewWorkloadClick(object sender, EventArgs e)
		{
			queryPersonForNewWorkload(_lastActionNode);
		}

		private void toolStripMenuItemActionQueueSourceNewWorkloadClick(object sender, EventArgs e)
		{
			queryPersonForNewWorkload(_lastActionNode);
		}

		private void queryPersonForNewWorkload(TreeNode node)
		{
			node = findAncestorNodeOfType(node, typeof(skillModel));

            if (node != null)
            {
                var s = (skillModel) node.Tag;

                var skill = getInitializedSkill(s);

                using (var wwp = new WorkloadWizardPages(skill, _repositoryFactory, _unitOfWorkFactory))
                {
                    wwp.Initialize(PropertyPagesHelper.GetWorkloadPages(), new LazyLoadingManagerWrapper());
                    using (var wizard = new Wizard(wwp))
                    {
                        if (wizard.ShowDialog(this) == DialogResult.Cancel)
                        {
                        }
                    }
                }
            }
		}

		private ISkill getInitializedSkill(skillModel skillModel)
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

		private IWorkload getInitializedWorkload(workloadModel workloadModel)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var repository = _repositoryFactory.CreateWorkloadRepository(unitOfWork);
			    var workload = repository.Get(workloadModel.Id);
			    LazyLoadingManager.Initialize(workload.Skill);
			    LazyLoadingManager.Initialize(workload.Skill.SkillType);
			    LazyLoadingManager.Initialize(workload.TemplateWeekCollection);
			    foreach (var template in workload.TemplateWeekCollection.Values)
			    {
			        LazyLoadingManager.Initialize(template.OpenHourList);
			    }
                LazyLoadingManager.Initialize(workload.QueueSourceCollection);
			    return workload;
			}
		}

		private void toolStripMenuItemNewWorkloadClick(object sender, EventArgs e)
		{
			queryPersonForNewWorkload(_lastActionNode);
		}

		private void toolStripMenuItemQueueWorkloadNewClick(object sender, EventArgs e)
		{
			queryPersonForNewWorkload(_lastActionNode);
		}

		private void toolStripMenuItemSkillsDeleteClick(object sender, EventArgs e)
		{
            if (_lastContextMenuNode != null)
            {
                var skill = (skillModel) _lastContextMenuNode.Tag;
                removeSkill(skill);
            }
		}

		void toolStripMenuItemActionWorkloadDeleteClick(object sender, EventArgs e)
		{
            if (_lastActionNode != null)
            {
                var workload = (workloadModel) _lastActionNode.Tag;
                removeWorkload(workload);
            }
		}

		private void toolStripMenuItemDeleteWorkloadClick(object sender, EventArgs e)
		{
            if (_lastContextMenuNode != null)
            {
                var workload = (workloadModel) _lastContextMenuNode.Tag;
                removeWorkload(workload);
            }
		}

		private void toolStripMenuItemRemoveQueueClick(object sender, EventArgs e)
		{
            if (_lastContextMenuNode != null)
            {
                var workload = (workloadModel) _lastContextMenuNode.Parent.Tag;
                var queueSource = (QueueModel) _lastContextMenuNode.Tag;
                removeQueueSource(workload, queueSource);
            }
		}

		private void toolStripMenuItemActionQueueSourceDeleteClick(object sender, EventArgs e)
		{
            if (_lastActionNode != null)
            {
                var workload = (workloadModel) _lastActionNode.Parent.Tag;
                var queueSource = (QueueModel) _lastActionNode.Tag;
                removeQueueSource(workload, queueSource);
            }
		}

		private void removeQueueSource(workloadModel workloadModel, QueueModel queueSourceModel)
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


		private void toolStripMenuItemWorkloadPropertiesClick(object sender, EventArgs e)
		{
            if (_lastContextMenuNode != null)
            {
                var w = (workloadModel) _lastContextMenuNode.Tag;
                showWorkloadProperties(w);
            }
		}

		void toolStripMenuItemActionWorkloadPropertiesClick(object sender, EventArgs e)
		{
            if (_lastActionNode != null)
            {
                var w = (workloadModel) _lastActionNode.Tag;
                showWorkloadProperties(w);
            }
		}

		private void showWorkloadProperties(workloadModel workloadModel)
		{
			var workload = getInitializedWorkload(workloadModel);
			using (var wpp = new WorkloadPropertiesPages(workload,_repositoryFactory,_unitOfWorkFactory))
			{
				wpp.Initialize(PropertyPagesHelper.GetWorkloadPages(), new LazyLoadingManagerWrapper());
				using (var propertiesPages = new PropertiesPages(wpp))
				{
					propertiesPages.ShowDialog(this);
				}
			}
		}

		private void toolStripMenuItemActionSkillPropertiesClick(object sender, EventArgs e)
		{
			showSkillProperties();
		}

		private void toolStripMenuItemSkillsPropertiesClick(object sender, EventArgs e)
		{
			showSkillProperties();
		}

		private void showSkillProperties()
		{
            var skillModel = (skillModel)_lastActionNode.Tag;
			if (skillModel == null) return;

            try
            {
            	var skill = getInitializedSkill(skillModel);
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
            }           
		}

		private void toolStripMenuCreateForecastClick(object sender, EventArgs e)
		{
			skillModel skillModel = null;
			if (_lastActionNode != null)
			{
				skillModel = _lastActionNode.Tag as skillModel;
			}
			if (skillModel == null && _lastContextMenuNode != null)
			{
				skillModel = _lastContextMenuNode.Tag as skillModel;
			}
			if (skillModel == null)
			{
				workloadModel workloadModel = null;
				if (_lastActionNode != null)
				{
					workloadModel = _lastActionNode.Tag as workloadModel;
					if (workloadModel!=null)
					{
						skillModel = _lastActionNode.Parent.Tag as skillModel;
					}
				}
				if (workloadModel == null && _lastContextMenuNode != null)
				{
					workloadModel = _lastContextMenuNode.Tag as workloadModel;
					if (workloadModel!=null)
					{
						skillModel = _lastContextMenuNode.Parent.Tag as skillModel;
					}
				}
				if (skillModel == null)
					return;
			}
			if (!skillModel.HasWorkloads) return;

			openWizard(skillModel);
		}

		private void openWizard(skillModel skillModel)
		{
			var skill = getInitializedSkill(skillModel);
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
			var forecaster = new Forecaster(skill, selectedPeriod, scenario, true, _toggleManager);
			forecaster.Show();
			Cursor = Cursors.Default;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void showPrepareWorkload(workloadModel workloadModel)
		{
			Cursor = Cursors.WaitCursor;
			if (workloadModel != null)
			{
				var workload = getInitializedWorkload(workloadModel);
				var forecastWorkflow = new ForecastWorkflow(workload);
				forecastWorkflow.Show();
			}
			Cursor = Cursors.Default;
		}

		void toolStripMenuItemActionWorkloadPrepareForecastClick(object sender, EventArgs e)
		{
            if (_lastActionNode != null)
            {
                var w = _lastActionNode.Tag as workloadModel;
                showPrepareWorkload(w);
            }
		}

		private void toolStripMenuItemWorkloadPrepareWorkloadClick(object sender, EventArgs e)
		{
            if (_lastContextMenuNode != null)
            {
                var w = _lastContextMenuNode.Tag as workloadModel;
                showPrepareWorkload(w);
            }
		}

		private void toolStripMenuItemManageDayTemplatesClick(object sender, EventArgs e)
		{
			var skillModel = _lastActionNode.Tag as skillModel;
			if (skillModel != null)
			{
				var skill = getInitializedSkill(skillModel);
				var templateTool = new SkillDayTemplates(skill);
				templateTool.Show(this);
			}
		}

		private void toolStripMenuItemActionSkillNewMultisiteSkillClick(object sender, EventArgs e)
		{
            var skillType = getSkillType(_lastActionNode);
            createMultisiteSkill(skillType);
		}

		private void toolStripMenuItemMultisiteSkillNewClick(object sender, EventArgs e)
		{
            var skillType = getSkillType(_lastContextMenuNode);
            createMultisiteSkill(skillType);
		}

		private void toolStripMenuItemActionSkillTypeNewMultisiteSkillClick(object sender, EventArgs e)
		{
           
            var skillType = getSkillType(_lastActionNode);
            createMultisiteSkill(skillType);
        
		}

		private void toolStripMenuItemSkillTypesMultisiteSkillNewClick(object sender, EventArgs e)
		{
            var skillType = getSkillType(_lastContextMenuNode);
            createMultisiteSkill(skillType);
	    }

		private ISkillType getInitializedSkillType(skillTypeModel skillTypeModel)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var repsoitory = _repositoryFactory.CreateSkillTypeRepository(uow);
				return repsoitory.Get(skillTypeModel.Id);
			}
		}

		private void createMultisiteSkill(skillTypeModel skillTypeModel)
		{
            var culture = TeleoptiPrincipal.Current.Regional.Culture;
			var skillType = getInitializedSkillType(skillTypeModel);

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
			initializeWorkloadCollectionForSkill(skill, null);
		}

		private void multisiteSkillSaved(object sender, AfterSavedEventArgs e)
		{
			using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				IMultisiteSkill skill = new MultisiteSkillRepository(uow).Get(e.SavedAggregateRoot.Id.Value);
				if (skill.ChildSkills.Count == 0)
				{
					if (ViewBase.ShowYesNoMessage(Resources.QuestionCreateSubSkill, Resources.MultisiteSkillWizard) == DialogResult.Yes)
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
							ViewBase.ShowYesNoMessage(Resources.QuestionCreateAnotherSubSkill, Resources.MultisiteSkillWizard)
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
		
		private void toolStripMenuItemManageDistributionClick(object sender, EventArgs e)
		{
			var skillModel = _lastActionNode.Tag as skillModel;
			if (skillModel != null && skillModel.IsMultisite)
			{
				var skill = getInitializedSkill(skillModel);
				var templateTool = new MultisiteDayTemplates((IMultisiteSkill)skill);
				templateTool.Show(this);
			}
		}

		private void toolStripMenuItemQuickForecastClick(object sender, EventArgs e)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var skillRep = _repositoryFactory.CreateSkillRepository(uow);
				var skills = skillRep.FindAllWithWorkloadAndQueues();
				skills = skills.Except(skills.OfType<IChildSkill>()).ToList();
				var quickForecastPages = PropertyPagesHelper.GetQuickForecastPages(skills);
				var model = new QuickForecastCommandDto {WorkloadIds = getWorkloadIds(treeViewSkills.SelectedNode)};
				using (var wwp = new QuickForecastWizardPages(model))
				{
					wwp.Initialize(quickForecastPages);
					using (var wizard = new WizardNoRoot<QuickForecastCommandDto>(wwp))
					{
						if (wizard.ShowDialog(this) == DialogResult.OK)
						{
							var dto = wwp.CreateNewStateObj();
							var everyStep = Convert.ToInt32((1000 / dto.WorkloadIds.Count )/ 3);
							dto.IncreaseWith = everyStep;
							var jobId = _sendCommandToSdk.ExecuteCommand(dto).AffectedId.GetValueOrDefault();
							using (
								var statusDialog =
									new JobStatusView(new JobStatusModel {JobStatusId = jobId, ProgressMax = everyStep*dto.WorkloadIds.Count*3}))
							{
								statusDialog.ShowDialog();
							}
							//_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() => _jobHistoryViewFactory.Create());
						}
					}
				}
			}
		}

		private ICollection<Guid> getWorkloadIds(TreeNode treeNode)
		{
			var retList = new List<Guid>();
			if (treeNode != null)
			{
				var workload = treeNode.Tag as workloadModel;
				if (workload != null) retList.Add(workload.Id);
				foreach (TreeNode node in treeNode.Nodes)
				{
					retList.AddRange(getWorkloadIds(node));
				}
			}
			
			return retList;
		}
		private void toolStripMenuItemCopyToClick(object sender, EventArgs e)
		{
			var workloadModel = _lastActionNode.Tag as workloadModel;
		    if (workloadModel == null) return;
		    var workload = getInitializedWorkload(workloadModel);
		    var model = new CopyToSkillModel(workload);
		    using (var view = new CopyToSkillView(model))
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
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void toolStripMenuItemExportClick(object sender, EventArgs e)
        {
        	var instance = PrincipalAuthorization.Instance();
			using (var model = new ExportSkillModel(instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.ExportForecastToOtherBusinessUnit), instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.ExportForecastFile)))
			{
				var settingProvider = new ExportAcrossBusinessUnitsSettingsProvider(_unitOfWorkFactory, _repositoryFactory);
				var forecastExportSettingProvider = new ExportForecastToFileSettingsProvider(_unitOfWorkFactory,
				                                                                             _repositoryFactory);
				_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(
					() => initializeExportAcrossBusinessUnitsWizard(model, settingProvider));

				_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(
					() => initializeExportForecastToFileWizard(model, forecastExportSettingProvider));

				using (var pages = new ExportSkillWizardPages(model, settingProvider, forecastExportSettingProvider))
				{
					var firstPage =
						PropertyPagesHelper.GetExportSkillFirstPage(
							b =>
								{
									var firstPageFromPages = (SelectExportType) pages.FirstPage;
									if (b)
									{
										pages.ChangePages(PropertyPagesHelper.GetExportSkillToFilePages(firstPageFromPages));
									}
									else
									{
										pages.ChangePages(
											PropertyPagesHelper.GetExportAcrossBusinessUnitsPages(firstPageFromPages));
									}
								});
					var exportToFilePages = PropertyPagesHelper.GetExportSkillToFilePages(firstPage);

					pages.Initialize(exportToFilePages);
					using (var wizard = new WizardNoRoot<ExportSkillModel>(pages))
					{
						if (wizard.ShowDialog(this) == DialogResult.OK)
						{
							if (model.ExportToFile)
							{
								try
								{
									pages.SaveSettings();
								}
								catch (DataSourceException exception)
								{
									_dataSourceExceptionHandler.DataSourceExceptionOccurred(exception);
								}
							}
							else
							{
								var dto = model.ExportMultisiteSkillToSkillCommandModel.TransformToDto();
								try
								{
									pages.SaveSettings();
									var statusDialog =
										new JobStatusView(new JobStatusModel {JobStatusId = Guid.NewGuid()});
									statusDialog.Show(this);
									statusDialog.SetJobStatusId(
										_sendCommandToSdk.ExecuteCommand(dto).AffectedId.GetValueOrDefault());
								}
								catch (OptimisticLockException)
								{
									ViewBase.ShowErrorMessage(
										string.Concat(Resources.SomeoneChangedTheSameDataBeforeYouDot,
										              Resources.YourChangesWillBeDiscarded),
										Resources.PleaseTryAgainLater);
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
        }

	    private static void initializeExportForecastToFileWizard(ExportSkillModel model, ExportForecastToFileSettingsProvider forecastExportSettingProvider)
	    {
            var savedSettings = forecastExportSettingProvider.ExportForecastToFileSettings;
            if (!savedSettings.Period.Equals(new DateOnlyPeriod()))
                model.ExportSkillToFileCommandModel.Period = savedSettings.Period;
	    }

	    private static void initializeExportAcrossBusinessUnitsWizard(ExportSkillModel model, IExportAcrossBusinessUnitsSettingsProvider settingProvider)
        {
            model.ExportMultisiteSkillToSkillCommandModel = new ExportMultisiteSkillToSkillCommandModel();
            var savedSettings = settingProvider.ExportAcrossBusinessUnitsSettings;
            settingProvider.TransformSerializableToSelectionModels().ForEach(
                model.ExportMultisiteSkillToSkillCommandModel.MultisiteSkillSelectionModels.Add);
			if (!savedSettings.Period.Equals(new DateOnlyPeriod()))
				model.ExportMultisiteSkillToSkillCommandModel.Period = new DateOnlyPeriodDto
					{
						StartDate = new DateOnlyDto {DateTime = savedSettings.Period.StartDate},
						EndDate = new DateOnlyDto {DateTime = savedSettings.Period.EndDate}
					};
        }

	    private void toolStripMenuItemJobHistoryClick(object sender, EventArgs e)
	    {
	        _dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() => _jobHistoryViewFactory.Create());
	    }

        private void importForecast(TreeNode node)
        {
            node = findAncestorNodeOfType(node, typeof(skillModel));
          
            var skillModel = (skillModel) node.Tag;
            if (!skillModel.HasWorkloads)
            {
                ViewBase.ShowWarningMessage("No workload available.", Resources.ImportError);
                return;
            }
            var skill = getInitializedSkill(skillModel);
            _importForecastViewFactory.Create(skill);
        }

        private void toolStripMenuItemSkillsImportForecastClick(object sender, EventArgs e)
        {
            importForecast(_lastContextMenuNode);
        }

        private void toolStripMenuItemActionSkillImportForecastClick(object sender, EventArgs e)
        {
            importForecast(_lastActionNode);
        }

		private class skillModel
		{
			public Guid Id { get; set; }
			public string Name { get; set; }
			public bool HasWorkloads { get; set; }
			public bool IsMultisite { get; set; }
			public IList<workloadModel> WorkloadModels { get; set; }
			public Guid SkillTypeId { get; set; }
			public bool IsDeleted { get; set; }

			public bool IsChild { get; set; }
		}

		private class skillTypeModel
		{
			public Guid Id { get; set; }
			public string Name { get; set; }
			public string ForecastSource { get; set; }
			public bool IsSkillTypePhone { get; set; }
		}

		private class workloadModel
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
