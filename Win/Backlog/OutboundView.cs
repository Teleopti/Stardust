using System;
using System.Collections.Generic;
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
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Campaign = Teleopti.Ccc.Domain.Outbound.Campaign;

namespace Teleopti.Ccc.Win.Backlog
{
	public partial class OutboundView : Form
	{
		private readonly IComponentContext _container;

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
		}

		private void loadCampaigns()
		{
			ICollection<Campaign> campaigns;
			// move to method in repository
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				campaigns = new OutboundCampaignRepository(uow).LoadAll();
				foreach (var campaign in campaigns)
				{
					LazyLoadingManager.Initialize(campaign.Skill);
					LazyLoadingManager.Initialize(campaign.Skill.Activity);
				}
			}

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
			//var outboundSkillCreator = _container.Resolve<OutboundSkillCreator>();
			//var activityRepository = _container.Resolve<IActivityRepository>();
			IActivity activity;
			ISkill skill;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var activityRepository = new ActivityRepository(uow);
				activity = activityRepository.LoadAll().First(); //just using first, make this for real
				var outboundSkillCreator = new OutboundSkillCreator(_container.Resolve<IUserTimeZone>(),
					new OutboundSkillTypeProvider(new SkillTypeRepository(uow)));
				skill = outboundSkillCreator.CreateSkill(activity, campaign);
			}
			//skill = outboundSkillCreator.CreateSkill(activity, campaign);
			var outboundSkillPersister = _container.Resolve<OutboundSkillPersister>();
			outboundSkillPersister.PersistAll(skill);
			campaign.Skill = skill;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var campaignRepository = new OutboundCampaignRepository(uow);
				campaignRepository.Add(campaign);
				uow.PersistAll();
			}

			return campaign;
		}

		private void updateAndPersistCampaignSkillPeriod(Campaign campaign)
		{
			//create productionPlan
			var incomingTaskFactory = _container.Resolve<IncomingTaskFactory>();
			var incomingTask = incomingTaskFactory.Create(new DateOnlyPeriod(campaign.StartDate.Value, campaign.EndDate.Value),
				campaign.CallListLen, TimeSpan.FromHours(campaign.ConnectAverageHandlingTime/campaign.CallListLen));
			incomingTask.RecalculateDistribution();

			//persist
			updateSkillDays(campaign.Skill, incomingTask);
		}

		private void saveSkillDays(IEnumerable<ISkillDay> dirtyList)
		{
			var dirtySkillDays = new List<ISkillDay>();
			dirtySkillDays.AddRange(dirtyList);
			foreach (var skillDay in dirtySkillDays)
			{
				using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					var skillDayRepository = new SkillDayRepository(uow);
					skillDayRepository.Add(skillDay);
					uow.PersistAll();				
				}
			}
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
					var forecastingTarget = new ForecastingTarget(dateOnly, new OpenForWork(true, true));  //fix this assumption
					//faked values for now
					forecastingTarget.Tasks = 100;
					forecastingTarget.AverageTaskTime = TimeSpan.FromMinutes(5);
					forecastingTargets.Add(forecastingTarget);
				}
				merger.Merge(forecastingTargets, workLoadDays);				
			}
			saveSkillDays(skillDays);			
		}
	}
}
