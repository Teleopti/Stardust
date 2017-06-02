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

	it('should display agents out of adherence in sites for preselected skill', function () {
		stateParams.skillIds = "emailGuid";
		$fakeBackend.withSiteAdherenceForSkill({
			Id: "parisGuid",
			NumberOfAgents: 11,
			OutOfAdherence: 5,
			SkillId: "emailGuid",
			Color: "warning"
		});

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites.length).toEqual(1);
		expect(vm.sites[0].Id).toEqual("parisGuid");
		expect(vm.sites[0].OutOfAdherence).toEqual(5);
		expect(vm.sites[0].Color).toEqual("warning");
	});

	it('should display agents out of adherence in sites for preselected skill area', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		$fakeBackend.withSiteAdherenceForSkill({
			Id: "londonGuid",
			NumberOfAgents: 11,
			OutOfAdherence: 1,
			SkillId: "phoneGuid",
			Color: "good"
		})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				NumberOfAgents: 10,
				OutOfAdherence: 3,
				SkillId: "emailGuid",
				Color: "good"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				NumberOfAgents: 10,
				OutOfAdherence: 2,
				SkillId: "phoneGuid",
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

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites.length).toEqual(2);
		expect(vm.sites[0].Id).toEqual("londonGuid");
		expect(vm.sites[0].OutOfAdherence).toEqual(1);
		expect(vm.sites[0].Color).toEqual("good");
		expect(vm.sites[1].Id).toEqual("parisGuid");
		expect(vm.sites[1].OutOfAdherence).toEqual(5);
		expect(vm.sites[1].Color).toEqual("warning");
	});

	it('should update adherence for site and preselected skill', function () {
		stateParams.skillIds = "phoneGuid";
		$fakeBackend
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				NumberOfAgents: 10,
				OutOfAdherence: 1,
				SkillId: "phoneGuid",
				Color: "good"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			$fakeBackend.clearSiteAdherencesForSkill()
				.withSiteAdherenceForSkill({
					Id: "londonGuid",
					NumberOfAgents: 10,
					OutOfAdherence: 5,
					SkillId: "phoneGuid",
					Color: "warning"
				})
		})
			.wait(5000);

		expect(vm.sites[0].OutOfAdherence).toEqual(5);
		expect(vm.sites[0].Color).toEqual("warning");
	});

	it('should update adherence for site and preselected skill area', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		$fakeBackend.withSiteAdherenceForSkill({
			Id: "londonGuid",
			NumberOfAgents: 10,
			OutOfAdherence: 2,
			SkillId: "phoneGuid",
			Color: "good"
		})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				NumberOfAgents: 10,
				OutOfAdherence: 3,
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
			$fakeBackend.clearSiteAdherencesForSkill()
				.withSiteAdherenceForSkill({
					Id: "londonGuid",
					NumberOfAgents: 10,
					OutOfAdherence: 5,
					SkillId: "phoneGuid",
					Color: "warning"
				})
				.withSiteAdherenceForSkill({
					Id: "londonGuid",
					NumberOfAgents: 10,
					OutOfAdherence: 4,
					SkillId: "emailGuid",
					Color: "warning"
				})
		})
			.wait(5000);

		expect(vm.sites[0].OutOfAdherence).toEqual(9);
		expect(vm.sites[0].Color).toEqual("danger");
	});
});
