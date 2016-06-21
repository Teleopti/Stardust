'use strict';
describe('RtaAgentsCtrlMonitorBySkills_39081', function () {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		scope,
		NoticeService;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function() {
		module(function($provide) {
			$provide.service('$stateParams', function() {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function(_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_, _NoticeService_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		NoticeService = _NoticeService_;
		$controllerBuilder = _ControllerBuilder_;

		$fakeBackend.clear();
		$fakeBackend.withToggle('RTA_MonitorBySkills_39081');

		scope = $controllerBuilder.setup('RtaAgentsCtrl');

	}));

	it('should display skill name', function () {
		stateParams.skillId = "f08d75b3-fdb4-484a-ae4c-9f0800e2f753";
		$fakeBackend
			.withSkill({
				Name: "Phone",
				SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			});

		$controllerBuilder.createController();

		expect(scope.skillName).toEqual("Phone");
	});
	
	it('should get agent for skill', function () {
		stateParams.skillId = "f08d75b3-fdb4-484a-ae4c-9f0800e2f753";
		$fakeBackend.withAgent({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
		});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
	});

	it('should get state for skill', function () {
		stateParams.skillId = "f08d75b3-fdb4-484a-ae4c-9f0800e2f753";
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Ready"
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].State).toEqual("Ready");
	});

	it('should state in alarm for skill', function () {
		stateParams.skillId = "f08d75b3-fdb4-484a-ae4c-9f0800e2f753";
		$fakeBackend.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Break",
				TimeInAlarm: 0
			})
			.withAgent({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			})
			.withState({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				State: "Break",
				TimeInAlarm: 60
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = true');

		expect(scope.filteredData.length).toEqual(1);
		expect(scope.filteredData[0].Name).toEqual("Charley Caper");
		expect(scope.filteredData[0].State).toEqual("Break");
	});

});
