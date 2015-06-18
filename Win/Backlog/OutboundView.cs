using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Backlog;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Campaign = Teleopti.Ccc.Domain.Outbound.Campaign;

namespace Teleopti.Ccc.Win.Backlog
{
	public partial class OutboundView : Form
	{
		private readonly IComponentContext _container;
		private BacklogScheduledProvider _backlogScheduledProvider;
		private DateOnlyPeriod _loadedPeriod;
		ICollection<Campaign> _campaigns;

		public OutboundView()
		{
			InitializeComponent();
		}

		public OutboundView(IComponentContext container)
		{
			_container = container;
			InitializeComponent();
		}

		private void outboundViewLoad(object sender, EventArgs e)
		{
			load();
		}

		private void load()
		{
			loadCampaigns();
			if(_campaigns.Any())
				loadSchedulesAsync();
		}

		private void loadSchedulesAsync()
		{
			_backlogScheduledProvider = new BacklogScheduledProvider(_container, _loadedPeriod);
			Enabled = false;
			backgroundWorker1.RunWorkerAsync();
		}

		private void loadCampaigns()
		{
			// move to method in repository
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_campaigns = new OutboundCampaignRepository(uow).LoadAll().Where(c => c.EndDate > new DateOnly(DateTime.Now).AddDays(-30)).ToList();
				foreach (var campaign in _campaigns)
				{
					LazyLoadingManager.Initialize(campaign.Skill);
					LazyLoadingManager.Initialize(campaign.Skill.Activity);
					LazyLoadingManager.Initialize(campaign.Skill.WorkloadCollection);
					LazyLoadingManager.Initialize(campaign.Skill.WorkloadCollection.First().TemplateWeekCollection);
					foreach (var workloadDayTemplate in campaign.Skill.WorkloadCollection.First().TemplateWeekCollection)
					{
						LazyLoadingManager.Initialize(workloadDayTemplate);
						LazyLoadingManager.Initialize(workloadDayTemplate.Value.OpenHourList);
					}
					LazyLoadingManager.Initialize(campaign.Skill.TemplateWeekCollection);
					foreach (var skillDayTemplate in campaign.Skill.TemplateWeekCollection)
					{
						LazyLoadingManager.Initialize(skillDayTemplate.Value.TemplateSkillDataPeriodCollection);
						foreach (var templateSkillDataPeriod in skillDayTemplate.Value.TemplateSkillDataPeriodCollection)
						{
							LazyLoadingManager.Initialize(templateSkillDataPeriod);
						}
					}
					LazyLoadingManager.Initialize(campaign.Skill.SkillType);
					foreach (var campaignWorkingPeriod in campaign.CampaignWorkingPeriods)
					{
						LazyLoadingManager.Initialize(campaignWorkingPeriod);
						foreach (var assignment in campaignWorkingPeriod.CampaignWorkingPeriodAssignments)
						{
							LazyLoadingManager.Initialize(assignment);
						}
					}
				}
			}

			if (!_campaigns.Any())
			{
				return;
			}

				var earliestStart = _campaigns.Min(c => c.StartDate).Value;
				var latestEnd = _campaigns.Max(c => c.EndDate).Value;
				_loadedPeriod = new DateOnlyPeriod(earliestStart, latestEnd);

			

			listView1.Items.Clear();
			listView1.Groups.Clear();
			foreach (var campaign in _campaigns)
			{
				if (campaign.IsDeleted)
					continue;
				
				var listItem = listView1.Items.Add(campaign.Name);			
				listItem.Tag = campaign;
				listItem.SubItems.Add(campaign.SpanningPeriod.ToShortDateString(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture));
				listItem.SubItems.Add("Unknown");

				var skillActivityName = campaign.Skill.Activity.Name;
				ListViewGroup listViewGroup = null;
				bool groupFound = false;
				foreach (var group in listView1.Groups)
				{
					listViewGroup = (ListViewGroup) group;
					if (((IActivity) listViewGroup.Tag).Equals(campaign.Skill.Activity))
					{
						groupFound = true;
						break;
					}
				}
				if (!groupFound)
				{
					listViewGroup = new ListViewGroup(skillActivityName, skillActivityName) {Tag = campaign.Skill.Activity};
					listView1.Groups.Add(listViewGroup);
				}
				listItem.Group = listViewGroup;
			}
		}

