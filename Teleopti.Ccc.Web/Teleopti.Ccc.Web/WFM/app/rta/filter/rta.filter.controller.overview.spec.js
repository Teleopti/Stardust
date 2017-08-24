'use strict';
describe('RtaFilterController', function () {
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

		scope = $controllerBuilder.setup('RtaFilterController');
		$fakeBackend.clear();
		spyOn($state, 'go');
	}));

	it('should have preselected skill in field for sites', function () {
		stateParams.skillIds = "phoneGuid";
		$fakeBackend
			.withSiteAdherence({
				Id: "londonGuid",
				InAlarmCount: 3,
				SkillId: "phoneGuid"
			})
			.withSkill({
				Id: "phoneGuid",
				Name: "Phone"
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.selectedSkill.Id).toEqual('phoneGuid');
		expect(vm.selectedSkill.Name).toEqual('Phone');
	});

	it('should have preselected skill area in field for sites', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		$fakeBackend
			.withSiteAdherence({
				Id: "londonGuid",
				InAlarmCount: 3,
				SkillId: "phoneGuid"
			})
			.withSkillAreas([{
				Id: "emailAndPhoneGuid",
				Name: "Email and phone",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				}]
			}]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.selectedSkillArea.Id).toEqual('emailAndPhoneGuid');
		expect(vm.selectedSkillArea.Name).toEqual('Email and phone');
	});

	it('should have preselected skill in field for teams', function () {
		stateParams.skillIds = "phoneGuid";
		stateParams.siteIds = "parisGuid";
		$fakeBackend
			.withTeamAdherence({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 3,
				SkillId: "phoneGuid"
			})
			.withSkill({
				Id: "phoneGuid",
				Name: "Phone"
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.selectedSkill.Id).toEqual('phoneGuid');
		expect(vm.selectedSkill.Name).toEqual('Phone');
	});

	it('should have preselected skill area in field for teams', function () {
		stateParams.skillAreaId = "emailAndPhoneGuid";
		stateParams.siteIds = ["parisGuid"];
		$fakeBackend
			.withTeamAdherence({
				SiteId: "parisGuid",
				Id: "parisTeamGreenGuid",
				OutOfAdherence: 3,
				SkillId: "phoneGuid"
			})
			.withSkillAreas([{
				Id: "emailAndPhoneGuid",
				Name: "Email and phone",
				Skills: [{
					Id: "phoneGuid"
				}, {
					Id: "emailGuid"
				}]
			}]);

		vm = $controllerBuilder.createController().vm;

		expect(vm.selectedSkillArea.Id).toEqual('emailAndPhoneGuid');
		expect(vm.selectedSkillArea.Name).toEqual('Email and phone');
	});
});
