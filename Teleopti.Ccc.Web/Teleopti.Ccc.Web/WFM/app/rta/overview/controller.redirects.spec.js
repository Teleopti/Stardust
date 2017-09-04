'use strict';

describe('RtaOverviewController redirects', function () {

	var
		$controllerBuilder,
		$fakeBackend,
		$httpBackend,
		$interval,
		$state;

	var
		stateParams,
		scope,
		vm;

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

	var lastGoParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.factory('$stateParams', function () {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(function () {
		module(function ($provide) {
			$provide.value('skills', function () {
				return allSkills;
			});
		});
	});

	beforeEach(function () {
		module(function ($provide) {
			$provide.value('skillAreas', function () {
				return skillAreas;
			});
		});
	});

	beforeEach(inject(function (_FakeRtaBackend_, _ControllerBuilder_, _$httpBackend_, _$interval_, _$state_) {
		$controllerBuilder = _ControllerBuilder_;
		$fakeBackend = _FakeRtaBackend_;
		$httpBackend = _$httpBackend_;
		$interval = _$interval_;
		$state = _$state_;

		scope = $controllerBuilder.setup('RtaOverviewController39082');

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

		$fakeBackend.clear();
		spyOn($state, 'go').and.callFake(function (_, params) {
			lastGoParams = params;
		});
	}));

	it('should go to sites by skill state', function () {
		var c = $controllerBuilder.createController(allSkills);
		var vm = c.vm;

		c.apply(function () {
			vm.selectSkillOrSkillArea(vm.skills[0]);
		});

		expect(lastGoParams.skillIds).toEqual('channelSalesId');
	});

	it('should go to sites by skill area state', function () {
		vm = $controllerBuilder.createController(undefined, skillAreas).vm;

		vm.selectSkillOrSkillArea(vm.skillAreas[0]);

		expect(lastGoParams.skillAreaId).toEqual('skillArea1Id');
	});

	it('should go to sites with skill when changing selection from skill area to skill', function () {
		stateParams.skillAreaId = 'skillArea1Id';
		var c = $controllerBuilder.createController(allSkills, skillAreas);
		vm = c.vm;

		c.apply(function () {
			vm.selectSkillOrSkillArea(vm.skills[0]);
		});

		expect(lastGoParams.skillAreaId).toEqual(undefined);
		expect(lastGoParams.skillIds).toEqual('channelSalesId');
	});

	it('should go to sites with skill area when changing selection from skill to skill area', function () {
		stateParams.skillIds = ['channelSalesId'];
		var c = $controllerBuilder.createController(allSkills, skillAreas)
		vm = c.vm

		c.apply(function () {
			vm.selectSkillOrSkillArea(skillArea1);
		});

		expect(lastGoParams.skillAreaId).toEqual('skillArea1Id');
		expect(lastGoParams.skillIds).toEqual(undefined);
	});

	it('should clear url when sending in empty input in filter', function () {
		stateParams.skillIds = ['channelSalesId'];
		var c = $controllerBuilder.createController()
		vm = c.vm;

		c.apply(function () {
			vm.selectSkillOrSkillArea(undefined);
		});

		expect(lastGoParams.skillAreaId).toEqual(undefined);
		expect(lastGoParams.skillIds).toEqual(undefined);
	});

	it('should go to agents for selected site', function () {
		$fakeBackend
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
		var c = $controllerBuilder.createController();
		vm = c.vm;

		c.apply(function () {
			vm.siteCards[0].isSelected = true;
		});
		c.apply(function () {
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta-agents', { siteIds: ['parisId'], teamIds: [], skillIds: [], skillAreaId: undefined });
	});

	it('should go to agents for the right selected site after deselecting some other', function () {
		$fakeBackend
			.withSiteAdherence({
				Id: 'parisId'
			})
			.withSiteAdherence({
				Id: 'londonId'
			});
		var c = $controllerBuilder.createController();
		vm = c.vm;

		c.apply(function () {
			vm.siteCards[0].isSelected = true;
		});
		c.apply(function () {
			vm.siteCards[1].isSelected = true;
		});
		c.apply(function () {
			vm.siteCards[1].isSelected = false;
		});
		$state.go.calls.reset();
		c.apply(function () {
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta-agents', { siteIds: ['parisId'], teamIds: [], skillIds: [], skillAreaId: undefined });
	});

	it('should go to agents for selected site with preselected skill', function () {
		stateParams.skillIds = ['channelSalesId'];
		$fakeBackend
			.withSiteAdherence({
				Id: 'parisId',
				SkillId: 'channelSalesId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				Id: 'redId',
				SkillId: 'channelSalesId'
			});
		var c = $controllerBuilder.createController();
		vm = c.vm;

		c.apply(function () {
			vm.siteCards[0].isSelected = true;
		});
		c.apply(function () {
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta-agents', { siteIds: ['parisId'], teamIds: [], skillIds: ['channelSalesId'], skillAreaId: undefined });
	});

	it('should go to agents for selected site with  skill area', function () {
		stateParams.skillAreaId = 'skillArea1Id';
		$fakeBackend
			.withSiteAdherence({
				Id: 'parisId',
				SkillId: 'channelSalesId'
			})
			.withSiteAdherence({
				Id: 'parisId',
				SkillId: 'phoneId'
			});
		var c = $controllerBuilder.createController(undefined, skillAreas);
		vm = c.vm;

		c.apply(function () {
			vm.siteCards[0].isSelected = true;
		});
		c.apply(function () {
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta-agents', { siteIds: ['parisId'], teamIds: [], skillIds: [], skillAreaId: 'skillArea1Id' });
	});

	it('should go to agents for selected team', function () {
		$fakeBackend
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
		var c = $controllerBuilder.createController();
		vm = c.vm;

		c.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		c.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		c.apply(function () {
			vm.goToAgents();
		});


		expect($state.go).toHaveBeenCalledWith('rta-agents', { siteIds: [], teamIds: ['greenId'], skillIds: [], skillAreaId: undefined });
	});

	it('should go to agents for selected team with selected skill', function () {
		stateParams.skillIds = ['channelSalesId'];
		$fakeBackend
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
		var c = $controllerBuilder.createController();
		vm = c.vm;

		c.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		c.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		$state.go.calls.reset();
		c.apply(function () {
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta-agents', { siteIds: [], teamIds: ['greenId'], skillIds: ['channelSalesId'], skillAreaId: undefined });
	});

	it('should go to agents for selected team with preselected skill', function () {
		stateParams.skillIds = ['channelSalesId'];
		$fakeBackend
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
		var c = $controllerBuilder.createController();
		vm = c.vm;

		c.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		c.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		c.apply(function () {
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta-agents', { siteIds: [], teamIds: ['greenId'], skillIds: ['channelSalesId'], skillAreaId: undefined });
	});

	it('should go to agents for selected team with selected skill area', function () {
		stateParams.skillAreaId = 'skillArea1Id';
		$fakeBackend
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
		var c = $controllerBuilder.createController(undefined, skillAreas);
		vm = c.vm;

		c.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		c.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		$state.go.calls.reset();
		c.apply(function () {
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta-agents', { siteIds: [], teamIds: ['greenId'], skillIds: [], skillAreaId: 'skillArea1Id' });
	});

	it('should go to agents for selected team with preselected skill area', function () {
		stateParams.skillAreaId = 'skillArea1Id';
		$fakeBackend
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
		var c = $controllerBuilder.createController(undefined, skillAreas);
		vm = c.vm;
		c.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		c.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		c.apply(function () {
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta-agents', { siteIds: [], teamIds: ['greenId'], skillIds: [], skillAreaId: 'skillArea1Id' });
	});

	it('should go to agents for site if all teams under it are selected one by one', function () {
		$fakeBackend
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
		var c = $controllerBuilder.createController();
		vm = c.vm;

		c.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		c.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		c.apply(function () {
			vm.siteCards[0].teams[1].isSelected = true;
		});
		c.apply(function () {
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta-agents', { siteIds: ['londonId'], teamIds: [], skillIds: [], skillAreaId: undefined });
	});

	it('should go to agents for teams if all teams under a site were selected and then one was unselected', function () {
		$fakeBackend
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
		var c = $controllerBuilder.createController();
		vm = c.vm;

		c.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		c.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		c.apply(function () {
			vm.siteCards[0].teams[1].isSelected = true;
		});
		c.apply(function () {
			vm.siteCards[0].teams[0].isSelected = false;
		});
		$state.go.calls.reset();
		c.apply(function () {
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta-agents', { siteIds: [], teamIds: ['redId'], skillIds: [], skillAreaId: undefined });
	});

	it('should go to agents for teams if site were selected and then one was unselected', function () {
		$fakeBackend
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
		var c = $controllerBuilder.createController();
		vm = c.vm;

		c.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		c.apply(function () {
			vm.siteCards[0].isSelected = true;
		});
		c.apply(function () {
			vm.siteCards[0].teams[1].isSelected = false;
		});
		$state.go.calls.reset();
		c.apply(function () {
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta-agents', { siteIds: [], teamIds: ['greenId'], skillIds: [], skillAreaId: undefined });
	});

	it('should always clear teams selection for teams under site if site is selected', function () {
		$fakeBackend
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
		var c = $controllerBuilder.createController();
		vm = c.vm;

		c.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		c.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		c.apply(function () {
			vm.siteCards[0].teams[1].isSelected = true;
		});
		c.apply(function () {
			vm.siteCards[0].teams[0].isSelected = false;
		});
		c.apply(function () {
			vm.siteCards[0].isSelected = true;
		});
		c.apply(function () {
			vm.goToAgents();
		});

		expect($state.go).toHaveBeenCalledWith('rta-agents', { siteIds: ['londonId'], teamIds: [], skillIds: [], skillAreaId: undefined });
	});

	it('should select teams under site when site is not selected and team is', function () {
		$fakeBackend
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
		var c = $controllerBuilder.createController();
		vm = c.vm;

		c.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		c.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		c.apply(function () {
			vm.siteCards[0].isOpen = false;
		});
		c.apply(function () {
			vm.siteCards[0].isOpen = true;
		});

		expect(vm.siteCards[0].teams[0].isSelected).toEqual(true);
	});

	it('should select all teams under site when site is selected and initially closed', function () {
		$fakeBackend
			.withSiteAdherence({
				Id: 'londonId'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'greenId'
			});

		var c = $controllerBuilder.createController()
		vm = c.vm;

		c.apply(function () {
			vm.siteCards[0].isSelected = true;
		});
		c.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		
		expect(vm.siteCards[0].teams[0].isSelected).toEqual(true);
	});

	it('should go to agents team and site in cross site selection where one site has only one team', function () {
		$fakeBackend
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
		var c = $controllerBuilder.createController();
		vm = c.vm;

		c.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		c.apply(function () {
			vm.siteCards[0].teams[0].isSelected = true;
		});
		c.apply(function () {
			vm.siteCards[1].isOpen = true;
		});
		c.apply(function () {
			vm.siteCards[1].teams[0].isSelected = true;
		});
		c.apply(function () {
			vm.goToAgents();
		});

		//expect(vm.selectedItems).toEqual({ siteIds: ['parisId'], teamIds: ['greenId'], skillIds: [], skillAreaId: undefined });
		expect($state.go).toHaveBeenCalledWith('rta-agents', { siteIds: ['parisId'], teamIds: ['greenId'], skillIds: [], skillAreaId: undefined });

	});

});

