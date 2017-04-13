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
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public partial class OutboundView : Form
	{
		private readonly IComponentContext _container;
		private IOutboundScheduledResourcesProvider _outboundScheduledResourcesProvider;
		private DateOnlyPeriod _loadedPeriod;
        ICollection<IOutboundCampaign> _campaigns;

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
			_outboundScheduledResourcesProvider = _container.Resolve<IOutboundScheduledResourcesProvider>();

			Enabled = false;
			backgroundWorker1.RunWorkerAsync();
		}

		private void loadCampaigns()
		{
			// move to method in repository
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_campaigns = new OutboundCampaignRepository(uow).LoadAll().Where(c => c.SpanningPeriod.EndDateTime > DateTime.Now.AddDays(-30)).ToList();
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
					foreach (var workingHour in campaign.WorkingHours)
					{
						LazyLoadingManager.Initialize(workingHour);
					}
				}
			}

			if (!_campaigns.Any())
			{
				return;
			}

				var earliestStart = _campaigns.Min(c => new DateOnly(c.SpanningPeriod.StartDateTime));
				var latestEnd = _campaigns.Max(c =>  new DateOnly(c.SpanningPeriod.EndDateTime));
				_loadedPeriod = new DateOnlyPeriod(earliestStart, latestEnd);

			

			listView1.Items.Clear();
			listView1.Groups.Clear();
			foreach (var campaign in _campaigns)
			{
				if (campaign.IsDeleted)
					continue;
				
				var listItem = listView1.Items.Add(campaign.Name);			
				listItem.Tag = campaign;
				listItem.SubItems.Add(campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone).ToShortDateString(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture));
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

        private IncomingTask getIncomingTaskFromCampaign(IOutboundCampaign campaign)
		{
			var incomingTaskFactory = _container.Resolve<OutboundProductionPlanFactory>();
			var incomingTask = incomingTaskFactory.CreateAndMakeInitialPlan(campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone), campaign.CampaignTasks(),
				campaign.AverageTaskHandlingTime(), campaign.WorkingHours);

			if (_outboundScheduledResourcesProvider == null)
				return incomingTask;

			foreach (var dateOnly in incomingTask.SpanningPeriod.DayCollection())
			{
				var manualTime = campaign.GetManualProductionPlan(dateOnly);
				if(manualTime.HasValue)
					incomingTask.SetTimeOnDate(dateOnly, manualTime.Value, PlannedTimeTypeEnum.Manual);
				var scheduled = _outboundScheduledResourcesProvider.GetScheduledTimeOnDate(dateOnly, campaign.Skill);
				var forecasted = _outboundScheduledResourcesProvider.GetForecastedTimeOnDate(dateOnly, campaign.Skill);
				if (scheduled != TimeSpan.Zero)
					incomingTask.SetTimeOnDate(dateOnly, scheduled, PlannedTimeTypeEnum.Scheduled);
				else if (forecasted != TimeSpan.Zero && !manualTime.HasValue)
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
						var timeOnDate = incomingTask.GetTimeOnDate(dateOnly);
						forecastingTarget.Tasks = timeOnDate.TotalSeconds / incomingTask.AverageWorkTimePerItem.TotalSeconds;
						forecastingTarget.AverageTaskTime = incomingTask.AverageWorkTimePerItem;
						_outboundScheduledResourcesProvider.SetForecastedTimeOnDate(dateOnly, skill, timeOnDate);
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
                var campaign = (IOutboundCampaign)listItem.Tag;
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
					campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone).ToShortDateString(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture);
			}
		}

        private CampaignStatus getStatusOnCampaign(IOutboundCampaign campaign)
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

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_outboundScheduledResourcesProvider.Load(_campaigns.ToList(), _loadedPeriod);
			}
			
		}

		private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
		{

		}

		private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			Enabled = true;
			updateStatusOnCampaigns();		
		}

		private void viewStatusToolStripMenuItem_Click(object sender, EventArgs e)
		{
            var selectedCampaign = (IOutboundCampaign)listView1.SelectedItems[0].Tag;
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
            var selectedCampaign = (IOutboundCampaign)listView1.SelectedItems[0].Tag;
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
            var selectedCampaign = (IOutboundCampaign)listView1.SelectedItems[0].Tag;
			var incomingTask = getIncomingTaskFromCampaign(selectedCampaign);
			incomingTask.RecalculateDistribution();
			//persist productionPlan
			updateSkillDays(selectedCampaign.Skill, incomingTask, true);
			updateStatusOnCampaigns();
		}

		private void changePeriodToolStripMenuItem_Click(object sender, EventArgs e)
		{
            var selectedCampaign = (IOutboundCampaign)listView1.SelectedItems[0].Tag;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var oldPeriod = selectedCampaign.SpanningPeriod;
				selectedCampaign.SpanningPeriod = new DateTimePeriod(oldPeriod.StartDateTime.AddDays(7), oldPeriod.EndDateTime.AddDays(7));
				var outboundCampaignRepo = _container.Resolve<IOutboundCampaignRepository>();
				outboundCampaignRepo.Add(selectedCampaign);

				var outboundPeriodMover = _container.Resolve<IOutboundPeriodMover>();
				outboundPeriodMover.Move(selectedCampaign, oldPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc));

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
