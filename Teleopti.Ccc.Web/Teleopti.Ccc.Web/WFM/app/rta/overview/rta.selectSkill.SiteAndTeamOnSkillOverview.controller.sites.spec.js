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
		$fakeBackend.withSiteAdherence({
			Id: "parisGuid",
			AgentsCount: 11,
			InAlarmCount: 5,
			SkillId: "emailGuid",
			Color: "warning"
		});

		vm = $controllerBuilder.createController().vm;

		expect(vm.sites.length).toEqual(1);
		expect(vm.sites[0].Id).toEqual("parisGuid");
		expect(vm.sites[0].InAlarmCount).toEqual(5);
		expect(vm.sites[0].Color).toEqual("warning");
	});

	it('should display agents out of adherence in sites for preselected skill area', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		$fakeBackend.withSiteAdherence({
			Id: "londonGuid",
			InAlarmCount: 1,
			SkillId: "phoneGuid",
			Color: "good"
		})
			.withSiteAdherence({
				Id: "parisGuid",
				InAlarmCount: 5,
				SkillId: "emailGuid",
				Color: "warning"
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
		expect(vm.sites[0].InAlarmCount).toEqual(1);
		expect(vm.sites[0].Color).toEqual("good");
		expect(vm.sites[1].Id).toEqual("parisGuid");
		expect(vm.sites[1].InAlarmCount).toEqual(5);
		expect(vm.sites[1].Color).toEqual("warning");
	});

	it('should update adherence for site and preselected skill', function () {
		stateParams.skillIds = "phoneGuid";
		$fakeBackend
			.withSiteAdherence({
				Id: "londonGuid",
				InAlarmCount: 1,
				SkillId: "phoneGuid",
				Color: "good"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function () {
			$fakeBackend.clearSiteAdherences()
				.withSiteAdherence({
					Id: "londonGuid",
					InAlarmCount: 5,
					SkillId: "phoneGuid",
					Color: "warning"
				})
		})
			.wait(5000);

		expect(vm.sites[0].InAlarmCount).toEqual(5);
		expect(vm.sites[0].Color).toEqual("warning");
	});

	it('should update adherence for site and preselected skill area', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		$fakeBackend.withSiteAdherence({
			Id: "londonGuid",
			InAlarmCount: 2,
			SkillId: "phoneGuid",
			Color: "good"
		})
			.withSiteAdherence({
				Id: "londonGuid",
				InAlarmCount: 3,
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
			$fakeBackend.clearSiteAdherences()
				.withSiteAdherence({
					Id: "londonGuid",
					InAlarmCount: 9,
					SkillId: "emailGuid",
					Color: "danger"
				})
		})
			.wait(5000);

		expect(vm.sites[0].InAlarmCount).toEqual(9);
		expect(vm.sites[0].Color).toEqual("danger");
	});
});
