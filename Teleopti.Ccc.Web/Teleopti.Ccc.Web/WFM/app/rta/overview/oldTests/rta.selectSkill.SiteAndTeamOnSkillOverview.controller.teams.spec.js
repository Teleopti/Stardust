'use strict';
describe('RtaOverviewController', function () {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		scope,
		$fakeBackend,
		$controllerBuilder,
		$timeout,
		vm;

	var stateParams = {};

	beforeEach(module('wfm.rta'));
	beforeEach(function () {
		module(function ($provide) {
			$provide.factory('$stateParams', function () {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_, _$timeout_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;
		$timeout = _$timeout_;

		scope = $controllerBuilder.setup('RtaOverviewController');

		$fakeBackend.clear();

		spyOn($state, 'go');
	}));

	//should include other site to test filtering it out?
		it('should display agents out of adherence in team for preselected skill', function () {
		stateParams.skillIds = "emailGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeam({
				Id: "parisTeamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeam({
				Id: "parisTeamRedGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "phoneGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamRedGuid",
				OutOfAdherence: 7,
				SkillId: "emailGuid"
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.teams.length).toEqual(1);
		expect(vm.teams[0].Id).toEqual("parisTeamRedGuid");
		expect(vm.teams[0].OutOfAdherence).toEqual(7);
	});

	it('should display agents out of adherence in teams for preselected skill area', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeam({
				Id: "parisTeamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeam({
				Id: "parisTeamRedGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "phoneGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 2,
				SkillId: "emailGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamRedGuid",
				OutOfAdherence: 7,
				SkillId: "emailGuid"
			})
			.withSkillAreas([{
				Id: "emailAndPhoneGuid",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				}]
			}]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.teams.length).toEqual(2);
		expect(vm.teams[0].Id).toEqual("parisTeamGreenGuid");
		expect(vm.teams[1].Id).toEqual("parisTeamRedGuid");
		expect(vm.teams[0].OutOfAdherence).toEqual(7);
		expect(vm.teams[1].OutOfAdherence).toEqual(7);
	});

	it('should update adherence for site and preselected skill', function () {
		stateParams.skillIds = "phoneGuid";
		$fakeBackend
			.withSite({
				Id: "parisGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 5,
				SkillId: "phoneGuid"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			$fakeBackend
				.clearSiteAdherencesForSkill()
				.withSiteAdherenceForSkill({
					Id: "parisGuid",
					OutOfAdherence: 3,
					SkillId: "phoneGuid"
				})
		})
			.wait(5000);

		expect(vm.sites[0].OutOfAdherence).toEqual(3);
	});

	it('should update adherence for site and preselected skill area', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		$fakeBackend
			.withSite({
				Id: "parisGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 5,
				SkillId: "phoneGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 2,
				SkillId: "emailGuid"
			})
			.withSkillAreas([{
				Id: "emailAndPhoneGuid",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				}]
			}]);

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			$fakeBackend.clearSiteAdherencesForSkill()
				.withSiteAdherenceForSkill({
					Id: "parisGuid",
					OutOfAdherence: 3,
					SkillId: "phoneGuid"
				})
				.withSiteAdherenceForSkill({
					Id: "parisGuid",
					OutOfAdherence: 4,
					SkillId: "emailGuid"
				})
		})
			.wait(5000);

		expect(vm.sites[0].OutOfAdherence).toEqual(7);
	});

	it('should update adherence for team and preselected skill', function () {
		stateParams.skillIds = "phoneGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeam({
				Id: "parisTeamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "phoneGuid"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			$fakeBackend
				.clearTeamAdherencesForSkill()
				.withTeamAdherenceForSkill({
					SiteId: "parisGuid",
					Id: "parisTeamGreenGuid",
					OutOfAdherence: 3,
					SkillId: "phoneGuid"
				})
		})
			.wait(5000);

		expect(vm.teams[0].OutOfAdherence).toEqual(3);
	});

	it('should update adherence for team and preselected skill area', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeam({
				Id: "parisTeamGreenGuid",
				SiteId: "parisGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 5,
				SkillId: "phoneGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 2,
				SkillId: "emailGuid"
			})
			.withTeamAdherenceForSkill({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 7,
				SkillId: "phoneGuid"
			})
			.withSkillAreas([{
				Id: "emailAndPhoneGuid",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				}]
			}]);

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			$fakeBackend.clearTeamAdherencesForSkill()
				.withTeamAdherenceForSkill({
					SiteId: "parisGuid",
					Id: "parisTeamGreenGuid",
					OutOfAdherence: 3,
					SkillId: "phoneGuid"
				})
				.withTeamAdherenceForSkill({
					SiteId: "parisGuid",
					Id: "parisTeamGreenGuid",
					OutOfAdherence: 4,
					SkillId: "emailGuid"
				})
		})
			.wait(5000);

		expect(vm.teams[0].OutOfAdherence).toEqual(7);
	});
});
