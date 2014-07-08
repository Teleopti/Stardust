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

			"should fill agent state data": function() {
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
				vm.fillAgents([{ PersonId: "guid1", Name: "Bill", SiteId: "gui1", SiteName: "site", TeamId: "guid2", TeamName: "team", TimeZoneOffsetMinutes: -600 },
					{ PersonId: "guid2", Name: "John", SiteId: "gui1", SiteName: "site", TeamId: "guid2", TeamName: "team", TimeZoneOffsetMinutes: -600 }]);
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

			"should order by agent name" : function() {
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
				vm.fillAgents([agent1,agent2]);
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
			"should filter agents": function () {
				var agent1 = { PersonId: "guid1", Name: "Kurt", TimeZoneOffsetMinutes: 0 },
					agent2 = { PersonId: "guid2", Name: "Glen", TimeZoneOffsetMinutes: 0 },
					agent1State = { PersonId: "guid1" },
					agent2State = { PersonId: "guid2" };
				
				var vm = viewModel();
				vm.fillAgents([agent1, agent2]);
				vm.fillAgentsStates([agent1State, agent2State]);
				vm.filter("Kurt");
				assert.equals(vm.filteredAgents()[0].Name, "Kurt");
			}
		});
	};
});