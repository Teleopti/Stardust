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

	it('should display agents out of adherence in team for preselected skill', function () {
		stateParams.skillIds = "emailGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend.withTeamAdherence({
			SiteId: "parisGuid",
			Id: "parisTeamGreenGuid",
			NumberOfAgents: 10,
			OutOfAdherence: 5,
			SkillId: "phoneGuid",
			Color: "warning"
		})
			.withTeamAdherence({
				SiteId: "parisGuid",
				Id: "parisTeamRedGuid",
				NumberOfAgents: 8,
				OutOfAdherence: 2,
				SkillId: "emailGuid",
				Color: "good"
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.teams.length).toEqual(1);
		expect(vm.teams[0].Id).toEqual("parisTeamRedGuid");
		expect(vm.teams[0].OutOfAdherence).toEqual(2);
		expect(vm.teams[0].Color).toEqual("good");
	});

	it('should update adherence for team and preselected skill', function () {
		stateParams.skillIds = "phoneGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeamAdherence({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				NumberOfAgents: 10,
				OutOfAdherence: 5,
				SkillId: "phoneGuid",
				Color: "danger"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			$fakeBackend
				.clearTeamAdherences()
				.withTeamAdherence({
					SiteId: "parisGuid",
					Id: "parisTeamGreenGuid",
					NumberOfAgents: 10,
					OutOfAdherence: 2,
					SkillId: "phoneGuid",
					Color: "good"
				})
		})
			.wait(5000);

		expect(vm.teams[0].OutOfAdherence).toEqual(2);
		expect(vm.teams[0].Color).toEqual("good");
	});

	it('should display agents out of adherence in teams for preselected skill area', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeamAdherence({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				NumberOfAgents: 10,
				OutOfAdherence: 7,
				SkillId: "phoneGuid",
				Color: "warning"
			})
			.withTeamAdherence({
				SiteId: "parisGuid",
				Id: "parisTeamRedGuid",
				NumberOfAgents: 11,
				OutOfAdherence: 7,
				SkillId: "emailGuid",
				Color: "danger"
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
		expect(vm.teams[0].Color).toEqual("warning");
		expect(vm.teams[1].Color).toEqual("danger");
	});

	it('should update adherence for team and preselected skill area', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeamAdherence({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				NumberOfAgents: 10,
				OutOfAdherence: 5,
				SkillId: "phoneGuid",
				Color: "danger"
			})
			.withTeamAdherence({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				NumberOfAgents: 10,
				OutOfAdherence: 2,
				SkillId: "emailGuid",
				Color: "good"
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
			$fakeBackend.clearTeamAdherences()
				.withTeamAdherence({
					SiteId: "parisGuid",
					Id: "parisTeamGreenGuid",
					NumberOfAgents: 10,
					OutOfAdherence: 3,
					SkillId: "phoneGuid",
					Color: "good"
				})
		})
			.wait(5000);

		expect(vm.teams[0].OutOfAdherence).toEqual(3);
		expect(vm.teams[0].Color).toEqual("good");
	});
});
