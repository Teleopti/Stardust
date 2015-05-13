﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Backlog;
using Teleopti.Interfaces.Domain;
using Campaign = Teleopti.Ccc.Domain.Outbound.Campaign;

namespace Teleopti.Ccc.Win.Backlog
{
	public partial class OutboundView : Form
	{
		private readonly IComponentContext _container;
		private BacklogScheduledProvider _backlogScheduledProvider;
		private DateOnlyPeriod _loadedPeriod;

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
			loadCampaigns();
			updateStatusOnCampaigns();
		}

		private void loadCampaigns()
		{
			ICollection<Campaign> campaigns;
			// move to method in repository
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				campaigns = new OutboundCampaignRepository(uow).LoadAll().Where(c => c.EndDate > new DateOnly(DateTime.Now).AddDays(-14)).ToList();
				foreach (var campaign in campaigns)
				{
					LazyLoadingManager.Initialize(campaign.Skill);
					LazyLoadingManager.Initialize(campaign.Skill.Activity);
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

			var earliestStart = campaigns.Min(c => c.StartDate).Value;
			var latestEnd = campaigns.Max(c => c.EndDate).Value;
			_loadedPeriod = new DateOnlyPeriod(earliestStart, latestEnd);

			listView1.Items.Clear();
			listView1.Groups.Clear();
			foreach (var campaign in campaigns)
			{
				if (campaign.IsDeleted)
					continue;
				
				var listItem = listView1.Items.Add(campaign.Name);			
				listItem.Tag = campaign;
				listItem.SubItems.Add(new DateOnlyPeriod(campaign.StartDate.Value, campaign.EndDate.Value).ToShortDateString(
					TeleoptiPrincipal.CurrentPrincipal.Regional.Culture));
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
				var campaign = createAndPersistCampaign();
				if(campaign == null)
					return;

				updateAndPersistCampaignSkillPeriod(campaign);
				loadCampaigns();		
		}

		private Campaign createAndPersistCampaign()
		{
			Campaign campaign;
			using (var addCampaign = new AddCampaign())
			{
				addCampaign.ShowDialog(this);
				if (addCampaign.DialogResult != DialogResult.OK)
					return null;

				campaign = addCampaign.CreatedCampaign;
			}

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var outboundSkillCreator = _container.Resolve<OutboundSkillCreator>();
				var activityRepository = _container.Resolve<IActivityRepository>();
				var activity = activityRepository.LoadAll().First(); //just using first, make this for real
				var skill = outboundSkillCreator.CreateSkill(activity, campaign);
				var outboundSkillPersister = _container.Resolve<OutboundSkillPersister>();
				outboundSkillPersister.PersistAll(skill);
				campaign.Skill = skill;

				var campaignRepository = new OutboundCampaignRepository(uow);
				campaignRepository.Add(campaign);
				uow.PersistAll();
			}

			return campaign;
		}

		private void updateAndPersistCampaignSkillPeriod(Campaign campaign)
		{
			var incomingTask = createProductionPlan(campaign);

			//persist productionPlan
			updateSkillDays(campaign.Skill, incomingTask);
		}

		private IncomingTask createProductionPlan(Campaign campaign)
		{
			var incomingTaskFactory = _container.Resolve<OutboundProductionPlanFactory>();
			var incomingTask =
				incomingTaskFactory.CreateAndMakeInitialPlan(new DateOnlyPeriod(campaign.StartDate.Value, campaign.EndDate.Value),
					campaign.CallListLen, TimeSpan.FromHours((double) campaign.ConnectAverageHandlingTime/campaign.CallListLen),
					campaign.CampaignWorkingPeriods.ToList());

			if(_backlogScheduledProvider == null)
				return incomingTask;

			foreach (var dateOnly in incomingTask.SpanningPeriod.DayCollection())
			{
				var scheduled = _backlogScheduledProvider.GetScheduledTimeOnDate(dateOnly, campaign.Skill);
				if(scheduled != TimeSpan.Zero)
					incomingTask.SetTimeOnDate(dateOnly, scheduled, PlannedTimeTypeEnum.Scheduled);
			}
			incomingTask.RecalculateDistribution();

			return incomingTask;
		}
	
		private void updateSkillDays(ISkill skill, IncomingTask incomingTask)
		{
			//var q = _container.Resolve<IFetchAndFillSkillDays>();
			ICollection<ISkillDay> skillDays;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var skillDayRepository = new SkillDayRepository(uow);
				var q = new FetchAndFillSkillDays(skillDayRepository, new DefaultScenarioFromRepository(new ScenarioRepository(uow)),
					skillDayRepository);
				skillDays = q.FindRange(incomingTask.SpanningPeriod, skill);
				var workload = skill.WorkloadCollection.First();
				var workLoadDays = new List<IWorkloadDay>();
				foreach (var skillDay in skillDays)
				{
					var dayOfWeek = skillDay.CurrentDate.DayOfWeek;
					var template = workload.TemplateWeekCollection[(int)dayOfWeek];
					var workloadDay = skillDay.WorkloadDayCollection.First();
					workloadDay.ApplyTemplate(template, day => { }, day => { });
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
						forecastingTarget.Tasks = 1;
						forecastingTarget.AverageTaskTime = incomingTask.GetTimeOnDate(dateOnly);
					}

					forecastingTargets.Add(forecastingTarget);
				}
				merger.Merge(forecastingTargets, workLoadDays);

				skillDayRepository.AddRange(skillDays);
				uow.PersistAll();	
			}		
		}

		private void updateStatusOnCampaigns()
		{
			foreach (var item in listView1.Items)
			{
				var listItem = (ListViewItem) item;
				var status = getStatusOnCampaign((Campaign)listItem.Tag);

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
			}
		}

		private CampaignStatus getStatusOnCampaign(Campaign campaign)
		{
			var incomingTask = createProductionPlan(campaign);
			if(incomingTask.GetTimeOutsideSLA() > TimeSpan.Zero)
				return CampaignStatus.NotInSLA;

			foreach (var dateOnly in incomingTask.SpanningPeriod.DayCollection())
			{
				if(incomingTask.GetOverstaffTimeOnDate(dateOnly) > TimeSpan.Zero)
					return CampaignStatus.Overstaffed;
			}

			return CampaignStatus.Ok;
		}

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			var selectedCampaign = listView1.SelectedItems[0];
			var incomingTask = createProductionPlan((Campaign) selectedCampaign.Tag);
			using (var outboundStatusView = new OutboundStatusView(incomingTask))
			{
				outboundStatusView.ShowDialog(this);
			}
		}

		private void toolStripButton2_Click(object sender, EventArgs e)
		{
			_backlogScheduledProvider = new BacklogScheduledProvider(_container, _loadedPeriod);
			Enabled = false;
			backgroundWorker1.RunWorkerAsync();
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
			Enabled = true;
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
