'use strict';
describe('RtaOverviewController', function() {
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
	beforeEach(function() {
		module(function($provide) {
			$provide.factory('$stateParams', function() {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function(_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_, _$timeout_) {
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

	it('should display agents out of adherence in sites for preselected skill', function() {
		stateParams.skillIds = "emailGuid";
		$fakeBackend
			.withSite({
				Id: "parisGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 5,
				SkillId: "emailGuid"
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites.length).toEqual(1);
		expect(vm.sites[0].Id).toEqual("parisGuid");
		expect(vm.sites[0].OutOfAdherence).toEqual(5);
	});

	it('should display agents out of adherence in sites for preselected skill area', function() {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		$fakeBackend
			.withSite({
				Id: "londonGuid"
			})
			.withSite({
				Id: "parisGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				OutOfAdherence: 1,
				SkillId: "phoneGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 3,
				SkillId: "emailGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "parisGuid",
				OutOfAdherence: 2,
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

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites.length).toEqual(2);
		expect(vm.sites[0].Id).toEqual("londonGuid");
		expect(vm.sites[0].OutOfAdherence).toEqual(1);
		expect(vm.sites[1].Id).toEqual("parisGuid");
		expect(vm.sites[1].OutOfAdherence).toEqual(5);
	});

	it('should update adherence for site and preselected skill', function() {
		stateParams.skillIds = "phoneGuid";
		$fakeBackend
			.withSite({
				Id: "londonGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				OutOfAdherence: 1,
				SkillId: "phoneGuid"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
				$fakeBackend.clearSiteAdherencesForSkill()
					.withSiteAdherenceForSkill({
						Id: "londonGuid",
						OutOfAdherence: 3,
						SkillId: "phoneGuid"
					})
			})
			.wait(5000);

		expect(vm.sites[0].OutOfAdherence).toEqual(3);
	});

	it('should update adherence for site and preselected skill area', function() {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		$fakeBackend
			.withSite({
				Id: "londonGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				OutOfAdherence: 1,
				SkillId: "phoneGuid"
			})
			.withSiteAdherenceForSkill({
				Id: "londonGuid",
				OutOfAdherence: 5,
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
		c.apply(function() {
				$fakeBackend.clearSiteAdherencesForSkill()
					.withSiteAdherenceForSkill({
						Id: "londonGuid",
						OutOfAdherence: 3,
						SkillId: "phoneGuid"
					})
					.withSiteAdherenceForSkill({
						Id: "londonGuid",
						OutOfAdherence: 4,
						SkillId: "emailGuid"
					})
			})
			.wait(5000);

		expect(vm.sites[0].OutOfAdherence).toEqual(7);
	});
});
