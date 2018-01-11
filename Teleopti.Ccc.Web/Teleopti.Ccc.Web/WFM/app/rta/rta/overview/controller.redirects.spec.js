'use strict';

rtaTester.describe('RtaOverviewController', function (it, fit, xit) {

	var
		channelSales,
		phone,
		invoice,
		bts;

	var
		skills1,
		skills2,
		allSkills;

	var
		skillArea1,
		skillArea2;

	var skillAreas;

	var goodColor = '#C2E085';
	var warningColor = '#FFC285';
	var dangerColor = '#EE8F7D';
	
	channelSales = {
		Name: 'Channel Sales',
		Id: 'channelSalesId'
	};

	phone = {
		Name: 'Phone',
		Id: 'phoneId'
	};

	invoice = {
		Name: 'Invoice',
		Id: 'invoiceId'
	};

	bts = {
		Name: 'BTS',
		Id: 'btsId'
	};

	skills1 = [channelSales, phone];
	skills2 = [invoice, bts];
	allSkills = [channelSales, phone, invoice, bts];

	skillArea1 = {
		Id: 'skillArea1Id',
		Name: 'SkillArea1',
		Skills: skills1
	};
	skillArea2 = {
		Id: 'skillArea2Id',
		Name: 'SkillArea2',
		Skills: skills2
	};
	skillAreas = [skillArea1, skillArea2];

	it('should go to sites by skill state', function (t) {
		allSkills.forEach(function (skill) {
			t.backend.withSkill(skill);
		});
		var vm = t.createController();

		t.apply(function () {
			vm.selectSkillOrSkillArea(vm.skills[0]);
		});

		expect(t.lastGoParams.skillIds).toEqual(['channelSalesId']);
	});

	it('should go to sites by skill area state', function (t) {
		t.backend.withSkillAreas(skillAreas);
		var vm = t.createController();

		vm.selectSkillOrSkillArea(vm.skillAreas[0]);

		expect(t.lastGoParams.skillAreaId).toEqual('skillArea1Id');
	});

	it('should go to sites with skill when changing selection from skill area to skill', function (t) {
		t.stateParams.skillAreaId = 'skillArea1Id';
		allSkills.forEach(function (skill) {
			t.backend.withSkill(skill);
		});
		t.backend.withSkillAreas(skillAreas);
		var vm = t.createController();

		t.apply(function () {
			vm.selectSkillOrSkillArea(vm.skills[0]);
		});

		expect(t.lastGoParams.skillAreaId).toEqual(undefined);
		expect(t.lastGoParams.skillIds).toEqual(['channelSalesId']);
	});

	it('should go to sites with skill area when changing selection from skill to skill area', function (t) {
		t.stateParams.skillIds = ['channelSalesId'];
		allSkills.forEach(function (skill) {
			t.backend.withSkill(skill);
		});
		t.backend.withSkillAreas(skillAreas);
		var vm = t.createController();

		t.apply(function () {
			vm.selectSkillOrSkillArea(skillArea1);
		});

		expect(t.lastGoParams.skillAreaId).toEqual('skillArea1Id');
		expect(t.lastGoParams.skillIds).toEqual(undefined);
	});

	it('should clear url when sending in empty input in filter', function (t) {
		t.stateParams.skillIds = ['channelSalesId'];
		allSkills.forEach(function (skill) {
			t.backend.withSkill(skill);
		});
		t.backend.withSkillAreas(skillAreas);
		var vm = t.createController();

		t.apply(function () {
			vm.selectSkillOrSkillArea(undefined);
		});

		expect(t.lastGoParams.hasOwnProperty('skillAreaId')).toEqual(true);
		expect(t.lastGoParams.skillAreaId).toEqual(undefined);
		expect(t.lastGoParams.hasOwnProperty('skillIds')).toEqual(true);
		expect(t.lastGoParams.skillIds).toEqual(undefined);
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
		t.backend.withSkillAreas(skillAreas);
		t.backend
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
		allSkills.forEach(function (skill) {
			t.backend.withSkill(skill);
		});
		t.backend
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
		allSkills.forEach(function (skill) {
			t.backend.withSkill(skill);
		});
		t.backend
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
		t.backend.withSkillAreas(skillAreas);
		t.backend
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
			})
		;
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
		t.backend.withSkillAreas(skillAreas);
		t.backend
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
			})
		;
		var vm = t.createController(undefined, skillAreas);

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

