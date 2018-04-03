'use strict';
rtaTester.describe('RtaAgentsController', function (it, fit, xit, _,
													 $state,
													 $fakeBackend,
													 $controllerBuilder,
													 stateParams) {
	var vm;

	it('should include state of agents in view', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend
			.withPhoneState({
				Id: '17560fe4-0130-4568-97de-9b5e015b2555',
				Name: 'LoggedOut'
			})
			.withAgentState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				State: "LoggedOut",
				StateId: '17560fe4-0130-4568-97de-9b5e015b2555',
				TimeInAlarm: 10
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true);

		expect(vm.states[0].Name).toEqual('LoggedOut');
		expect(vm.states[0].Id).toEqual('17560fe4-0130-4568-97de-9b5e015b2555');
		expect(vm.states[0].Selected).toEqual(true);
	});

	it('should include state of agent in view', function (t) {
		t.backend
			.withPhoneState({Id: "state"})
			.withAgentState({
				PersonId: "person",
				StateId: 'state'
			});
		var c = t.createController();

		t.apply(function () {
			c.showInAlarm = false;
		});
		t.apply(function () {
			c.states[0].Selected = false;
		});
		t.wait(5000);

		expect(c.states.length).toEqual(1);
		expect(c.states[0].Id).toEqual('state');
		expect(c.states[0].Selected).toBe(false);
	});

	it('should exclude state not on screen', function (t) {
		t.backend
			.withPhoneState({
				Id: 'NotOnScreen'
			})
			.withPhoneState({
				Id: 'OnScreen'
			})
			.withAgentState({
				PersonId: 'person',
				StateId: 'OnScreen',
				TimeInAlarm: 10
			});

		var c = t.createController();

		expect(c.states.length).toEqual(1);
		expect(c.states[0].Id).toEqual('OnScreen');
	});

	it('should not have duplicate states', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend
			.withPhoneState({
				Id: '17560fe4-0130-4568-97de-9b5e015b2555',
				Name: 'LoggedOut'
			})
			.withAgentState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				State: "LoggedOut",
				StateId: '17560fe4-0130-4568-97de-9b5e015b2555',
				TimeInAlarm: 15
			})
			.withAgentState({
				PersonId: "22610fe4-0130-4568-97de-9b5e015b2577",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				State: "LoggedOut",
				StateId: '17560fe4-0130-4568-97de-9b5e015b2555',
				TimeInAlarm: 10
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true);

		expect(vm.states.length).toEqual(1);
	});

	it('should have empty state', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgentState({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			State: "",
			StateId: null,
			TimeInAlarm: 15
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true);

		expect(vm.states.length).toEqual(1);
		expect(vm.states[0].Name).toEqual("No State");
	});

	it('should have empty state if excluded', function () {
		stateParams.teamIds = ["teamGuid"];
		stateParams.es = ["noState"];
		$fakeBackend
			.withAgentState({
				PersonId: "personGuid1",
				TeamId: "teamGuid"
			});

		vm = $controllerBuilder.createController().vm;

		expect(vm.states[0].Name).toEqual('No State');
		expect(vm.states[0].Selected).toEqual(false);
	});

	it('should order states by name', function (t) {
		t.stateParams.teamIds = ["teamGuid"];
		t.backend
			.withPhoneState({
				Id: 'StateGuid1',
				Name: 'B'
			})
			.withPhoneState({
				Id: 'StateGuid2',
				Name: 'A'
			})
			.withAgentState({
				PersonId: "personGuid1",
				TeamId: "teamGuid",
				State: "B",
				StateId: 'StateGuid1',
				TimeInAlarm: 15
			})
			.withAgentState({
				PersonId: "personGuid2",
				TeamId: "teamGuid",
				State: "A",
				StateId: 'StateGuid2',
				TimeInAlarm: 10
			});

		var c = t.createController();

		expect(c.states[0].Name).toEqual("A");
		expect(c.states[1].Name).toEqual("B");
	});

	it('should order with stateParam', function (t) {
		t.stateParams.teamIds = ["teamGuid"];
		t.stateParams.es = ["guid2"];
		t.backend
			.withAgentState({
				PersonId: "personGuid1",
				TeamId: "teamGuid",
				State: "B",
				StateId: 'guid1',
				TimeInAlarm: 15
			})
			.withPhoneState({
				Name: "A",
				Id: "guid2"
			})
			.withPhoneState({
				Name: "B",
				Id: "guid1"
			});

		var c = t.createController();

		expect(c.states[0].Name).toEqual("A");
		expect(c.states[1].Name).toEqual("B");
	});

	it('should get names for hidden states sent through stateParams', function (t) {
		t.stateParams.teamIds = ["teamGuid"];
		t.stateParams.es = ["loggedOutGuid"];
		t.backend
			.withPhoneState({
				Name: "LoggedOut",
				Id: "loggedOutGuid"
			})
			.withAgentState({
				PersonId: "personGuid1",
				TeamId: "teamGuid",
			});

		var c = t.createController();

		expect(c.states[0].Name).toEqual('LoggedOut');
		expect(c.states[0].Id).toEqual('loggedOutGuid');
		expect(c.states[0].Selected).toBeFalsy();
	});

	it('should still get phone state information when deselect multiple from stateParam', function (t) {
		t.stateParams.teamIds = ["teamGuid"];
		t.stateParams.es = ["noState", "loggedOutGuid"];
		t.backend
			.withPhoneState({
				Name: "LoggedOut",
				Id: "loggedOutGuid"
			})
			.withAgentState({
				PersonId: "personGuid1",
				TeamId: "teamGuid"
			});

		var c = t.createController();

		var result = c.states.filter(function (s) {
			return s.Id === 'loggedOutGuid'
		})[0];

		expect(c.states.length).toEqual(2);
		expect(result.Name).toEqual('LoggedOut');
		expect(result.Id).toEqual('loggedOutGuid');
		expect(result.Selected).toBeFalsy();
	});


	[{
		name: "site",
		type: 'siteIds',
		id: "siteGuid",
		createAgent: function (data) {
			return {
				SiteId: "siteGuid",
				PersonId: data.PersonId,
				State: data.State,
				StateId: data.StateId,
				TimeInAlarm: data.TimeInAlarm
			}
		}
	}, {
		name: "team",
		type: 'teamIds',
		id: "teamGuid",
		createAgent: function (data) {
			return {
				TeamId: "teamGuid",
				PersonId: data.PersonId,
				State: data.State,
				StateId: data.StateId,
				TimeInAlarm: data.TimeInAlarm
			}
		}
	}, {
		name: "skill",
		type: 'skillIds',
		id: "skillGuid",
		createAgent: function (data) {
			return {
				SkillId: "skillGuid",
				PersonId: data.PersonId,
				State: data.State,
				StateId: data.StateId,
				TimeInAlarm: data.TimeInAlarm
			}
		}
	}].forEach(function (selection) {
		it('should hide states when unselecting state for ' + selection.name, function () {
			stateParams[selection.type] = [selection.id];
			$fakeBackend
				.withPhoneState({
					Id: 'TrainingGuid',
					Name: 'Training'
				})
				.withPhoneState({
					Id: 'LoggedOutGuid',
					Name: 'LoggedOut'
				})
				.withAgentState(
					selection.createAgent({
						PersonId: "person1",
						State: "Training",
						StateId: 'TrainingGuid',
						TimeInAlarm: 15
					}))
				.withAgentState(
					selection.createAgent({
						PersonId: "person2",
						State: "LoggedOut",
						StateId: 'LoggedOutGuid',
						TimeInAlarm: 10
					}));

			var c = $controllerBuilder.createController();
			vm = c.vm;
			c.apply(vm.showInAlarm = true)
				.apply(function () {
					vm.states.filter(function (s) {
						return s.Id === 'LoggedOutGuid';
					})[0].Selected = false;
				});
			c.wait(5000);

			expect(vm.agentStates.length).toEqual(1);
			expect(vm.agentStates[0].PersonId).toEqual("person1");
		});

		it('should take excluded states as stateParam for ' + selection.name, function () {
			stateParams[selection.type] = [selection.id];
			stateParams.es = ["StateGuid2"];
			$fakeBackend
				.withPhoneState({
					Id: 'StateGuid1',
					Name: 'StateGuid1'
				})
				.withPhoneState({
					Id: 'StateGuid2',
					Name: 'StateGuid2'
				})
				.withAgentState(
					selection.createAgent({
						PersonId: "person1",
						State: "StateGuid1",
						StateId: 'StateGuid1',
						TimeInAlarm: 15
					}))
				.withAgentState(
					selection.createAgent({
						PersonId: "person2",
						State: "StateGuid2",
						StateId: 'StateGuid2',
						TimeInAlarm: 10
					}));

			var c = $controllerBuilder.createController();
			vm = c.vm;
			c.apply(vm.showInAlarm = true);
			c.wait(5000);

			expect(vm.agentStates.length).toEqual(1);
			expect(vm.agentStates[0].PersonId).toEqual("person1");
		});

		// fix
		xit('should update url when deselecting state for ' + selection.name, function () {
			t.stateParams[selection.type] = [selection.id];
			t.backend
				.withPhoneState({
					Id: 'TrainingGuid',
					Name: 'Training'
				})
				.withPhoneState({
					Id: 'LoggedOutGuid',
					Name: 'LoggedOut'
				})
				.withAgentState(
					selection.createAgent({
						PersonId: "person1",
						State: "Training",
						StateId: 'TrainingGuid',
						TimeInAlarm: 15
					}))
				.withAgentState(
					selection.createAgent({
						PersonId: "person2",
						State: "LoggedOut",
						StateId: 'LoggedOutGuid',
						TimeInAlarm: 10
					}));

			var c = $controllerBuilder.createController();
			vm = c.vm;
			c.apply('agentsInAlarm = true')
				.apply(function () {
					vm.states.filter(function (s) {
						return s.Id === 'LoggedOutGuid';
					})[0].Selected = false;
				});

			expect(t.lastGoParams.es).toEqual(['LoggedOutGuid']);
		});

		it('should be able to deselect if sending in excluded state in param ' + selection.name, function (t) {
			t.stateParams[selection.type] = [selection.id];
			t.stateParams.es = ["LoggedOutGuid"];
			t.backend
				.withPhoneState({Id: "TrainingGuid"})
				.withPhoneState({Id: "LoggedOutGuid"})
				.withPhoneState({Id: "PhoneGuid"})
				.withAgentState(
					selection.createAgent({
						PersonId: "person1",
						State: "Training",
						StateId: 'TrainingGuid',
						TimeInAlarm: 15
					}))
				.withAgentState(
					selection.createAgent({
						PersonId: "person2",
						State: "Logged out",
						StateId: 'LoggedOutGuid',
						TimeInAlarm: 10
					}))
				.withAgentState(
					selection.createAgent({
						PersonId: "person3",
						State: "Phone",
						StateId: 'PhoneGuid',
						TimeInAlarm: 5
					}));

			var c = t.createController();
			t.apply(c.showInAlarm = true)
				.apply(function () {
					c.states.filter(function (s) {
						return s.Id === 'LoggedOutGuid';
					})[0].Selected = true;
				})
				.apply(function () {
					c.states.filter(function (s) {
						return s.Id === 'PhoneGuid';
					})[0].Selected = false;
				});
			t.wait(5000);

			expect(c.agentStates.length).toEqual(2);
			expect(c.agentStates[0].PersonId).toEqual("person1");
			expect(c.agentStates[1].PersonId).toEqual("person2");
		});

		it('should deselect No State for ' + selection.name, function () {
			stateParams[selection.type] = [selection.id];
			$fakeBackend
				.withPhoneState({Id: "TrainingGuid"})
				.withAgentState(
					selection.createAgent({
						PersonId: "person1",
						State: "Training",
						StateId: 'TrainingGuid',
						TimeInAlarm: 15
					}))
				.withAgentState(
					selection.createAgent({
						PersonId: "person2",
						State: "",
						StateId: null,
						TimeInAlarm: 10
					}));

			var c = $controllerBuilder.createController();
			vm = c.vm;
			c.apply(vm.showInAlarm = true)
				.apply(function () {
					vm.states.filter(function (s) {
						return s.Id === "noState";
					})[0].Selected = false;
				});
			c.wait(5000);

			expect(vm.agentStates.length).toEqual(1);
			expect(vm.agentStates[0].PersonId).toEqual("person1");
		});

		it('should select after deselecting No State for ' + selection.name, function () {
			stateParams[selection.type] = [selection.id];
			stateParams.es = ["noState"];
			$fakeBackend
				.withPhoneState({Id: "TrainingGuid"})
				.withAgentState(
					selection.createAgent({
						PersonId: "person1",
						State: "Training",
						StateId: 'TrainingGuid',
						TimeInAlarm: 15
					}))
				.withAgentState(
					selection.createAgent({
						PersonId: "person2",
						State: "",
						StateId: null,
						TimeInAlarm: 10
					}));

			var c = $controllerBuilder.createController();
			vm = c.vm;
			c.apply(vm.showInAlarm = true)
				.apply(function () {
					vm.states.filter(function (s) {
						return s.Id === "noState";
					})[0].Selected = true;
				});
			c.wait(5000);

			expect(vm.agentStates.length).toEqual(2);
		});

	});


	/**********************************************************/

	it('should hide states when unselecting state for skill area', function () {
		stateParams.skillAreaId = "skillAreaGuid";
		$fakeBackend
			.withPhoneState({Id: "TrainingGuid"})
			.withPhoneState({Id: "LoggedOutGuid"})
			.withSkillAreas([{
				Id: "skillAreaGuid",
				Skills: [{
					Id: "phoneGuid",
					Name: "Phone"
				}]
			}])
			.withAgentState({
				PersonId: "person1",
				State: "Training",
				StateId: 'TrainingGuid',
				TimeInAlarm: 15,
				SkillId: "phoneGuid"
			})
			.withAgentState({
				PersonId: "person2",
				State: "LoggedOut",
				StateId: 'LoggedOutGuid',
				TimeInAlarm: 10,
				SkillId: "phoneGuid"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true)
			.apply(function () {
				vm.states.filter(function (s) {
					return s.Id === 'LoggedOutGuid';
				})[0].Selected = false;
			});
		c.wait(5000);

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].PersonId).toEqual("person1");
	});

	it('should take excluded states as stateParam for skill area', function () {
		stateParams.skillAreaId = "skillAreaGuid";
		stateParams.es = ["StateGuid2"];
		$fakeBackend
			.withPhoneState({Id: "StateGuid1"})
			.withPhoneState({Id: "StateGuid2"})
			.withSkillAreas([{
				Id: "skillAreaGuid",
				Skills: [{
					Id: "phoneGuid",
					Name: "Phone"
				}]
			}])
			.withAgentState({
				PersonId: "person1",
				State: "StateGuid1",
				StateId: 'StateGuid1',
				TimeInAlarm: 15,
				SkillId: "phoneGuid"
			})
			.withAgentState({
				PersonId: "person2",
				State: "StateGuid2",
				StateId: 'StateGuid2',
				TimeInAlarm: 10,
				SkillId: "phoneGuid"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true);
		c.wait(5000);

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].PersonId).toEqual("person1");
	});

	// fix
	xit('should update url when deselecting state for skill area', function (t) {
		t.stateParams.skillAreaId = "skillAreaGuid";
		t.backend
			.withPhoneState({Id: "TrainingGuid"})
			.withPhoneState({Id: "LoggedOutGuid"})
			.withSkillAreas([{
				Id: "skillAreaGuid",
				Skills: [{
					Id: "phoneGuid",
					Name: "Phone"
				}]
			}])
			.withAgentState({
				PersonId: "person1",
				State: "Training",
				StateId: 'TrainingGuid',
				TimeInAlarm: 15,
				SkillId: "phoneGuid"
			})
			.withAgentState({
				PersonId: "person2",
				State: "LoggedOut",
				StateId: 'LoggedOutGuid',
				TimeInAlarm: 10,
				SkillId: "phoneGuid"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply('agentsInAlarm = true')
			.apply(function () {
				vm.states.filter(function (s) {
					return s.Id === 'LoggedOutGuid';
				})[0].Selected = false;
			});

		expect(t.lastGoParams.es).toEqual(['LoggedOutGuid']);
	});

	it('should be able to deselect if sending in excluded state in param skill area', function (t) {
		t.stateParams.skillAreaId = "skillAreaGuid";
		t.stateParams.es = ["LoggedOutGuid"];
		t.backend
			.withPhoneState({Id: "TrainingGuid"})
			.withPhoneState({Id: "LoggedOutGuid"})
			.withPhoneState({Id: "PhoneGuid"})
			.withSkillAreas([{
				Id: "skillAreaGuid",
				Skills: [{
					Id: "phoneGuid",
					Name: "Phone"
				}]
			}])
			.withAgentState({
				PersonId: "person1",
				State: "Training",
				StateId: 'TrainingGuid',
				TimeInAlarm: 15,
				SkillId: "phoneGuid"
			})
			.withAgentState({
				PersonId: "person2",
				State: "Logged out",
				StateId: 'LoggedOutGuid',
				TimeInAlarm: 10,
				SkillId: "phoneGuid"
			})
			.withAgentState({
				PersonId: "person3",
				State: "Phone",
				StateId: 'PhoneGuid',
				TimeInAlarm: 5,
				SkillId: "phoneGuid"
			});

		var c = t.createController();
		t
			.apply(function () {
				c.states.filter(function (s) {
					return s.Id === 'LoggedOutGuid';
				})[0].Selected = true;
			})
			.apply(function () {
				c.states.filter(function (s) {
					return s.Id === 'PhoneGuid';
				})[0].Selected = false;
			});
		t.wait(5000);

		expect(c.agentStates.length).toEqual(2);
		expect(c.agentStates[0].PersonId).toEqual("person1");
		expect(c.agentStates[1].PersonId).toEqual("person2");
	});

	it('should deselect No State for skill area', function () {
		stateParams.skillAreaId = "skillAreaGuid";
		$fakeBackend
			.withPhoneState({Id: "TrainingGuid"})
			.withSkillAreas([{
				Id: "skillAreaGuid",
				Skills: [{
					Id: "phoneGuid",
					Name: "Phone"
				}]
			}])
			.withAgentState({
				PersonId: "person1",
				State: "Training",
				StateId: 'TrainingGuid',
				TimeInAlarm: 15,
				SkillId: "phoneGuid"
			})
			.withAgentState({
				PersonId: "person2",
				State: "",
				StateId: null,
				TimeInAlarm: 10,
				SkillId: "phoneGuid"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true)
			.apply(function () {
				vm.states.filter(function (s) {
					return s.Id === "noState";
				})[0].Selected = false;
			});
		c.wait(5000);

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].PersonId).toEqual("person1");
	});

	it('should select after deselecting No State for skill area', function () {
		stateParams.skillAreaId = "skillAreaGuid";
		stateParams.es = ["noState"];
		$fakeBackend
			.withPhoneState({Id: "TrainingGuid"})
			.withSkillAreas([{
				Id: "skillAreaGuid",
				Skills: [{
					Id: "phoneGuid",
					Name: "Phone"
				}]
			}])
			.withAgentState({
				PersonId: "person1",
				State: "Training",
				StateId: 'TrainingGuid',
				TimeInAlarm: 15,
				SkillId: "phoneGuid"
			})
			.withAgentState({
				PersonId: "person2",
				State: "",
				StateId: null,
				TimeInAlarm: 10,
				SkillId: "phoneGuid"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true)
			.apply(function () {
				vm.states.filter(function (s) {
					return s.Id === "noState";
				})[0].Selected = true;
			});
		c.wait(5000);

		expect(vm.agentStates.length).toEqual(2);
	});

	it('should not add duplicate excluded states for each poll', function (t) {
		t.stateParams.es = ["state"];
		t.backend.withPhoneState({Id: "state"});

		var c = t.createController();
		t.wait(5000);

		expect(c.states.length).toEqual(1);
		expect(c.states[0].Id).toEqual('state');
	});

});