		private void toolStripButtonAddCampaignClick(object sender, EventArgs e)
		{
			Campaign campaign;
			IActivity activity;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var activityRepository = _container.Resolve<IActivityRepository>();
				var existinActivities = activityRepository.LoadAll();

				using (var addCampaign = new AddCampaign(existinActivities))
				{
					addCampaign.ShowDialog(this);
					if (addCampaign.DialogResult != DialogResult.OK)
						return;

					campaign = addCampaign.CreatedCampaign;
					activity = addCampaign.ExistingActivity;
				}

				if (activity == null)
				{
					activity = new Activity(campaign.Name)
					{
						DisplayColor = Color.Black,
						InContractTime = true,
						InPaidTime = true,
						InWorkTime = true,
						RequiresSkill = true,
						IsOutboundActivity = true,
						AllowOverwrite = true
					};
					activityRepository.Add(activity);
				}

				createAndPersistCampaign(campaign, activity, uow);
				uow.PersistAll();
			}

			loadCampaigns();
			updateStatusOnCampaigns();
		}

		//AddCampaignCommand
		private void createAndPersistCampaign(Campaign campaign, IActivity activity, IUnitOfWork uow)
		{
			var outboundSkillCreator = _container.Resolve<IOutboundSkillCreator>();
			var skill = outboundSkillCreator.CreateSkill(activity, campaign);
			var outboundSkillPersister = _container.Resolve<OutboundSkillPersister>();
			outboundSkillPersister.PersistSkill(skill);
			campaign.Skill = skill;

			var campaignRepository = new OutboundCampaignRepository(uow);
			campaignRepository.Add(campaign);

			var createOrUpdateSkillDays = _container.Resolve<ICreateOrUpdateSkillDays>();
			createOrUpdateSkillDays.Create(campaign.Skill, campaign.SpanningPeriod, campaign.CampaignTasks(),
				campaign.AverageTaskHandlingTime(), campaign.CampaignWorkingPeriods);
		}

		private IncomingTask getIncomingTaskFromCampaign(Campaign campaign)
		{
			var incomingTaskFactory = _container.Resolve<OutboundProductionPlanFactory>();
			var incomingTask = incomingTaskFactory.CreateAndMakeInitialPlan(campaign.SpanningPeriod, campaign.CampaignTasks(),
				campaign.AverageTaskHandlingTime(), campaign.CampaignWorkingPeriods.ToList());

			if(_backlogScheduledProvider == null)
				return incomingTask;

			foreach (var dateOnly in incomingTask.SpanningPeriod.DayCollection())
			{
				var manualTime = campaign.GetManualProductionPlan(dateOnly);
				if(manualTime.HasValue)
					incomingTask.SetTimeOnDate(dateOnly, manualTime.Value, PlannedTimeTypeEnum.Manual);
				var scheduled = _backlogScheduledProvider.GetScheduledTimeOnDate(dateOnly, campaign.Skill);
				var forecasted = _backlogScheduledProvider.GetForecastedTimeOnDate(dateOnly, campaign.Skill);
				if(scheduled != TimeSpan.Zero)
					incomingTask.SetTimeOnDate(dateOnly, scheduled, PlannedTimeTypeEnum.Scheduled);
				else if(forecasted != TimeSpan.Zero && !manualTime.HasValue)
					incomingTask.SetTimeOnDate(dateOnly, forecasted, PlannedTimeTypeEnum.Calculated);
			}

			return incomingTask;
		}
	
		private void updateSkillDays(ISkill skill, IncomingTask incomingTask, bool applyDefaultTemplate)
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				ICollection<ISkillDay> skillDays = _container.Resolve<IFetchAndFillSkillDays>()
					.FindRange(incomingTask.SpanningPeriod, skill);
				var workload = skill.WorkloadCollection.First();
				var workLoadDays = new List<IWorkloadDay>();
				foreach (var skillDay in skillDays)
				{
					var workloadDay = skillDay.WorkloadDayCollection.First();
					if(applyDefaultTemplate)
					{
						var dayOfWeek = skillDay.CurrentDate.DayOfWeek;
						var template = workload.TemplateWeekCollection[(int)dayOfWeek];						
						workloadDay.ApplyTemplate(template, day => { }, day => { });
					}
					workLoadDays.Add(workloadDay);
				}
				var merger = _container.Resolve<IForecastingTargetMerger>();
				var forecastingTargets = new List<IForecastingTarget>();
				foreach (var dateOnly in incomingTask.SpanningPeriod.DayCollection())
				{
					var isOpen = incomingTask.PlannedTimeTypeOnDate(dateOnly) != PlannedTimeTypeEnum.Closed;
					var forecastingTarget = new ForecastingTarget(dateOnly, new OpenForWork(isOpen, isOpen));
					if(isOpen)
					{
						forecastingTarget.Tasks = incomingTask.GetTimeOnDate(dateOnly).TotalSeconds/incomingTask.AverageWorkTimePerItem.TotalSeconds;
						forecastingTarget.AverageTaskTime = incomingTask.AverageWorkTimePerItem;
					}

					forecastingTargets.Add(forecastingTarget);
				}
				merger.Merge(forecastingTargets, workLoadDays);

				var skillDayRepository = _container.Resolve<ISkillDayRepository>();
				skillDayRepository.AddRange(skillDays);
				uow.PersistAll();	
			}		
		}

		private void updateStatusOnCampaigns()
		{
			foreach (var item in listView1.Items)
			{
				var listItem = (ListViewItem) item;
				var campaign = (Campaign) listItem.Tag;
				var status = getStatusOnCampaign(campaign);

				switch (status)
				{
						case CampaignStatus.Ok:
						listItem.SubItems[2].Text = "OK";
						listItem.ForeColor = Color.DarkGreen;
						break;

						case CampaignStatus.NotInSLA:
						listItem.SubItems[2].Text = "Outside SLA";
						listItem.ForeColor = Color.Red;
						break;

						case CampaignStatus.Overstaffed:
						listItem.SubItems[2].Text = "Overstaffed";
						listItem.ForeColor = Color.Red;
						break;
				}

				listItem.SubItems[1].Text =
					campaign.SpanningPeriod.ToShortDateString(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture);
			}
		}

		private CampaignStatus getStatusOnCampaign(Campaign campaign)
		{
			var incomingTask = getIncomingTaskFromCampaign(campaign);
			if(incomingTask.GetTimeOutsideSLA() > TimeSpan.FromMinutes(1))
				return CampaignStatus.NotInSLA;

			foreach (var dateOnly in incomingTask.SpanningPeriod.DayCollection())
			{
				if (incomingTask.GetOverstaffTimeOnDate(dateOnly) > TimeSpan.FromMinutes(1))
					return CampaignStatus.Overstaffed;
			}

			return CampaignStatus.Ok;
		}

		private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			_backlogScheduledProvider.Load();
			
		}

		private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
		{

		}

		private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			//Nix det blev konstigt
			//foreach (var campaign in _campaigns)
			//{
			//	updateAndPersistCampaignSkillPeriod(campaign, false); //lägger bara upp nya dagar med longterm, gör på annat sätt
			//}
			Enabled = true;
			updateStatusOnCampaigns();
			
		}

		private void viewStatusToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var selectedCampaign = (Campaign)listView1.SelectedItems[0].Tag;
			var incomingTask = getIncomingTaskFromCampaign(selectedCampaign);
			using (var outboundStatusView = new OutboundStatusView(incomingTask, selectedCampaign.Name))
			{
				outboundStatusView.ShowDialog(this);
			}
		}

		private void addActualBacklogToolStripMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void addManualProductionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var selectedCampaign = (Campaign)listView1.SelectedItems[0].Tag;
			using (var addManualProductionView = new AddManualProductionView(selectedCampaign))
			{
				addManualProductionView.ShowDialog(this);
				if (addManualProductionView.DialogResult == DialogResult.OK)
				{
					using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
					{
						var campaignRepository = new OutboundCampaignRepository(uow);
						campaignRepository.Add(selectedCampaign);
						uow.PersistAll();
					}

					var incomingTask = getIncomingTaskFromCampaign(selectedCampaign);
					updateSkillDays(selectedCampaign.Skill, incomingTask, false);
					updateStatusOnCampaigns();
				}
			}
		}

		private void replanToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var selectedCampaign = (Campaign)listView1.SelectedItems[0].Tag;
			var incomingTask = getIncomingTaskFromCampaign(selectedCampaign);
			incomingTask.RecalculateDistribution();
			//persist productionPlan
			updateSkillDays(selectedCampaign.Skill, incomingTask, true);
			loadSchedulesAsync();
			//no code after loadSchedulesAsync();
		}

		private void changePeriodToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var selectedCampaign = (Campaign)listView1.SelectedItems[0].Tag;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var oldPeriod = selectedCampaign.SpanningPeriod;
				selectedCampaign.SpanningPeriod = new DateOnlyPeriod(oldPeriod.StartDate.AddDays(7), oldPeriod.EndDate.AddDays(7));
				var outboundCampaignRepo = _container.Resolve<IOutboundCampaignRepository>();
				outboundCampaignRepo.Add(selectedCampaign);

				var outboundPeriodMover = _container.Resolve<IOutboundPeriodMover>();
				outboundPeriodMover.Move(selectedCampaign, oldPeriod);

				uow.PersistAll();
			}
			updateStatusOnCampaigns();
		}
	}

	public enum CampaignStatus
	{
		Ok,
		NotInSLA,
		Overstaffed
	}
}
