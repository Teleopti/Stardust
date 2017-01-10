'use strict';
describe('RtaAgentsController', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		scope,
		NoticeService,
		vm;

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

		scope = $controllerBuilder.setup('RtaAgentsController');

		spyOn($state, 'go');
	}));

	it('should get agent for skill', function() {
		stateParams.skillIds = ["f08d75b3-fdb4-484a-ae4c-9f0800e2f753"];
		$fakeBackend.withAgent({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
	});

	it('should get state for skill', function() {
		stateParams.skillIds = ["f08d75b3-fdb4-484a-ae4c-9f0800e2f753"];
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Ready"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].State).toEqual("Ready");
	});

	it('should state in alarm for skill', function() {
		stateParams.skillIds = ["f08d75b3-fdb4-484a-ae4c-9f0800e2f753"];
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

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = true);

		expect(vm.filteredData.length).toEqual(1);
		expect(vm.filteredData[0].Name).toEqual("Charley Caper");
		expect(vm.filteredData[0].State).toEqual("Break");
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
			.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "d08d75b3-fdb4-484a-ae4c-9f0800e2f753"
			})
			.withAgent({
				PersonId: "22610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "5f15b334-22d1-4bc1-8e41-72359805d30f"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].PersonId).toEqual("22610fe4-0130-4568-97de-9b5e015b2564");
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
			.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "5f15b334-22d1-4bc1-8e41-72359805d30f"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Ready"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].State).toEqual("Ready");
	});

	it('should state in alarm for skill area', function() {
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
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SkillId: "d08d75b3-fdb4-484a-ae4c-9f0800e2f753"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Break",
				TimeInAlarm: 60
			})
			.withAgent({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				SkillId: "5f15b334-22d1-4bc1-8e41-72359805d30f"
			})
			.withState({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				State: "Break",
				TimeInAlarm: 60
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = true);

		expect(vm.filteredData.length).toEqual(1);
		expect(vm.filteredData[0].Name).toEqual("Charley Caper");
		expect(vm.filteredData[0].State).toEqual("Break");
	});
});
