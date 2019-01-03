'use strict';

rtaTester.describe('RtaOverviewController', function (it, fit, xit) {
	var vm;
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

	it('should create controller with empty state', function (t) {
		t.stateParams.siteIds = undefined;
		t.stateParams.teamIds = undefined;
		t.stateParams.skillIds = undefined;
		t.stateParams.skillAreaId = undefined;
		t.stateParams.open = undefined;
		t.stateParams.es = undefined;

		expect(function () {
			t.createController()
		}).not.toThrow();
	});

	it('should get skills', function (t) {
		allSkills.forEach(function (skill) {
			t.backend.withSkill(skill);
		});
		var vm = t.createController();

		expect(vm.skills[0].Name).toEqual('Channel Sales');
		expect(vm.skills[0].Id).toEqual('channelSalesId');
		expect(vm.skills[1].Name).toEqual('Phone');
		expect(vm.skills[1].Id).toEqual('phoneId');
		expect(vm.skills[2].Name).toEqual('Invoice');
		expect(vm.skills[2].Id).toEqual('invoiceId');
		expect(vm.skills[3].Name).toEqual('BTS');
		expect(vm.skills[3].Id).toEqual('btsId');
	});

	it('should get skill areas', function (t) {
		t.backend.withSkillGroups(skillAreas);
		var vm = t.createController();

		expect(vm.skillAreas.length).toEqual(2);
		expect(vm.skillAreas[0].Id).toEqual('skillArea1Id');
		expect(vm.skillAreas[0].Name).toEqual('SkillArea1');
		expect(vm.skillAreas[0].Skills[0].Id).toEqual('channelSalesId');
		expect(vm.skillAreas[0].Skills[0].Name).toEqual('Channel Sales');
		expect(vm.skillAreas[0].Skills[1].Id).toEqual('phoneId');
		expect(vm.skillAreas[0].Skills[1].Name).toEqual('Phone');
		expect(vm.skillAreas[1].Id).toEqual('skillArea2Id');
		expect(vm.skillAreas[1].Name).toEqual('SkillArea2');
		expect(vm.skillAreas[1].Skills[0].Id).toEqual('invoiceId');
		expect(vm.skillAreas[1].Skills[0].Name).toEqual('Invoice');
		expect(vm.skillAreas[1].Skills[1].Id).toEqual('btsId');
		expect(vm.skillAreas[1].Skills[1].Name).toEqual('BTS');
	});

	it('should build site card view model', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				Name: 'London',
				AgentsCount: 11,
				InAlarmCount: 5,
				Color: 'warning'
			});

		var vm = t.createController();

		expect(vm.siteCards[0].Id).toEqual('londonId');
		expect(vm.siteCards[0].Name).toEqual('London');
		expect(vm.siteCards[0].AgentsCount).toEqual(11);
		expect(vm.siteCards[0].InAlarmCount).toEqual(5);
		expect(vm.siteCards[0].ClassesOnSelection).toEqual('warning-border');
		expect(vm.siteCards[0].isOpen).toEqual(false);
	});

	it('should set total number of agents', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				Name: 'London',
				AgentsCount: 11,
				InAlarmCount: 5,
				Color: 'warning'
			})
			.withSiteAdherence({
				Id: 'parisId',
				Name: 'Paris',
				AgentsCount: 10,
				InAlarmCount: 9,
				Color: 'warning'
			});

		var vm = t.createController();

		expect(vm.totalAgentsInAlarm).toEqual(14);
	});

	it('should build site card view model when open in url is true', function (t) {
		t.stateParams.open = 'true';
		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				Name: 'London',
				AgentsCount: 11,
				InAlarmCount: 5,
				Color: 'warning'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'greenId'
			});

		var vm = t.createController();

		expect(vm.siteCards[0].teams.length).toEqual(1);
		expect(vm.siteCards[0].teams[0].SiteId).toEqual('londonId');
		expect(vm.siteCards[0].teams[0].Id).toEqual('greenId');
	});

	it('should build site card view model with skill ids when open in url is true', function (t) {
		t.stateParams.open = 'true';
		t.stateParams.skillIds = ['phoneId'];

		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				Name: 'London',
				AgentsCount: 11,
				InAlarmCount: 5,
				SkillId: 'phoneId',
				Color: 'warning'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				SkillId: 'phoneId',
				Id: 'greenId'
			});

		var vm = t.createController();

		expect(vm.siteCards[0].teams.length).toEqual(1);
		expect(vm.siteCards[0].teams[0].SiteId).toEqual('londonId');
		expect(vm.siteCards[0].teams[0].Id).toEqual('greenId');
	});

	it('should update adherence for teams when open in url is true', function (t) {
		t.stateParams.open = 'true';
		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				Name: 'London',
				AgentsCount: 11,
				InAlarmCount: 5,
				Color: 'warning'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'greenId',
				InAlarmCount: 2,
				Color: 'good'
			});

		var vm = t.createController();
		t.apply(function () {
			t.backend
				.clearTeamAdherences()
				.withTeamAdherence({
					SiteId: 'londonId',
					Id: 'greenId',
					InAlarmCount: 6,
					Color: 'warning'
				});
		})
			.wait(5000);

		expect(vm.siteCards[0].teams[0].InAlarmCount).toEqual(6);
		expect(vm.siteCards[0].teams[0].Color).toEqual('warning');
	});

	it('should build site card view model when preselected skill', function (t) {
		t.stateParams.skillIds = ['phoneId'];
		allSkills.forEach(function (skill) {
			t.backend.withSkill(skill);
		});
		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				Name: 'London',
				SkillId: 'phoneId',
				AgentsCount: 11,
				InAlarmCount: 5,
				Color: 'warning'
			})
			.withSiteAdherence({
				Id: 'parisId',
				Name: 'Paris',
				SkillId: 'invoiceId',
				AgentsCount: 10,
				InAlarmCount: 5,
				Color: 'warning'
			});

		var vm = t.createController();

		expect(vm.siteCards.length).toEqual(1);
		expect(vm.siteCards[0].Id).toEqual('londonId');
		expect(vm.siteCards[0].Name).toEqual('London');
		expect(vm.siteCards[0].AgentsCount).toEqual(11);
		expect(vm.siteCards[0].InAlarmCount).toEqual(5);
		expect(vm.siteCards[0].isOpen).toEqual(false);
	});

	it('should build site card view model when preselected skill area', function (t) {
		t.stateParams.skillAreaId = 'skillArea1Id';
		t.backend
			.withSkillGroups(skillAreas)
			.withSiteAdherence({
				Id: 'londonId',
				SkillId: 'channelSalesId',
				Name: 'London',
				AgentsCount: 11,
				InAlarmCount: 2,
				Color: 'good'
			});

		var vm = t.createController();

		expect(vm.siteCards[0].Id).toEqual('londonId');
		expect(vm.siteCards[0].Name).toEqual('London');
		expect(vm.siteCards[0].AgentsCount).toEqual(11);
		expect(vm.siteCards[0].InAlarmCount).toEqual(2);
		expect(vm.siteCards[0].isOpen).toEqual(false);
	});

	it('should update adherence', function (t) {
		t.backend.withSiteAdherence({
			Id: 'londonId',
			Name: 'London',
			AgentsCount: 11,
			InAlarmCount: 5,
			Color: 'warning'
		});

		var vm = t.createController();

		t.apply(function () {
			t.backend
				.clearSiteAdherences()
				.withSiteAdherence({
					Id: 'londonId',
					Name: 'London',
					AgentsCount: 11,
					InAlarmCount: 2,
					Color: 'good'
				});
		})
			.wait(5000);

		expect(vm.siteCards[0].Id).toEqual('londonId');
		expect(vm.siteCards[0].Name).toEqual('London');
		expect(vm.siteCards[0].AgentsCount).toEqual(11);
		expect(vm.siteCards[0].InAlarmCount).toEqual(2);
		expect(vm.siteCards[0].isOpen).toEqual(false);
	});

	it('should update adherence for site with skill', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				SkillId: 'phoneId',
				InAlarmCount: 5,
				Color: 'warning'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.skillIds = ['phoneId'];
			t.backend
				.clearSiteAdherences()
				.withSiteAdherence({
					Id: 'londonId',
					SkillId: 'phoneId',
					InAlarmCount: 2,
					Color: 'good'
				})
		})
			.wait(5000);

		expect(vm.siteCards[0].InAlarmCount).toEqual(2);
	});

	it('should update adherence for site with skill area', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				SkillId: 'channelSalesId',
				InAlarmCount: 5,
				Color: 'warning'
			})
			.withSiteAdherence({
				Id: 'londonId',
				SkillId: 'phoneId',
				InAlarmCount: 4,
				Color: 'warning'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.skillIds = ['channelSalesId', 'phoneId'];
			t.backend
				.clearSiteAdherences()
				.withSiteAdherence({
					Id: 'londonId',
					SkillId: 'channelSalesId',
					InAlarmCount: 2,
					Color: 'good'
				})
		})
			.wait(5000);

		expect(vm.siteCards[0].InAlarmCount).toEqual(2);
	});

	it('should update adherence for site with preselected skill', function (t) {
		t.stateParams.skillIds = ['channelSalesId'];
		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				SkillId: 'channelSalesId',
				InAlarmCount: 5,
				Color: 'warning'
			});
		var vm = t.createController();

		t.apply(function () {
			t.backend
				.clearSiteAdherences()
				.withSiteAdherence({
					Id: 'londonId',
					SkillId: 'channelSalesId',
					InAlarmCount: 2,
					Color: 'good'
				})
		})
			.wait(5000);

		expect(vm.siteCards[0].InAlarmCount).toEqual(2);
	});

	it('should update adherence for site with preselected skill area', function (t) {
		t.stateParams.skillAreaId = 'skillArea1Id';
		t.backend
			.withSkillGroups(skillAreas)
			.withSiteAdherence({
				Id: 'londonId',
				SkillId: 'channelSalesId',
				InAlarmCount: 5,
				Color: 'warning'
			})
			.withSiteAdherence({
				Id: 'londonId',
				SkillId: 'phoneId',
				InAlarmCount: 4,
				Color: 'warning'
			});
		var vm = t.createController();

		t.apply(function () {
			t.backend
				.clearSiteAdherences()
				.withSiteAdherence({
					Id: 'londonId',
					SkillId: 'channelSalesId',
					InAlarmCount: 2,
					Color: 'good'
				})
		})
			.wait(5000);

		expect(vm.siteCards[0].InAlarmCount).toEqual(2);
	});

	it('should update adherence for site when clearing selection', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'londonId',
				SkillId: 'phoneId',
				Name: 'London',
				AgentsCount: 11,
				InAlarmCount: 5,
				Color: 'warning'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.skillIds = ['phoneId'];
			t.backend
				.clearSiteAdherences()
				.withSiteAdherence({
					Id: 'londonId',
					SkillId: 'phoneId',
					Name: 'London',
					AgentsCount: 11,
					InAlarmCount: 2,
					Color: 'good'
				})
		})
			.wait(5000);
		t.apply(function () {
			vm.skillIds = [];
			t.backend
				.clearSiteAdherences()
				.withSiteAdherence({
					Id: 'londonId',
					Name: 'London',
					AgentsCount: 11,
					InAlarmCount: 9,
					Color: 'danger'
				})
		})
			.wait(5000);

		expect(vm.siteCards[0].Id).toEqual('londonId');
		expect(vm.siteCards[0].Name).toEqual('London');
		expect(vm.siteCards[0].AgentsCount).toEqual(11);
		expect(vm.siteCards[0].InAlarmCount).toEqual(9);
		expect(vm.siteCards[0].ClassesOnSelection).toEqual('danger-border');
		expect(vm.siteCards[0].isOpen).toEqual(false);
	});

	it('should stop polling when page is about to destroy', function (t) {
		t.createController();
		t.wait(5000);

		t.destroyController();
		t.backend.lastOverviewSiteCardsRequestParams = undefined;
		t.wait(10000);
		expect(t.backend.lastOverviewSiteCardsRequestParams).toBeFalsy();
	});

	it('should update total agents in alarm', function (t) {
		t.backend.withSiteAdherence({
			Id: 'londonId',
			Name: 'London',
			AgentsCount: 11,
			InAlarmCount: 5,
			Color: 'warning'
		});

		var vm = t.createController();
		t.apply(function () {
			t.backend.clearSiteAdherences()
				.withSiteAdherence({
					Id: 'londonId',
					Name: 'London',
					AgentsCount: 11,
					InAlarmCount: 2,
					Color: 'good'
				});
		})
			.wait(5000);

		expect(vm.totalAgentsInAlarm).toEqual(2);
	});

	it('should have href for site', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'londonId'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'greenId'
			});

		var vm = t.createController();
		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});

		expect(vm.siteCards[0].href).toBe(t.href('rta-agents', {siteIds: ['londonId']}));
	});

	it('should have href for team (to SITE! A BUG!)', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'londonId'
			})
			.withTeamAdherence({
				SiteId: 'londonId',
				Id: 'greenId'
			});

		var vm = t.createController();
		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});

		expect(vm.siteCards[0].teams[0].href).toBe(t.href('rta-agents', {siteIds: ['londonId']}));
	});

	it('should update agents in alarm with missing team in organization', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'siteId'
			})
			.withTeamAdherence({
				SiteId: 'siteId',
				Id: 'team1Id',
				InAlarmCount: 1
			})
			.withTeamAdherence({
				SiteId: 'siteId',
				Id: 'team2Id',
				InAlarmCount: 2
			})
			.clearOrganization() // because withSiteAdherence/withTeamAdherence also adds to the organization :'(
			.withOrganization({
				Id: 'siteId',
				Teams: [{
					Id: 'team1Id',
				}]
			})
		;
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});

		expect(vm.siteCards[0].teams[1].InAlarmCount).toEqual(2);
	});

    it('should have href for team when new site and team (#79991)', function (t) {
        t.backend
            .withSiteAdherence({
                Id: 'londonId'
            })
            .withTeamAdherence({
                SiteId: 'londonId',
                Id: 'greenId'
            });
        var vm = t.createController();
        t.backend
            .withSiteAdherence({
                Id: 'parisId'
            })
            .withTeamAdherence({
                SiteId: 'parisId',
                Id: 'redId'
            });
        t.wait(5000);

        t.apply(function () {
            vm.siteCards[1].isOpen = true;
        });

        expect(vm.siteCards[1].teams[0].href).toBe(t.href('rta-agents', {siteIds: ['parisId']}));
    });
});