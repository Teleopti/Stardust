define(['buster', 'views/realtimeadherenceagents/vm'], function (buster, viewModel) {
	return function () {

		buster.testCase("real time adherence agents viewmodel", {
			"should have no agents if none filled": function () {
				var vm = viewModel();
				assert.equals(vm.agentStates(), []);
			},

			"should fill agents info": function () {
				var agent = { PersonId: "guid", Name: "Bill", SiteId: "gui1", SiteName: "site", TeamId: "guid2", TeamName: "team", TimeZoneOffsetMinutes: -600 };
				var vm = viewModel();
				vm.fillAgents([agent]);

				assert.equals(vm.agents[0].PersonId, agent.PersonId);
				assert.equals(vm.agents[0].Name, agent.Name);
				assert.equals(vm.agents[0].SiteId, agent.SiteId);
				assert.equals(vm.agents[0].SiteName, agent.SiteName);
				assert.equals(vm.agents[0].TeamId, agent.TeamId);
				assert.equals(vm.agents[0].TeamName, agent.TeamName);
				assert.equals(vm.agents[0].TimeZoneOffset, agent.TimeZoneOffsetMinutes);
			},
			"should fill agent state data": function () {
				var state1 = {
					PersonId: 'guid1',
					State: 'Ready',
					StateStart: moment('2014-01-21 12:00').format(),
					Activity: 'Phone',
					NextActivity: 'Lunch',
					NextActivityStartTime: moment('2014-01-21 13:00').format(),
					Alarm: 'Adhering',
					AlarmColor: '#333',
					AlarmStart: moment('2014-01-21 12:15').format()
				};
				var state2 = {
					PersonId: 'guid2',
					State: 'Pause',
					StateStart: moment('2014-01-21 12:00').format(),
					Activity: 'Phone',
					NextActivity: 'Lunch',
					NextActivityStartTime: moment('2014-01-21 13:00').format(),
					Alarm: 'Not Adhering',
					AlarmColor: '#fff',
					AlarmStart: moment('2014-01-21 12:15').format()
				};

				var vm = viewModel();
				vm.fillAgents([
					{ PersonId: "guid1", Name: "Bill", SiteId: "gui1", SiteName: "site", TeamId: "guid2", TeamName: "team", TimeZoneOffsetMinutes: -600 },
					{ PersonId: "guid2", Name: "John", SiteId: "gui1", SiteName: "site", TeamId: "guid2", TeamName: "team", TimeZoneOffsetMinutes: -600 }
				]);
				vm.fillAgentsStates([state1, state2]);

				var hexToRgb = function (hex) {

					hex = hex ? hex.substring(1) : 'ffffff';
					var bigint = parseInt(hex, 16);
					var r = (bigint >> 16) & 255;
					var g = (bigint >> 8) & 255;
					var b = bigint & 255;

					return r + "," + g + "," + b;
				}
				assert.equals(vm.agentStates()[0].PersonId, state1.PersonId);
				assert.equals(vm.agentStates()[0].State(), state1.State);
				assert.equals(vm.agentStates()[0].EnteredCurrentAlarm(), moment.utc(state1.StateStart).add(-600, 'minutes').format());
				assert.equals(vm.agentStates()[0].Activity(), state1.Activity);
				assert.equals(vm.agentStates()[0].NextActivity(), state1.NextActivity);
				assert.equals(vm.agentStates()[0].NextActivityStartTime(), moment.utc(state1.NextActivityStartTime).add(-600, 'minutes').format());
				assert.equals(vm.agentStates()[0].Alarm(), state1.Alarm);
				assert.equals(vm.agentStates()[0].AlarmColor(), 'rgba(' + hexToRgb(state1.AlarmColor) + ', 0.6)');
				assert.equals(vm.agentStates()[0].AlarmStart(), moment.utc(state1.AlarmStart).add(-600, 'minutes').format());
				assert.equals(vm.agentStates()[1].PersonId, state2.PersonId);
				assert.equals(vm.agentStates()[1].State(), state2.State);
				assert.equals(vm.agentStates()[1].EnteredCurrentAlarm(), moment.utc(state1.StateStart).add(-600, 'minutes').format());
				assert.equals(vm.agentStates()[1].Activity(), state2.Activity);
				assert.equals(vm.agentStates()[1].NextActivity(), state2.NextActivity);
				assert.equals(vm.agentStates()[1].NextActivityStartTime(), moment.utc(state2.NextActivityStartTime).add(-600, 'minutes').format());
				assert.equals(vm.agentStates()[1].Alarm(), state2.Alarm);
				assert.equals(vm.agentStates()[1].AlarmColor(), 'rgba(' + hexToRgb(state2.AlarmColor) + ', 0.6)');
				assert.equals(vm.agentStates()[1].AlarmStart(), moment.utc(state2.AlarmStart).add(-600, 'minutes').format());
			},

			"should order by agent name": function () {
				var state1 = {
					PersonId: 'guid1',
					State: 'Ready',
					StateStart: moment('2014-01-21 12:00').format(),
					Activity: 'Phone',
					NextActivity: 'Lunch',
					NextActivityStartTime: moment('2014-01-21 13:00').format(),
					Alarm: 'Adhering',
					AlarmColor: '#333',
					AlarmStart: moment('2014-01-21 12:15').format()
				};
				var state2 = {
					PersonId: 'guid2',
					State: 'Pause',
					StateStart: moment('2014-01-21 12:00').format(),
					Activity: 'Phone',
					NextActivity: 'Lunch',
					NextActivityStartTime: moment('2014-01-21 13:00').format(),
					Alarm: 'Not Adhering',
					AlarmColor: '#fff',
					AlarmStart: moment('2014-01-21 12:15').format()
				};
				var agent1 = { PersonId: "guid1", Name: "John", SiteId: "gui1", SiteName: "site", TeamId: "guid2", TeamName: "team", TimeZoneOffsetMinutes: -600 };
				var agent2 = { PersonId: "guid2", Name: "Bill", SiteId: "gui1", SiteName: "site", TeamId: "guid2", TeamName: "team", TimeZoneOffsetMinutes: -600 };
				var vm = viewModel();
				vm.fillAgents([agent1, agent2]);

				vm.fillAgentsStates([state1, state2]);

				assert.equals(vm.agentStates()[0].Name, agent2.Name);
				assert.equals(vm.agentStates()[1].Name, agent1.Name);
			},
			"should display alarmtime based on when agent entered current alarm": function () {
				var state = {
					PersonId: 'guid1',
					StateStart: moment().add(-10, 'seconds'),
					AlarmStart: moment()
				};
				var agent = { PersonId: "guid1", TimeZoneOffsetMinutes: 0 };
				var vm = viewModel();
				vm.fillAgents([agent]);

				vm.fillAgentsStates([state]);

				assert.equals(vm.agentStates()[0].AlarmTime(), "0:00:10");
			},
			"should not display alarm or alarm color untill alarm start has passed": function () {
				var state = {
					PersonId: 'guid1',
					Alarm: 'Adhering',
					StateStart: moment(),
					AlarmStart: moment().add(10, 'seconds')
				};
				var agent = { PersonId: "guid1", TimeZoneOffsetMinutes: 0 };
				var vm = viewModel();
				vm.fillAgents([agent]);

				vm.fillAgentsStates([state]);

				assert.equals(vm.agentStates()[0].Alarm(), undefined);
			},
			"filtering:": {
				"should only display matching agents": function() {
					var agent1 = { PersonId: "guid1", Name: "Kurt", TimeZoneOffsetMinutes: 0 },
						agent2 = { PersonId: "guid2", Name: "Glen", TimeZoneOffsetMinutes: 0 },
						agent1State = { PersonId: "guid1" },
						agent2State = { PersonId: "guid2" };
					var vm = viewModel();
					vm.fillAgents([agent1, agent2]);
					vm.fillAgentsStates([agent1State, agent2State]);

					vm.filter("Kurt");

					assert.equals(vm.filteredAgents().length, 1);
					assert.equals(vm.filteredAgents()[0].Name, "Kurt");
				},
				"should only display agent matching tripple filter criteria": function() {
					var agent1 = { PersonId: "guid1", Name: "Kurt", TimeZoneOffsetMinutes: 0 },
						agent2 = { PersonId: "guid2", Name: "Kurt", TimeZoneOffsetMinutes: 0 },
						agent1State = { PersonId: "guid1", Activity: "Lunch", Alarm: "Adhering" },
						agent2State = { PersonId: "guid2", Activity: "Lunch", Alarm: "Positive" };
					var vm = viewModel();
					vm.fillAgents([agent1, agent2]);
					vm.fillAgentsStates([agent1State, agent2State]);

					vm.filter("Kurt Lunch Adhering");

					assert.equals(vm.filteredAgents().length, 1);
					assert.equals(vm.filteredAgents()[0].Alarm(), "Adhering");
				},
				"should only display agent matching two word filter criteria": function() {
					var agent1 = { PersonId: "guid1", Name: "Out of", TimeZoneOffsetMinutes: 0 },
						agent2 = { PersonId: "guid2", Name: "Kurt", TimeZoneOffsetMinutes: 0 },
						agent1State = { PersonId: "guid1", Alarm: "In adherence" },
						agent2State = { PersonId: "guid2", Alarm: "Out of adherence" };
					var vm = viewModel();
					vm.fillAgents([agent1, agent2]);
					vm.fillAgentsStates([agent1State, agent2State]);

					vm.filter("'Out of adherence'");

					assert.equals(vm.filteredAgents().length, 1);
					assert.equals(vm.filteredAgents()[0].Alarm(), "Out of adherence");
				},
				"should only display agent not matching when negating filter word with multiple words": function() {
					var agent1 = { PersonId: "guid1", Name: "Kurt", TimeZoneOffsetMinutes: 0 },
						agent2 = { PersonId: "guid2", Name: "Glen", TimeZoneOffsetMinutes: 0 },
						agent3 = { PersonId: "guid3", Name: "Arne", TimeZoneOffsetMinutes: 0 },
						agent1State = { PersonId: "guid1", Alarm: "Positive" },
						agent2State = { PersonId: "guid2", Alarm: "Positive" },
						agent3State = { PersonId: "guid3", Alarm: "In adherence" };
					var vm = viewModel();
					vm.fillAgents([agent1, agent2, agent3]);
					vm.fillAgentsStates([agent1State, agent2State, agent3State]);

					vm.filter("!Glen Positive");

					assert.equals(vm.filteredAgents().length, 1);
					assert.equals(vm.filteredAgents()[0].Name, "Kurt");
				},
				"should only display agent that exactly matches when using quotes": function() {
					var agent1 = { PersonId: "guid1", Name: "Glen", TimeZoneOffsetMinutes: 0 },
						agent2 = { PersonId: "guid2", Name: "Kurt", TimeZoneOffsetMinutes: 0 },
						agent1State = { PersonId: "guid1", State: "Ready" },
						agent2State = { PersonId: "guid2", State: "Not ready" };
					var vm = viewModel();
					vm.fillAgents([agent1, agent2]);
					vm.fillAgentsStates([agent1State, agent2State]);

					vm.filter("'Ready'");

					assert.equals(vm.filteredAgents().length, 1);
					assert.equals(vm.filteredAgents()[0].State(), "Ready");
				},
				"should only display agents thats not matching negating quoted searchwords": function() {
					var agent1 = { PersonId: "guid1", Name: "Kurt", TimeZoneOffsetMinutes: 0 },
						agent2 = { PersonId: "guid2", Name: "Glen", TimeZoneOffsetMinutes: 0 },
						agent1State = { PersonId: "guid1", Alarm: "Positive" },
						agent2State = { PersonId: "guid2", Alarm: "In adherence" };
					var vm = viewModel();
					vm.fillAgents([agent1, agent2]);
					vm.fillAgentsStates([agent1State, agent2State]);

					vm.filter("!'In adherence'");

					assert.equals(vm.filteredAgents().length, 1);
					assert.equals(vm.filteredAgents()[0].Name, "Kurt");
				},
				"should display all matching agents when using OR between searchwords": function() {
					var agent1 = { PersonId: "guid1", Name: "Kurt", TimeZoneOffsetMinutes: 0 },
						agent2 = { PersonId: "guid2", Name: "Glen", TimeZoneOffsetMinutes: 0 },
						agent3 = { PersonId: "guid3", Name: "Arne", TimeZoneOffsetMinutes: 0 },
						agent1State = { PersonId: "guid1", Alarm: "In adherence" },
						agent2State = { PersonId: "guid2", Alarm: "Positive" },
						agent3State = { PersonId: "guid3", Alarm: "Negative" };
					var vm = viewModel();
					vm.fillAgents([agent1, agent2, agent3]);
					vm.fillAgentsStates([agent1State, agent2State, agent3State]);

					vm.filter("Positive OR Negative");

					assert.equals(vm.filteredAgents().length, 2);
					assert.equals(vm.filteredAgents()[0].Name, "Arne");
					assert.equals(vm.filteredAgents()[1].Name, "Glen");
				},
				"should display all matching agents when using OR and match other words normally": function() {
					var agent1 = { PersonId: "guid1", Name: "Kurt", TimeZoneOffsetMinutes: 0 },
						agent2 = { PersonId: "guid2", Name: "Glen", TimeZoneOffsetMinutes: 0 },
						agent1State = { PersonId: "guid1", Activity: "Phone", NextActivity: "Lunch", State: "Not Ready" },
						agent2State = { PersonId: "guid2", Activity: "Phone", NextActivity: "Lunch", State: "Ready" };
					var vm = viewModel();
					vm.fillAgents([agent1, agent2]);
					vm.fillAgentsStates([agent1State, agent2State]);

					vm.filter("Phone or lunch 'Ready'");

					assert.equals(vm.filteredAgents().length, 1);
					assert.equals(vm.filteredAgents()[0].Name, "Glen");
				},
				"should display all agents not matching negated OR": function() {
					var agent1 = { PersonId: "guid1", Name: "Kurt", TimeZoneOffsetMinutes: 0 },
						agent2 = { PersonId: "guid2", Name: "Glen", TimeZoneOffsetMinutes: 0 },
						agent3 = { PersonId: "guid3", Name: "Arne", TimeZoneOffsetMinutes: 0 },
						agent1State = { PersonId: "guid1", Alarm: "In adherence" },
						agent2State = { PersonId: "guid2", Alarm: "Positive" },
						agent3State = { PersonId: "guid3", Alarm: "Negative" };
					var vm = viewModel();
					vm.fillAgents([agent1, agent2, agent3]);
					vm.fillAgentsStates([agent1State, agent2State, agent3State]);

					vm.filter("Positive OR !Negative");

					assert.equals(vm.filteredAgents().length, 2);
					assert.equals(vm.filteredAgents()[0].Name, "Glen");
					assert.equals(vm.filteredAgents()[1].Name, "Kurt");
				},
				"should display agents matching more than two OR searchwords" : function() {
					var agent1 = { PersonId: "guid1", Name: "Kurt", TimeZoneOffsetMinutes: 0 },
						agent2 = { PersonId: "guid2", Name: "Glen", TimeZoneOffsetMinutes: 0 },
						agent3 = { PersonId: "guid3", Name: "Arne", TimeZoneOffsetMinutes: 0 },
						agent4 = { PersonId: "guid4", Name: "Kalle", TimeZoneOffsetMinutes: 0 },
						agent1State = { PersonId: "guid1", State: "Phone" },
						agent2State = { PersonId: "guid2", State: "Lunch" },
						agent3State = { PersonId: "guid3", State: "Break" },
						agent4State = { PersonId: "guid4", State: "Admin" };
					var vm = viewModel();
					vm.fillAgents([agent1, agent2, agent3, agent4]);
					vm.fillAgentsStates([agent1State, agent2State, agent3State, agent4State]);

					vm.filter("Phone OR Lunch OR Break");

					assert.equals(vm.filteredAgents().length, 3);
					assert.equals(vm.filteredAgents()[0].Name, "Arne");
					assert.equals(vm.filteredAgents()[1].Name, "Glen");
					assert.equals(vm.filteredAgents()[2].Name, "Kurt");
				},
				"should display agents matching time searchwords" : function() {
					var agent1 = { PersonId: "guid1", Name: "Kurt", TimeZoneOffsetMinutes: 0 },
						agent2 = { PersonId: "guid2", Name: "Glen", TimeZoneOffsetMinutes: 0 },
						agent1State = { PersonId: "guid1", State: "Phone", NextActivity: "Lunch", NextActivityStartTime: moment('2014-01-21 13:00').format() },
						agent2State = { PersonId: "guid2", State: "Lunch", NextActivity: "Lunch", NextActivityStartTime: moment('2014-01-22 13:00').format() };
					var vm = viewModel();
					vm.fillAgents([agent1, agent2]);
					vm.fillAgentsStates([agent1State, agent2State]);
					vm.filter("2014-01-21");

					assert.equals(vm.filteredAgents().length, 1);
					assert.equals(vm.filteredAgents()[0].Name, "Kurt");
				}
			}
		});
	};
});