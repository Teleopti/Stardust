using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.ExportPages;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Main;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ExportPages;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.QuickForecastPages;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.SkillPages;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.WorkloadPages;
using Teleopti.Ccc.UserTexts;
using DataSourceException = Teleopti.Ccc.Domain.Infrastructure.DataSourceException;
using Wizard = Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.PropertyPageAndWizard.Wizard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
	public partial class ForecasterNavigator : AbstractNavigator
	{
		private TreeNodeAdv _lastActionNode;
		private TreeNodeAdv _lastContextMenuNode;
		private readonly IJobHistoryViewFactory _jobHistoryViewFactory;
		private readonly IImportForecastViewFactory _importForecastViewFactory;
		private readonly IToggleManager _toggleManager;
		private readonly IBusinessRuleConfigProvider _businessRuleConfigProvider;
		private readonly IEventPublisher _publisher;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private const string classPrefix = "Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts.Forecasting";
		private const string detailed = ".DetailedSmartPart";
		private const string validation = ".ValidationSmartPart";
		private const string budget = ".BudgetsSmartPart";
		private const string template = ".TemplateSmartPart";

		private readonly IGracefulDataSourceExceptionHandler _dataSourceExceptionHandler =
			new GracefulDataSourceExceptionHandler();

		private Form _mainWindow;
		private IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;
		private readonly IStaffingCalculatorServiceFacade _staffingCalculatorServiceFacade;
		private readonly IConfigReader _configReader;
		private readonly IStatisticHelper _statisticHelper;
		private bool _hidePriorityToggle;
		private readonly IApplicationInsights _applicationInsights;
		private readonly IComponentContext _container;

		protected ForecasterNavigator(IStatisticHelper statisticHelper, IBusinessRuleConfigProvider businessRuleConfigProvider, IApplicationInsights applicationInsights, IComponentContext container)
		{
			_statisticHelper = statisticHelper;
			_businessRuleConfigProvider = businessRuleConfigProvider;
			_applicationInsights = applicationInsights;
			_container = container;
			InitializeComponent();
			var license = DefinedLicenseDataFactory.GetLicenseActivator(UnitOfWorkFactory.Current.Name);
			if (license.EnabledLicenseSchemaName == DefinedLicenseSchemaCodes.TeleoptiWFMForecastsSchema)
			{
				skillMenuQuickForecast.Visible = false;
				workloadMenuQuickForecast.Visible = false;
				skillTypeMenuQuickForecast.Visible = false;
				xxQuickForecastToolStripMenuItem.Visible = false;
				xxQuickForecastToolStripMenuItem1.Visible = false;
			}
			if (!DesignMode)
			{
				SetTexts();
				EntityEventAggregator.EntitiesNeedsRefresh += entitiesNeedsRefresh;
			}
		}

		private void setVisibility()
		{
			toolStripMenuItemCopyTo.Visible = _toggleManager.IsEnabled(Toggles.Forecast_CopySettingsToWorkflow_11112);
			var instance = PrincipalAuthorization.Current();
			toolStripMenuItemActionSkillImportForecast.Visible =
				instance.IsPermitted(
					DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile);
			toolStripMenuItemSkillsImportForecast.Visible =
				instance.IsPermitted(
					DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile);
			_hidePriorityToggle = _toggleManager.IsEnabled(Toggles.ResourcePlanner_HideSkillPrioSliders_41312);
		}

		public ForecasterNavigator(IRepositoryFactory repositoryFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IJobHistoryViewFactory jobHistoryViewFactory,
			IImportForecastViewFactory importForecastViewFactory,
			IToggleManager toggleManager,
			IEventPublisher publisher,
			IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator, 
			IStatisticHelper statisticHelper, 
			IBusinessRuleConfigProvider businessRuleConfigProvider,
			IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade,
			IConfigReader configReader, 
			IApplicationInsights applicationInsights,
			IComponentContext container)
			: this(statisticHelper, businessRuleConfigProvider, applicationInsights, container)
		{
			_jobHistoryViewFactory = jobHistoryViewFactory;
			_importForecastViewFactory = importForecastViewFactory;
			_toggleManager = toggleManager;
			_repositoryFactory = repositoryFactory;
			_unitOfWorkFactory = unitOfWorkFactory;
			_publisher = publisher;
			_eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
			_staffingCalculatorServiceFacade = staffingCalculatorServiceFacade;
			_configReader = configReader;
			

			setVisibility();
		}

		public void SetMainOwner(Form mainWindow)
		{
			_mainWindow = mainWindow;
		}

		#region Event handling when entities are updated

		private void entitiesNeedsRefresh(object sender, EntitiesUpdatedEventArgs e)
		{
			using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				if (typeof(IWorkload).IsAssignableFrom(e.EntityType))
				{
					var repository = _repositoryFactory.CreateWorkloadRepository(uow);
					foreach (Guid guid in e.UpdatedIds)
					{
						List<TreeNodeAdv> foundNodes =
							treeViewSkills.Nodes.Cast<TreeNodeAdv>().Where(tempNode => ((skillTypeModel)tempNode.Tag).Id == guid).ToList();
						bool isWorkloadSelected = false;
						if (foundNodes.Count > 0)
						{
							isWorkloadSelected = foundNodes[0].IsSelected;
							foundNodes[0].Remove();
						}

						IWorkload workload = repository.Get(guid);
						if (workload == null) continue;

						var model = createWorkloadModel(workload);
						var skillRepository = _repositoryFactory.CreateSkillRepository(uow);
						ISkill skill = skillRepository.Get(model.SkillId);
						var skillTypeNode =
								treeViewSkills.Nodes.Cast<TreeNodeAdv>()
									.Where(tempNode => ((skillTypeModel)tempNode.Tag).Id == skill.SkillType.Id)
									.ToList();
						var skillNodes =
								skillTypeNode[0].Nodes.Cast<TreeNodeAdv>()
									.Where(tempNode => ((skillModel)tempNode.Tag).Id == skill.Id)
									.ToList();
						if (model.IsDeleted && skillNodes.Count > 0)
						{
							skillNodes[0].Nodes.Clear();
							var skillModel = createSkillModel(skill);
							foreach (workloadModel tempWorkload in skillModel.WorkloadModels)
							{
								if (tempWorkload.IsDeleted) continue;

								TreeNodeAdv workLoadNode = getWorkLoadNode(tempWorkload);
								skillNodes[0].Nodes.Add(workLoadNode);
								reloadQueueSourceNodes(workLoadNode);
							}
							continue;
						}

						TreeNodeAdv workloadNode = getWorkLoadNode(model);
						if (skillNodes.Count > 0)
						{
							skillNodes[0].Nodes.Clear();
							skillNodes[0].Tag = createSkillModel(skill);
							reloadWorkloadNodes(skillNodes[0]);
							reloadQueueSourceNodes(workloadNode);
						}
						if (isWorkloadSelected)
							treeViewSkills.SelectedNode = workloadNode;
					}
				}
				else if (typeof(ISkill).IsAssignableFrom(e.EntityType))
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
							List<TreeNodeAdv> foundNodes =
								treeViewSkills.Nodes.Cast<TreeNodeAdv>().Where(tempNode => ((skillTypeModel)tempNode.Tag).Id == guid).ToList();
							bool isSkillSelected = false;
							if (foundNodes.Count > 0)
							{
								isSkillSelected = foundNodes[0].IsSelected;
								foundNodes[0].Remove();
							}

							ISkill skill = repository.Get(guid);
							if (skill == null) continue;

							var model = createSkillModel(skill);
							if (model.IsDeleted || model.IsChild)
							{
								foundNodes =
								treeViewSkills.Nodes.Cast<TreeNodeAdv>()
									.Where(tempNode => ((skillTypeModel)tempNode.Tag).Id == skill.SkillType.Id)
									.ToList();
								if (foundNodes.Count == 0) continue;
								reloadSkillTypeNode(foundNodes[0]);
								continue;
							}

							TreeNodeAdv skillNode = getSkillNode(model);
							foundNodes =
								treeViewSkills.Nodes.Cast<TreeNodeAdv>()
									.Where(tempNode => ((skillTypeModel)tempNode.Tag).Id == skill.SkillType.Id)
									.ToList();
							if (foundNodes.Count > 0)
							{
								reloadSkillTypeNode(foundNodes[0]);
							}
							if (isSkillSelected)
								treeViewSkills.SelectedNode = skillNode;
						}
					}
				}
			}
		}


		#endregion

		#region uow_Stuff 

		private ICollection<skillTypeModel> loadSkillTypeCollection(IUnitOfWork uow)
		{
			ISkillTypeRepository skillTypeRep = new SkillTypeRepository(uow);
			var skillTypes = skillTypeRep.LoadAll();
			skillTypes = skillTypes.Where(s => s.ForecastSource != ForecastSource.OutboundTelephony).ToList();

			return
				skillTypes.Select(
					s =>
						new skillTypeModel
						{
							Id = s.Id.GetValueOrDefault(),
							Name = s.Description.Name,
							ForecastSource = (int)s.ForecastSource,
							IsSkillTypePhone = s is ISkillTypePhone
						}).ToList();
		}

		private ICollection<skillModel> loadSkillCollection(IUnitOfWork uow)
		{
			ISkillRepository skillRep = _repositoryFactory.CreateSkillRepository(uow);
			ICollection<ISkill> skills = skillRep.FindAllWithWorkloadAndQueues();
			skills = skills.Except(skills.OfType<IChildSkill>()).ToList();
			skills = skills.Where(s => s.SkillType.ForecastSource != ForecastSource.OutboundTelephony).ToList();

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
			var newWorkLoadModel = new workloadModel
			{
				Id = workload.Id.GetValueOrDefault(),
				Name = workload.Name,
				Queues =
					workload.QueueSourceCollection.Select(q => new QueueModel { Id = q.Id.GetValueOrDefault(), Name = q.Name }).
						ToList(),
				IsDeleted = ((IDeleteTag)workload).IsDeleted
			};
			if (workload.Skill.Id.HasValue)
				newWorkLoadModel.SkillId = workload.Skill.Id.Value;
			return newWorkLoadModel;
		}

		private void toolStripMenuItemActionSkillDeleteClick(object sender, EventArgs e)
		{
			var skill = (skillModel)_lastActionNode.Tag;
			removeSkill(skill);
		}

		private void removeSkill(skillModel skillModel)
		{
			string questionString = string.Format(CultureInfo.CurrentCulture, Resources.QuestionDeleteTheSkillTwoParameters, "\"",
				skillModel.Name);
			if (ViewBase.ShowYesNoMessage(questionString, Resources.Delete) == DialogResult.Yes)
			{
				IEnumerable<IRootChangeInfo> changes;
				using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
				{
					ISkillRepository skillRep = _repositoryFactory.CreateSkillRepository(uow);
					IWorkloadRepository workloadRep = _repositoryFactory.CreateWorkloadRepository(uow);
					var skill = skillRep.Get(skillModel.Id);
					//To solve Bug when removing MultisiteSkill, Get instead of Load as Load didn't enable the inheritance --> IMultisiteSkill

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


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId =
				"Teleopti.Ccc.Win.Common.ViewBase.ShowInformationMessage(System.Windows.Forms.IWin32Window,System.String,System.String)"
			),
		 System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
			 "CA1303:Do not pass literals as localized parameters",
			 MessageId = "Teleopti.Ccc.Win.Common.ViewBase.ShowInformationMessage(System.String,System.String)")]
		private void removeWorkload(workloadModel workloadModel)
		{
			string questionString = string.Format(CultureInfo.CurrentCulture, Resources.QuestionDeleteTheWorkloadTwoParameters,
				"\"", workloadModel.Name);

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
					string templateMessage = string.Concat(Resources.SomeoneElseHaveChanged, " {0}{1}{0} ",
						Resources.YourChangesWillBeDiscardedReloading);
					string message = string.Format(CultureInfo.CurrentCulture, templateMessage, "\"", workloadModel.Name);

					ViewBase.ShowInformationMessage(this, message, Resources.SaveError);
				}
			}
		}

		#endregion

		private void loadSkillsTree(TreeNodeAdvCollection skillNodes, IUnitOfWork uow)
		{
			skillNodes.Clear();

			TreeNodeAdv skillNode;

			foreach (skillTypeModel type in loadSkillTypeCollection(uow)) // Program.CommonState.SkillTypes)
			{
				skillNode = new TreeNodeAdv
				{
					Tag = type,
					Text = Resources.ResourceManager.GetString(type.Name),
					LeftImageIndices = new[] { type.ForecastSource }
				};
				skillNode.ExpandImageIndex = skillNode.CollapseImageIndex;
				skillNodes.Add(skillNode);
			}

			ICollection<skillModel> skills = loadSkillCollection(uow);
			foreach (skillModel aSkill in skills)
			{
				if (aSkill.IsDeleted) continue;
				skillNode = getSkillNode(aSkill);
				var foundNode = skillNodes.Cast<TreeNodeAdv>()
					.ToArray().Where(s => ((skillTypeModel)s.Tag).Id == aSkill.SkillTypeId);
				foundNode.FirstOrDefault().Nodes.Add(skillNode);
				reloadWorkloadNodes(skillNode);
			}
			treeViewSkills.Nodes.Sort();
		}


		private void reloadWorkloadNodes(TreeNodeAdv skillNode)
		{
			skillNode.Nodes.Clear();
			var aSkill = (skillModel)skillNode.Tag;
			foreach (workloadModel workload in aSkill.WorkloadModels)
			{
				if (workload.IsDeleted) continue;

				TreeNodeAdv workLoadNode = getWorkLoadNode(workload);
				skillNode.Nodes.Add(workLoadNode);
				reloadQueueSourceNodes(workLoadNode);
			}
			skillNode.Nodes.Sort();
		}

		private void reloadSkillTypeNode(TreeNodeAdv node)
		{
			node.Nodes.Clear();
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				ICollection<skillModel> skills = loadSkillCollection(unitOfWork);
				TreeNodeAdv skillNode;
				foreach (skillModel aSkill in skills)
				{
					if (aSkill.IsDeleted) continue;
					if (aSkill.SkillTypeId != ((skillTypeModel)node.Tag).Id) continue;
					skillNode = getSkillNode(aSkill);
					node.Nodes.Add(skillNode);
					reloadWorkloadNodes(skillNode);
				}
			}
		}

		private static TreeNodeAdv getWorkLoadNode(workloadModel workload)
		{
			var workLoadNode = new TreeNodeAdv(workload.Name)
			{
				Tag = workload,
				CollapseImageIndex = 7,
				LeftImageIndices = new[] { 7 },
			};
			return workLoadNode;
		}

		private static void reloadQueueSourceNodes(TreeNodeAdv workLoadNode)
		{
			workLoadNode.Nodes.Clear();
			var source = (workloadModel)workLoadNode.Tag;
			TreeNodeAdv ctiQueueSourceNode;
			foreach (QueueModel queueSource in source.Queues)
			{
				ctiQueueSourceNode = new TreeNodeAdv(queueSource.Name) { Tag = queueSource };
				workLoadNode.Nodes.Add(ctiQueueSourceNode);
				ctiQueueSourceNode.Tag = queueSource;
			}
			workLoadNode.Nodes.Sort();
		}

		private static TreeNodeAdv getSkillNode(skillModel aSkill)
		{

			if (!aSkill.IsMultisite)
			{
				var skillNode = new TreeNodeAdv(aSkill.Name)
				{
					Tag = aSkill,
					CollapseImageIndex = 6,
					LeftImageIndices = new[] { 6 },
				};
				return skillNode;

			}
			else
			{
				var skillNode = new TreeNodeAdv(aSkill.Name)
				{
					Tag = aSkill,
					CollapseImageIndex = 9,
					LeftImageIndices = new[] { 9 },
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

		private void queryPersonForNewSkill(TreeNodeAdv node)
		{
			skillTypeModel st = getSkillType(node);
			var skillType = getInitializedSkillType(st);
			using (var swp = new SkillWizardPages(skillType, _repositoryFactory, _unitOfWorkFactory, _staffingCalculatorServiceFacade))
			{
				swp.Initialize(PropertyPagesHelper.GetSkillPages(true, swp, skillType, _hidePriorityToggle), new LazyLoadingManagerWrapper());
				using (var wizard = new Wizard(swp))
				{
					swp.Saved += swpAfterSave;
					wizard.ShowDialog(this);
				}
			}
		}

		private skillTypeModel getSkillType(TreeNodeAdv node)
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId =
				"System.Windows.Forms.MessageBoxAdv.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)"
			)]
		private void initializeWorkloadCollectionForSkill(ISkill skill, IAbstractPropertyPages skillPropertyPages)
		{
			if (!skill.WorkloadCollection.Any())
			{
				if (ViewBase.ShowYesNoMessage(Resources.QuestionWouldYouLikeToCreateWorkloadQuestionMark, Resources.Skill) ==
					DialogResult.Yes)
				{
					//Hide the other window first
					if (skillPropertyPages != null) skillPropertyPages.Owner.Visible = false;

					DialogResult result;
					do
					{
						using (var wwp = new WorkloadWizardPages(skill, _repositoryFactory, _unitOfWorkFactory))
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
				List<TreeNodeAdv> foundNodes =
					treeViewSkills.Nodes.Cast<TreeNodeAdv>().Where(tempNode => ((skillTypeModel)tempNode.Tag).Id == skill.Id).ToList();
				if (foundNodes.Count > 0)
					foundNodes[0].Tag = createSkillModel(skill); //Just to make sure you can open forecast afterwards
			}
		}

		private static TreeNodeAdv findAncestorNodeOfType(TreeNodeAdv startingNode, Type typeOfObject)
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

		private void loadSmartPart(Guid skill, int smartPartId, string smartPartHeaderTitle,
			string smartPartName, int row, int col)
		{
			var smartPartInfo = new SmartPartInformation
			{
				ContainingAssembly = GetType().Assembly,
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
			if (skillTypeModel != null)
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
			var instance = PrincipalAuthorization.Current();
			var directExportEnabled =
				instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.ExportForecastToOtherBusinessUnit);
			var fileExportEnabled = instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.ExportForecastFile);
			var fileImportEnabled = instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile);
			toolStripMenuItemExport.Visible = directExportEnabled || fileExportEnabled;
			toolStripMenuItemJobHistory.Visible = directExportEnabled || fileImportEnabled;
			toolStripSeparatorExport.Visible = directExportEnabled || fileExportEnabled;
		}

		private TreeNodeAdv getNodeFromPosition(MouseEventArgs e)
		{
			return treeViewSkills.GetNodeAtPoint(e.X, e.Y);
		}

		private void toggleToolstrips(ToolStrip showThis)
		{
			toolStripSkillTypes.Visible = false;
			toolStripSkills.Visible = false;
			toolStripWorkload.Visible = false;
			toolStripQueues.Visible = false;
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

		private void toolStripMenuItemActionWorkloadNewSkillClick(object sender, EventArgs e)
		{
			queryPersonForNewSkill(_lastActionNode);
		}

		private void toolStripMenuItemWorkloadNewClick(object sender, EventArgs e)
		{
			queryPersonForNewWorkload(_lastContextMenuNode);
		}

		private void toolStripMenuItemActionWorkloadNewWorkloadClick(object sender, EventArgs e)
		{
			queryPersonForNewWorkload(_lastActionNode);
		}

		private void toolStripMenuItemActionQueueSourceNewWorkloadClick(object sender, EventArgs e)
		{
			queryPersonForNewWorkload(_lastActionNode);
		}

		private void queryPersonForNewWorkload(TreeNodeAdv node)
		{
			node = findAncestorNodeOfType(node, typeof(skillModel));

			if (node != null)
			{
				var s = (skillModel)node.Tag;

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
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
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
				var skill = (skillModel)_lastContextMenuNode.Tag;
				removeSkill(skill);
			}
		}

		private void toolStripMenuItemActionWorkloadDeleteClick(object sender, EventArgs e)
		{
			if (_lastActionNode != null)
			{
				var workload = (workloadModel)_lastActionNode.Tag;
				removeWorkload(workload);
			}
		}

		private void toolStripMenuItemDeleteWorkloadClick(object sender, EventArgs e)
		{
			if (_lastContextMenuNode != null)
			{
				var workload = (workloadModel)_lastContextMenuNode.Tag;
				removeWorkload(workload);
			}
		}

		private void toolStripMenuItemRemoveQueueClick(object sender, EventArgs e)
		{
			if (_lastContextMenuNode != null)
			{
				var workload = (workloadModel)_lastContextMenuNode.Parent.Tag;
				var queueSource = (QueueModel)_lastContextMenuNode.Tag;
				removeQueueSource(workload, queueSource);
			}
		}

		private void toolStripMenuItemActionQueueSourceDeleteClick(object sender, EventArgs e)
		{
			if (_lastActionNode != null)
			{
				var workload = (workloadModel)_lastActionNode.Parent.Tag;
				var queueSource = (QueueModel)_lastActionNode.Tag;
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
				var w = (workloadModel)_lastContextMenuNode.Tag;
				showWorkloadProperties(w);
			}
		}

		private void toolStripMenuItemActionWorkloadPropertiesClick(object sender, EventArgs e)
		{
			if (_lastActionNode != null)
			{
				var w = (workloadModel)_lastActionNode.Tag;
				showWorkloadProperties(w);
			}
		}

		private void showWorkloadProperties(workloadModel workloadModel)
		{
			var workload = getInitializedWorkload(workloadModel);
			using (var wpp = new WorkloadPropertiesPages(workload, _repositoryFactory, _unitOfWorkFactory))
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
				using (var spp = new SkillPropertiesPages(skill, _repositoryFactory, _unitOfWorkFactory, _staffingCalculatorServiceFacade))
				{
					var pages = PropertyPagesHelper.GetSkillPages(false, spp, _hidePriorityToggle);
					if (skillModel.IsMultisite) PropertyPagesHelper.AddMultisiteSkillPages(pages, _staffingCalculatorServiceFacade);
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
					if (workloadModel != null)
					{
						skillModel = _lastActionNode.Parent.Tag as skillModel;
					}
				}
				if (workloadModel == null && _lastContextMenuNode != null)
				{
					workloadModel = _lastContextMenuNode.Tag as workloadModel;
					if (workloadModel != null)
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"
			)]
		private void startForecaster(DateOnlyPeriod selectedPeriod, IScenario scenario, ISkill skill)
		{
			Cursor = Cursors.WaitCursor;
			var forecaster = new Forecaster(skill, selectedPeriod, scenario, true, _toggleManager, _mainWindow, _statisticHelper, _businessRuleConfigProvider, _staffingCalculatorServiceFacade, _configReader);
			forecaster.Show();
			Cursor = Cursors.Default;
			_applicationInsights.TrackEvent("Opened forecast for a skill in Forecast Module.");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"
			)]
		private void showPrepareWorkload(workloadModel workloadModel)
		{
			Cursor = Cursors.WaitCursor;
			if (workloadModel != null)
			{
				var workload = getInitializedWorkload(workloadModel);
				var forecastWorkflow = new ForecastWorkflow(workload, _statisticHelper);
				forecastWorkflow.Show();
			}
			Cursor = Cursors.Default;
		}

		private void toolStripMenuItemActionWorkloadPrepareForecastClick(object sender, EventArgs e)
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
			ISkillType skillType;
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var repsoitory = new SkillTypeRepository(uow);
				skillType = repsoitory.Get(skillTypeModel.Id);
			}
			return skillType;
		}

		private void createMultisiteSkill(skillTypeModel skillTypeModel)
		{
			var culture = TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture;
			var skillType = getInitializedSkillType(skillTypeModel);

			IMultisiteSkill skill = new MultisiteSkill(
				Resources.LessThanSkillNameGreaterThan,
				string.Format(culture, Resources.SkillCreatedDotParameter0, DateTime.Now),
				Color.FromArgb(0), skillType.DefaultResolution, skillType);
			new SkillWizardPages(skill, null, null, _staffingCalculatorServiceFacade).SetSkillDefaultSettings(skill);

			DialogResult result;
			using (var swp = new SkillWizardPages(skill, _repositoryFactory, _unitOfWorkFactory, _staffingCalculatorServiceFacade))
			{
				swp.Initialize(PropertyPagesHelper.GetSkillPages(true, swp, skillType, _hidePriorityToggle), new LazyLoadingManagerWrapper());
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
			IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork();

			IMultisiteSkill skill = new MultisiteSkillRepository(uow).Get(e.SavedAggregateRoot.Id.Value);
			skill.SkillType.ForecastSource = skill.SkillType.ForecastSource;
			//If child exist then dispose and return.
			if (skill.ChildSkills.Any())
			{
				uow.Dispose();
				return;
			}

			uow.Dispose();

			if (ViewBase.ShowYesNoMessage(Resources.QuestionCreateSubSkill, Resources.MultisiteSkillWizard) == DialogResult.Yes)
			{
				//Hide the other window first
				var skillPropertyPages = (AbstractPropertyPages<ISkill>)sender;
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
						skill.DisplayColor, skill);

					new SkillWizardPages(childSkill, null, null, _staffingCalculatorServiceFacade).SetSkillDefaultSettings(childSkill);
					using (var swp = new SkillWizardPages(childSkill, _repositoryFactory, _unitOfWorkFactory, _staffingCalculatorServiceFacade))
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
				var model = new QuickForecastModel { WorkloadIds = getWorkloadIds(treeViewSkills.SelectedNode) };
				using (var wwp = new QuickForecastWizardPages(model))
				{
					wwp.Initialize(quickForecastPages);
					using (var wizard = new WizardNoRoot<QuickForecastModel>(wwp, _container))
					{
						if (wizard.ShowDialog(this) == DialogResult.OK)
						{
							var message = wwp.CreateServiceBusMessage();
							var everyStep = Convert.ToInt32((1000 / message.WorkloadIds.Count) / 3);
							message.IncreaseWith = everyStep;
							var period = new DateOnlyPeriod(new DateOnly(message.TargetPeriodStart), new DateOnly(message.TargetPeriodEnd));
							var jobResultRep = new JobResultRepository(new ThisUnitOfWork(uow));
							var jobResult = new JobResult(JobCategory.QuickForecast, period,
											  ((ITeleoptiPrincipalForLegacy)TeleoptiPrincipalForLegacy.CurrentPrincipal).UnsafePerson, DateTime.UtcNow);
							jobResultRep.Add(jobResult);
							var jobId = jobResult.Id.GetValueOrDefault();
							uow.PersistAll();
							message.JobId = jobId;
							_eventInfrastructureInfoPopulator.PopulateEventContext(message);
							_publisher.Publish(message);

							using (
								var statusDialog =
									new JobStatusView(new JobStatusModel { JobStatusId = jobId, ProgressMax = everyStep * message.WorkloadIds.Count * 3 }))
							{
								statusDialog.ShowDialog();
							}
						}
					}
				}
			}
		}

		private ICollection<Guid> getWorkloadIds(TreeNodeAdv TreeNodeAdv)
		{
			var retList = new List<Guid>();
			if (TreeNodeAdv != null)
			{
				var workload = TreeNodeAdv.Tag as workloadModel;
				if (workload != null) retList.Add(workload.Id);
				foreach (TreeNodeAdv node in TreeNodeAdv.Nodes)
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"
			)]
		private void toolStripMenuItemExportClick(object sender, EventArgs e)
		{
			var instance = PrincipalAuthorization.Current();
			using (
				var model =
					new ExportSkillModel(
						instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.ExportForecastToOtherBusinessUnit),
						instance.IsPermitted(DefinedRaptorApplicationFunctionPaths.ExportForecastFile)))
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
								var firstPageFromPages = (SelectExportType)pages.FirstPage;
								if (b)
								{
									pages.ChangePages(PropertyPagesHelper.GetExportSkillToFilePages(firstPageFromPages, _staffingCalculatorServiceFacade));
								}
								else
								{
									pages.ChangePages(
										PropertyPagesHelper.GetExportAcrossBusinessUnitsPages(firstPageFromPages));
								}
							});
					var exportToFilePages = PropertyPagesHelper.GetExportSkillToFilePages(firstPage, _staffingCalculatorServiceFacade);

					pages.Initialize(exportToFilePages);
					using (var wizard = new WizardNoRoot<ExportSkillModel>(pages, _container))
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
								var message = model.ExportMultisiteSkillToSkillCommandModel.TransformToServiceBusMessage();
								using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
								{
									var jobResultRep = new JobResultRepository(new ThisUnitOfWork(uow));
									var period = new DateOnlyPeriod(new DateOnly(message.PeriodStart), new DateOnly(message.PeriodEnd));
									var jobResult = new JobResult(JobCategory.MultisiteExport, period,
																			((ITeleoptiPrincipalForLegacy)TeleoptiPrincipalForLegacy.CurrentPrincipal).UnsafePerson, DateTime.UtcNow);
									jobResultRep.Add(jobResult);
									message.JobId = jobResult.Id.GetValueOrDefault();
									uow.PersistAll();
								}

								_eventInfrastructureInfoPopulator.PopulateEventContext(message);
								_publisher.Publish(message);
								try
								{
									pages.SaveSettings();
									var statusDialog =
										new JobStatusView(new JobStatusModel { JobStatusId = message.JobId });
									statusDialog.Show(this);
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

		private static void initializeExportForecastToFileWizard(ExportSkillModel model,
			ExportForecastToFileSettingsProvider forecastExportSettingProvider)
		{
			var savedSettings = forecastExportSettingProvider.ExportForecastToFileSettings;
			if (!savedSettings.Period.Equals(new DateOnlyPeriod()))
				model.ExportSkillToFileCommandModel.Period = savedSettings.Period;
		}

		private static void initializeExportAcrossBusinessUnitsWizard(ExportSkillModel model,
			IExportAcrossBusinessUnitsSettingsProvider settingProvider)
		{
			model.ExportMultisiteSkillToSkillCommandModel = new ExportMultisiteSkillToSkillCommandModel();
			var savedSettings = settingProvider.ExportAcrossBusinessUnitsSettings;
			settingProvider.TransformSerializableToSelectionModels().ForEach(
				model.ExportMultisiteSkillToSkillCommandModel.MultisiteSkillSelectionModels.Add);
			if (!savedSettings.Period.Equals(new DateOnlyPeriod()))
				model.ExportMultisiteSkillToSkillCommandModel.Period = savedSettings.Period;
		}

		private void toolStripMenuItemJobHistoryClick(object sender, EventArgs e)
		{
			_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() => _jobHistoryViewFactory.Create());
		}

		private void importForecast(TreeNodeAdv node)
		{
			node = findAncestorNodeOfType(node, typeof(skillModel));

			var skillModel = (skillModel)node.Tag;
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
			public int ForecastSource { get; set; }
			public bool IsSkillTypePhone { get; set; }
		}

		private class workloadModel
		{
			public Guid Id { get; set; }
			public string Name { get; set; }
			public IList<QueueModel> Queues { get; set; }
			public bool IsDeleted { get; set; }
			public Guid SkillId { get; set; }
		}

		private class QueueModel
		{
			public Guid Id { get; set; }
			public string Name { get; set; }
		}

		private void treeViewSkillsBeforeSelectUpdated(object sender, TreeViewAdvCancelableSelectionEventArgs args)
		{
			_lastActionNode = args.SelectedNodes[0];

			//to display smartpart
			if (args.SelectedNodes[0].Tag is skillModel)
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
	}
}
