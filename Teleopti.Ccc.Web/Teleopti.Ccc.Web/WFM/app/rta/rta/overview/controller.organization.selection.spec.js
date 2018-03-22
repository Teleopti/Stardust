'use strict';

rtaTester.describe('RtaOverviewController', function (it, fit, xit) {

	it('should select site', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'parisId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isSelected = true;
		});

		expect(vm.siteCards[0].isSelected).toEqual(true);
	});

	it('should select site from url', function (t) {
		t.stateParams.siteIds = ['parisId'];
		t.backend
			.withSiteAdherence({
				Id: 'parisId'
			});

		var vm = t.createController();

		expect(vm.siteCards[0].isSelected).toEqual(true);
	});

	it('should select team from url', function (t) {
		t.stateParams.teamIds = ['redId'];
		t.backend
			.withSiteAdherence({
				Id: 'parisId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				Id: 'redId'
			});

		var vm = t.createController();

		expect(vm.siteCards[0].teams[0].isSelected).toEqual(true);
	});

	it('should open site when team in url', function (t) {
		t.stateParams.teamIds = ['redId'];
		t.backend
			.withSiteAdherence({
				Id: 'parisId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				Id: 'redId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				Id: 'greenId'
			});

		var vm = t.createController();

		expect(vm.siteCards[0].isOpen).toEqual(true);
	});

	it('should still display opened sites even though no team is selected', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'parisId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				Id: 'redId'
			});

		var vm = t.createController();
		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});

		expect(vm.siteCards[0].teams[0].isSelected).toEqual(false);
	});

	it('should go to agents for selected site', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'parisId',
				Name: 'Paris',
				AgentsCount: 11,
				InAlarmCount: 2,
				Color: 'good'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				Id: 'redId',
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isSelected = true;
		});
		t.apply(function () {
			vm.goToAgents();
		});

		expect(t.lastGoParams.siteIds).toEqual(['parisId']);
	});

	it('should go to agents for the right selected site after deselecting some other', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'parisId'
			})
			.withSiteAdherence({
				Id: 'londonId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isSelected = true;
		});
		t.apply(function () {
			vm.siteCards[1].isSelected = true;
		});
		t.apply(function () {
			vm.siteCards[1].isSelected = false;
		});
		t.apply(function () {
			vm.goToAgents();
		});

		expect(t.lastGoParams.siteIds).toEqual(['parisId']);
	});

	it('should go to agents for selected site with preselected skill', function (t) {
		t.stateParams.skillIds = ['channelSalesId'];
		t.backend
			.withSiteAdherence({
				Id: 'parisId',
				SkillId: 'channelSalesId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				Id: 'redId',
				SkillId: 'channelSalesId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isSelected = true;
		});
		t.apply(function () {
			vm.goToAgents();
		});

		expect(t.lastGoParams.siteIds).toEqual(['parisId']);
		expect(t.lastGoParams.skillIds).toEqual(['channelSalesId']);
	});

	it('should go to agents for selected site with  skill area', function (t) {
		t.stateParams.skillAreaId = 'skillArea1Id';
		t.backend
			.withSkillAreas([{
				Id: 'skillArea1Id',
				Name: 'SkillArea1',
				Skills: [{
					Name: 'Channel Sales',
					Id: 'channelSalesId'
				}, {
					Name: 'Phone',
					Id: 'phoneId'
				}]
			}, {
				Id: 'skillArea2Id',
				Name: 'SkillArea2',
				Skills: [{
					Name: 'Invoice',
					Id: 'invoiceId'
				}, {
					Name: 'BTS',
					Id: 'btsId'
				}]
			}])
			.withSiteAdherence({
				Id: 'parisId',
				SkillId: 'channelSalesId'
			})
			.withSiteAdherence({
				Id: 'parisId',
				SkillId: 'phoneId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isSelected = true;
		});
		t.apply(function () {
			vm.goToAgents();
		});

		expect(t.lastGoParams.siteIds).toEqual(['parisId']);
		expect(t.lastGoParams.skillAreaId).toEqual('skillArea1Id');
	});

	it('should go to agents for selected team', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				Name: 'London'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'greenId'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'redId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		t.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		t.apply(function () {
			vm.goToAgents();
		});

		expect(t.lastGoParams.teamIds).toEqual(['greenId']);
	});

	it('should go to agents for selected team with selected skill', function (t) {
		t.stateParams.skillIds = ['channelSalesId'];
		t.backend
			.withSkill({
				Name: 'Channel Sales',
				Id: 'channelSalesId'
			})
			.withSkill({
				Name: 'Phone',
				Id: 'phoneId'
			})
			.withSkill({
				Name: 'Invoice',
				Id: 'invoiceId'
			})
			.withSkill({
				Name: 'BTS',
				Id: 'btsId'
			})
			.withSiteAdherence({
				Id: 'londonId',
				SkillId: 'channelSalesId'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				SkillId: 'channelSalesId',
				Id: 'greenId'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				SkillId: 'channelSalesId',
				Id: 'redId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		t.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		t.apply(function () {
			vm.goToAgents();
		});

		expect(t.lastGoParams.teamIds).toEqual(['greenId']);
		expect(t.lastGoParams.skillIds).toEqual(['channelSalesId']);
	});

	it('should go to agents for selected team with preselected skill', function (t) {
		t.stateParams.skillIds = ['channelSalesId'];
		t.backend
			.withSkill({
				Name: 'Channel Sales',
				Id: 'channelSalesId'
			})
			.withSkill({
				Name: 'Phone',
				Id: 'phoneId'
			})
			.withSkill({
				Name: 'Invoice',
				Id: 'invoiceId'
			})
			.withSkill({
				Name: 'BTS',
				Id: 'btsId'
			})
			.withSiteAdherence({
				Id: 'londonId',
				SkillId: 'channelSalesId'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				SkillId: 'channelSalesId',
				Id: 'greenId'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				SkillId: 'channelSalesId',
				Id: 'redId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		t.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		t.apply(function () {
			vm.goToAgents();
		});

		expect(t.lastGoParams.teamIds).toEqual(['greenId']);
		expect(t.lastGoParams.skillIds).toEqual(['channelSalesId']);
	});

	it('should go to agents for selected team with selected skill area', function (t) {
		t.stateParams.skillAreaId = 'skillArea1Id';
		t.backend
			.withSkillAreas([{
				Id: 'skillArea1Id',
				Name: 'SkillArea1',
				Skills: [{
					Name: 'Channel Sales',
					Id: 'channelSalesId'
				}, {
					Name: 'Phone',
					Id: 'phoneId'
				}]
			}, {
				Id: 'skillArea2Id',
				Name: 'SkillArea2',
				Skills: [{
					Name: 'Invoice',
					Id: 'invoiceId'
				}, {
					Name: 'BTS',
					Id: 'btsId'
				}]
			}])
			.withSiteAdherence({
				Id: 'parisId',
				SkillId: 'channelSalesId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				SkillId: 'channelSalesId',
				Id: 'greenId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				SkillId: 'channelSalesId',
				Id: 'redId'
			})
			.withSiteAdherence({
				Id: 'parisId',
				SkillId: 'phoneId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				SkillId: 'phoneId',
				Id: 'greenId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		t.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		t.apply(function () {
			vm.goToAgents();
		});

		expect(t.lastGoParams.teamIds).toEqual(['greenId']);
		expect(t.lastGoParams.skillAreaId).toEqual('skillArea1Id');
	});

	it('should go to agents for selected team with preselected skill area', function (t) {
		t.stateParams.skillAreaId = 'skillArea1Id';
		t.backend.withSkillAreas([
			{
				Id: 'skillArea1Id',
				Name: 'SkillArea1',
				Skills: [{
					Name: 'Channel Sales',
					Id: 'channelSalesId'
				}, {
					Name: 'Phone',
					Id: 'phoneId'
				}]
			}, {
				Id: 'skillArea2Id',
				Name: 'SkillArea2',
				Skills: [{
					Name: 'Invoice',
					Id: 'invoiceId'
				}, {
					Name: 'BTS',
					Id: 'btsId'
				}]
			}])
			.withSiteAdherence({
				Id: 'parisId',
				SkillId: 'channelSalesId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				SkillId: 'channelSalesId',
				Id: 'greenId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				SkillId: 'channelSalesId',
				Id: 'redId'
			})
			.withSiteAdherence({
				Id: 'parisId',
				SkillId: 'phoneId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				SkillId: 'phoneId',
				Id: 'greenId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		t.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		t.apply(function () {
			vm.goToAgents();
		});

		expect(t.lastGoParams.teamIds).toEqual(['greenId']);
		expect(t.lastGoParams.skillAreaId).toEqual('skillArea1Id');
	});

	it('should go to agents for site if all teams under it are selected one by one', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				Name: 'London'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'greenId'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'redId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		t.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		t.apply(function () {
			vm.siteCards[0].teams[1].isSelected = true;
		});
		t.apply(function () {
			vm.goToAgents();
		});

		expect(t.lastGoParams.siteIds).toEqual(['londonId']);
		expect(t.lastGoParams.hasOwnProperty('teamIds')).toEqual(true);
		expect(t.lastGoParams.teamIds).toEqual(undefined);
	});

	it('should go to agents for teams if all teams under a site were selected and then one was unselected', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				Name: 'London'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'greenId'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'redId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		t.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		t.apply(function () {
			vm.siteCards[0].teams[1].isSelected = true;
		});
		t.apply(function () {
			vm.siteCards[0].teams[0].isSelected = false;
		});
		t.apply(function () {
			vm.goToAgents();
		});

		expect(t.lastGoParams.teamIds).toEqual(['redId']);
		expect(t.lastGoParams.hasOwnProperty('siteIds')).toEqual(true);
		expect(t.lastGoParams.siteIds).toEqual(undefined);
	});

	it('should go to agents for teams if site were selected and then one was unselected', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				Name: 'London'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'greenId'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'redId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		t.apply(function () {
			vm.siteCards[0].isSelected = true;
		});
		t.apply(function () {
			vm.siteCards[0].teams[1].isSelected = false;
		});
		t.apply(function () {
			vm.goToAgents();
		});

		expect(t.lastGoParams.teamIds).toEqual(['greenId']);
	});

	it('should always clear teams selection for teams under site if site is selected', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				Name: 'London'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'greenId'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'redId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		t.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		t.apply(function () {
			vm.siteCards[0].teams[1].isSelected = true;
		});
		t.apply(function () {
			vm.siteCards[0].teams[0].isSelected = false;
		});
		t.apply(function () {
			vm.siteCards[0].isSelected = true;
		});
		t.apply(function () {
			vm.goToAgents();
		});

		expect(t.lastGoParams.siteIds).toEqual(['londonId']);
	});

	it('should select teams under site when site is not selected and team is', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				Name: 'London'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'greenId'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'redId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		t.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		t.apply(function () {
			vm.siteCards[0].isOpen = false;
		});
		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});

		expect(vm.siteCards[0].teams[0].isSelected).toEqual(true);
	});

	it('should select all teams under site when site is selected and initially closed', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'londonId'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'greenId'
			});

		var vm = t.createController()

		t.apply(function () {
			vm.siteCards[0].isSelected = true;
		});
		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});

		expect(vm.siteCards[0].teams[0].isSelected).toEqual(true);
	});

	it('should go to agents team and site in cross site selection where one site has only one team', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				Name: 'London'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'greenId'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'pinkId'
			})
			.withSiteAdherence({
				Id: 'parisId',
				Name: 'Paris'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				Id: 'redId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		t.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		t.apply(function () {
			vm.siteCards[1].isOpen = true;
		});
		t.apply(function () {
			vm.siteCards[1].teams[0].isSelected = true;
		});
		t.apply(function () {
			vm.goToAgents();
		});

		expect(t.lastGoParams.siteIds).toEqual(['parisId']);
		expect(t.lastGoParams.teamIds).toEqual(['greenId']);
	});

});
