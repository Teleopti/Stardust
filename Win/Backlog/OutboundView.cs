using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

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

				var skillActivityName = campaign.Skill.Activity.Name;
				var listViewGroup = new ListViewGroup(skillActivityName, skillActivityName);
				if (!listView1.Groups.Contains(listViewGroup))
				{
					listView1.Groups.Add(listViewGroup);
				}
				else
				{
					listViewGroup = listView1.Groups[skillActivityName];
				}

				var listItem = listView1.Items.Add(campaign.Name);			
				listItem.Tag = campaign;
				listItem.SubItems.Add(new DateOnlyPeriod(campaign.StartDate.Value, campaign.EndDate.Value).ToShortDateString(
					TeleoptiPrincipal.CurrentPrincipal.Regional.Culture));
				listItem.SubItems.Add("Unknown");
				listItem.Group = listViewGroup;
			}
		}

		private void toolStripButtonAddCampaignClick(object sender, EventArgs e)
		{
			createAndPersistCampaign();
			loadCampaigns();
		}

		private void createAndPersistCampaign()
		{
			Campaign campaign = null;
			using (var addCampaign = new AddCampaign())
			{
				addCampaign.ShowDialog(this);
				if (addCampaign.DialogResult != DialogResult.OK)
					return;

				campaign = addCampaign.CreatedCampaign;
			}
			var outboundSkillCreator = _container.Resolve<OutboundSkillCreator>();
			var activityRepository = _container.Resolve<IActivityRepository>();
			var activity = activityRepository.LoadAll().First(); //just faking an activity
			var skill = outboundSkillCreator.CreateSkill(activity, campaign);
			var outboundSkillPersister = _container.Resolve<OutboundSkillPersister>();
			outboundSkillPersister.PersistAll(skill);
			campaign.Skill = skill;
			//var campaignRepository = _container.Resolve<IOutboundCampaignRepository>();
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var campaignRepository = new OutboundCampaignRepository(uow);
				campaignRepository.Add(campaign);
				uow.PersistAll();
			}
		}
	}
}
