'use strict';
describe('RtaAgentsController', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		scope,
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

	beforeEach(inject(function(_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;

		scope = $controllerBuilder.setup('RtaAgentsController');

		$fakeBackend.clear();

		spyOn($state, 'go');
	}));

	it('should include state id in agent states', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "LoggedOut",
				StateId: '17560fe4-0130-4568-97de-9b5e015b2555',
				TimeInAlarm: 10
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = true);

		expect(vm.states[0].Name).toEqual('LoggedOut');
		expect(vm.states[0].Id).toEqual('17560fe4-0130-4568-97de-9b5e015b2555');
		expect(vm.states[0].Selected).toEqual(true);
	});

	it('should not have duplicate states', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			})
			.withAgent({
				PersonId: "22610fe4-0130-4568-97de-9b5e015b2577",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "LoggedOut",
				StateId: '17560fe4-0130-4568-97de-9b5e015b2555',
				TimeInAlarm: 15
			})
			.withState({
				PersonId: "22610fe4-0130-4568-97de-9b5e015b2577",
				State: "LoggedOut",
				StateId: '17560fe4-0130-4568-97de-9b5e015b2555',
				TimeInAlarm: 10
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = true);

		expect(vm.states.length).toEqual(1);
	});

	it('should have empty state', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "",
				StateId: null,
				TimeInAlarm: 15
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = true);

		expect(vm.states.length).toEqual(1)
		expect(vm.states[0].Name).toEqual("No State");
	});

	it('should not get information for No State (fakeBackend will throw if you do)', function() {
		stateParams.teamIds = ["teamGuid"];
		stateParams.es = ["noState"];
		$fakeBackend
			.withAgent({
				PersonId: "personGuid1",
				TeamId: "teamGuid",
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.states[0].Name).toEqual('No State');
		expect(vm.states[0].Selected).toEqual(false);
	});

	it('should order states by name', function() {
		stateParams.teamIds = ["teamGuid"];
		$fakeBackend.withAgent({
				PersonId: "personGuid1",
				TeamId: "teamGuid",
			})
			.withAgent({
				PersonId: "personGuid2",
				TeamId: "teamGuid",
			})
			.withState({
				PersonId: "personGuid1",
				State: "B",
				StateId: 'StateGuid1',
				TimeInAlarm: 15
			})
			.withState({
				PersonId: "personGuid2",
				State: "A",
				StateId: 'StateGuid2',
				TimeInAlarm: 10
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = true);

		expect(vm.states[0].Name).toEqual("A");
		expect(vm.states[1].Name).toEqual("B");
	});

	it('should order with stateParam', function() {
		stateParams.teamIds = ["teamGuid"];
		stateParams.es = ["guid2"];
		$fakeBackend.withAgent({
				PersonId: "personGuid1",
				TeamId: "teamGuid",
			})
			.withState({
				PersonId: "personGuid1",
				State: "B",
				StateId: 'guid1',
				TimeInAlarm: 15
			})
			.withPhoneState(({
				Name: "A",
				Id: "guid2"
			}));

		vm = $controllerBuilder.createController().vm;

		expect(vm.states[0].Name).toEqual("A");
		expect(vm.states[1].Name).toEqual("B");
	});

	it('should get names for hidden states sent through stateParams', function() {
		stateParams.teamIds = ["teamGuid"];
		stateParams.es = ["loggedOutGuid"];
		$fakeBackend
			.withPhoneState({
				Name: "LoggedOut",
				Id: "loggedOutGuid"
			})
			.withAgent({
				PersonId: "personGuid1",
				TeamId: "teamGuid",
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.states[0].Name).toEqual('LoggedOut');
		expect(vm.states[0].Id).toEqual('loggedOutGuid');
		expect(vm.states[0].Selected).toBeFalsy();
	});

	it('should still get phone state information when deselect multiple from stateParam', function() {
		stateParams.teamIds = ["teamGuid"];
		stateParams.es = ["noState", "loggedOutGuid"];
		$fakeBackend
			.withPhoneState({
				Name: "LoggedOut",
				Id: "loggedOutGuid"
			})
			.withAgent({
				PersonId: "personGuid1",
				TeamId: "teamGuid",
			});

		vm = $controllerBuilder.createController().vm;

		var result = vm.states.filter(function(s) {
			return s.Id === 'loggedOutGuid'
		})[0]
		expect(vm.states.length).toEqual(2);
		expect(result.Name).toEqual('LoggedOut');
		expect(result.Id).toEqual('loggedOutGuid');
		expect(result.Selected).toBeFalsy();
	});




	[{
		name: "site",
		type: 'siteIds',
		id: "siteGuid",
		createAgent: function(personId) {
			return {
				PersonId: personId,
				SiteId: "siteGuid"
			}
		}
	}, {
		name: "team",
		type: 'teamIds',
		id: "teamGuid",
		createAgent: function(personId) {
			return {
				PersonId: personId,
				TeamId: "teamGuid"
			}
		}
	}, {
		name: "skill",
		type: 'skillIds',
		id: "skillGuid",
		createAgent: function(personId) {
			return {
				PersonId: personId,
				SkillId: "skillGuid"
			}
		}
	}].forEach(function(selection) {
		it('should hide states when unselecting state for ' + selection.name, function() {
			stateParams[selection.type] = [selection.id];
			$fakeBackend
				.withAgent(
					selection.createAgent("person1"))
				.withAgent(
					selection.createAgent("person2"))
				.withState({
					PersonId: "person1",
					State: "Training",
					StateId: 'TrainingGuid',
					TimeInAlarm: 15
				})
				.withState({
					PersonId: "person2",
					State: "LoggedOut",
					StateId: 'LoggedOutGuid',
					TimeInAlarm: 10
				});

			var c = $controllerBuilder.createController();
			vm = c.vm;
			c.apply(vm.agentsInAlarm = true)
				.apply(function() {
					vm.states.filter(function(s) {
						return s.Id === 'LoggedOutGuid';
					})[0].Selected = false;
				});
			c.wait(5000);

			expect(vm.filteredData.length).toEqual(1);
			expect(vm.filteredData[0].PersonId).toEqual("person1");
		});

		it('should take excluded states as stateParam for ' + selection.name, function() {
			stateParams[selection.type] = [selection.id];
			stateParams.es = ["StateGuid2"];
			$fakeBackend
				.withAgent(
					selection.createAgent("person1"))
				.withAgent(
					selection.createAgent("person2"))
				.withState({
					PersonId: "person1",
					State: "StateGuid1",
					StateId: 'StateGuid1',
					TimeInAlarm: 15
				})
				.withState({
					PersonId: "person2",
					State: "StateGuid2",
					StateId: 'StateGuid2',
					TimeInAlarm: 10
				});

			var c = $controllerBuilder.createController();
			vm = c.vm;
			c.apply(vm.agentsInAlarm = true);
			c.wait(5000);

			expect(vm.filteredData.length).toEqual(1);
			expect(vm.filteredData[0].PersonId).toEqual("person1");
		});

		it('should update url when deselecting state for ' + selection.name, function() {
			stateParams[selection.type] = [selection.id];
			$fakeBackend
				.withAgent(
					selection.createAgent("person1"))
				.withAgent(
					selection.createAgent("person2"))
				.withState({
					PersonId: "person1",
					State: "Training",
					StateId: 'TrainingGuid',
					TimeInAlarm: 15
				})
				.withState({
					PersonId: "person2",
					State: "LoggedOut",
					StateId: 'LoggedOutGuid',
					TimeInAlarm: 10
				});

			var c = $controllerBuilder.createController();
			vm = c.vm;
			c.apply('agentsInAlarm = true')
				.apply(function() {
					vm.states.filter(function(s) {
						return s.Id === 'LoggedOutGuid';
					})[0].Selected = false;
				});
			c.wait(5000);

			expect($state.go).toHaveBeenCalledWith($state.current.name, {
				es: ['LoggedOutGuid']
			}, {
				notify: false
			});
		});

		it('should be able to deselect if sending in excluded state in param ' + selection.name, function() {
			stateParams[selection.type] = [selection.id];
			stateParams.es = ["LoggedOutGuid"];
			$fakeBackend
				.withAgent(
					selection.createAgent("person1"))
				.withAgent(
					selection.createAgent("person2"))
				.withAgent(
					selection.createAgent("person3"))
				.withState({
					PersonId: "person1",
					State: "Training",
					StateId: 'TrainingGuid',
					TimeInAlarm: 15
				})
				.withState({
					PersonId: "person2",
					State: "Logged out",
					StateId: 'LoggedOutGuid',
					TimeInAlarm: 10
				})
				.withState({
					PersonId: "person3",
					State: "Phone",
					StateId: 'PhoneGuid',
					TimeInAlarm: 5
				});

			var c = $controllerBuilder.createController();
			vm = c.vm;
			c.apply(vm.agentsInAlarm = true)
				.apply(function() {
					vm.states.filter(function(s) {
						return s.Id === 'LoggedOutGuid';
					})[0].Selected = true;
				})
				.apply(function() {
					vm.states.filter(function(s) {
						return s.Id === 'PhoneGuid';
					})[0].Selected = false;
				});
			c.wait(5000);

			expect(vm.filteredData.length).toEqual(2);
			expect(vm.filteredData[0].PersonId).toEqual("person1");
			expect(vm.filteredData[1].PersonId).toEqual("person2");
		});

		it('should deselect No State for ' + selection.name, function() {
			stateParams[selection.type] = [selection.id];
			$fakeBackend
				.withAgent(
					selection.createAgent("person1"))
				.withAgent(
					selection.createAgent("person2"))
				.withState({
					PersonId: "person1",
					State: "Training",
					StateId: 'TrainingGuid',
					TimeInAlarm: 15
				})
				.withState({
					PersonId: "person2",
					State: "",
					StateId: null,
					TimeInAlarm: 10
				});

			var c = $controllerBuilder.createController();
			vm = c.vm;
			c.apply(vm.agentsInAlarm = true)
				.apply(function() {
					vm.states.filter(function(s) {
						return s.Id === "noState";
					})[0].Selected = false;
				});
			c.wait(5000);

			expect(vm.filteredData.length).toEqual(1);
			expect(vm.filteredData[0].PersonId).toEqual("person1");
		});

		it('should select after deselecting No State for ' + selection.name, function() {
			stateParams[selection.type] = [selection.id];
			stateParams.es = ["noState"];
			$fakeBackend
				.withAgent(
					selection.createAgent("person1"))
				.withAgent(
					selection.createAgent("person2"))
				.withState({
					PersonId: "person1",
					State: "Training",
					StateId: 'TrainingGuid',
					TimeInAlarm: 15
				})
				.withState({
					PersonId: "person2",
					State: "",
					StateId: null,
					TimeInAlarm: 10
				});

			var c = $controllerBuilder.createController();
			vm = c.vm;
			c.apply(vm.agentsInAlarm = true)
				.apply(function() {
					vm.states.filter(function(s) {
						return s.Id === "noState";
					})[0].Selected = true;
				});
			c.wait(5000);

			expect(vm.filteredData.length).toEqual(2);
		});

	});
});
