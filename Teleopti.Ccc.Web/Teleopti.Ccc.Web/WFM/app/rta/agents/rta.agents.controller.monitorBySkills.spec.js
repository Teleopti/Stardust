'use strict';
describe('RtaAgentsController', function() {
	var $interval,
		$httpBackend,
		$state,
		$fakeBackend,
		$controllerBuilder,
		scope,
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

	beforeEach(inject(function(_$httpBackend_, _$interval_, _$state_, _FakeRtaBackend_, _ControllerBuilder_) {
		$interval = _$interval_;
		$state = _$state_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;

		$fakeBackend.clear();

		scope = $controllerBuilder.setup('RtaAgentsController');
		spyOn($state, 'go');
	}));

	it('should get agent for skill', function() {
		stateParams.skillIds = ["f08d75b3-fdb4-484a-ae4c-9f0800e2f753"];
		$fakeBackend.withAgentState({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
	});

	it('should get state for skill', function() {
		stateParams.skillIds = ["f08d75b3-fdb4-484a-ae4c-9f0800e2f753"];
		$fakeBackend.withAgentState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
				State: "Ready"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].State).toEqual("Ready");
	});

	it('should state in alarm for skill', function() {
		stateParams.skillIds = ["f08d75b3-fdb4-484a-ae4c-9f0800e2f753"];
		$fakeBackend.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
				State: "Break",
				TimeInAlarm: 0
			})
			.withAgentState({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
				State: "Break",
				TimeInAlarm: 60
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true);

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].Name).toEqual("Charley Caper");
		expect(vm.agentStates[0].State).toEqual("Break");
	});


	it('should get agent for skill area', function() {
		stateParams.skillAreaId = "f08d75b3-fdb4-484a-ae4c-9f0800e2f753";

		$fakeBackend
			.withSkillAreas([{
				Id: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
				Name: "my skill area 2",
				Skills: [{
					Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
					Name: "Phone"
				}]
			}])
			.withAgentState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "d08d75b3-fdb4-484a-ae4c-9f0800e2f753"
			})
			.withAgentState({
				PersonId: "22610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "5f15b334-22d1-4bc1-8e41-72359805d30f"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].PersonId).toEqual("22610fe4-0130-4568-97de-9b5e015b2564");
	});

	it('should get state for skill area', function() {
		stateParams.skillAreaId = "f08d75b3-fdb4-484a-ae4c-9f0800e2f753";
		$fakeBackend
			.withSkillAreas([{
				Id: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
				Name: "my skill area 2",
				Skills: [{
					Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
					Name: "Phone"
				}]
			}])
			.withAgentState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "5f15b334-22d1-4bc1-8e41-72359805d30f",
				State: "Ready"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].State).toEqual("Ready");
	});

	it('should get state in alarm for skill area', function() {
		stateParams.skillAreaId = "f08d75b3-fdb4-484a-ae4c-9f0800e2f753";
		$fakeBackend
			.withSkillAreas([{
				Id: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
				Name: "my skill area 2",
				Skills: [{
					Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
					Name: "Phone"
				}]
			}])
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "5f15b334-22d1-4bc1-8e41-72359805d30f",
				State: "Break"
			})
			.withAgentState({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				SkillId: "5f15b334-22d1-4bc1-8e41-72359805d30f",
				State: "Break",
				TimeInAlarm: 60
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true);

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].Name).toEqual("Charley Caper");
		expect(vm.agentStates[0].State).toEqual("Break");
	});
});
