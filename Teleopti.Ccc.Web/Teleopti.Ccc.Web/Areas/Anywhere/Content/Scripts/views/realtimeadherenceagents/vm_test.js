﻿define(['buster', 'views/realtimeadherenceagents/vm', 'resources'], function (buster, viewModel, resources) {
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
					StateStart: moment('2014-01-21 12:00').format(resources.DateTimeFormatForMoment),
					Activity: 'Phone',
					NextActivity: 'Lunch',
					NextActivityStartTime: moment('2014-01-21 13:00').format(resources.DateTimeFormatForMoment),
					Alarm: 'Adhering',
					AlarmColor: '#333',
					AlarmStart: moment('2014-01-21 12:15').format(resources.DateTimeFormatForMoment)
				};
				var state2 = {
					PersonId: 'guid2',
					State: 'Pause',
					StateStart: moment('2014-01-21 12:00').format(resources.DateTimeFormatForMoment),
					Activity: 'Phone',
					NextActivity: 'Lunch',
					NextActivityStartTime: moment('2014-01-21 13:00').format(resources.DateTimeFormatForMoment),
					Alarm: 'Not Adhering',
					AlarmColor: '#fff',
					AlarmStart: moment('2014-01-21 12:15').format(resources.DateTimeFormatForMoment)
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
				assert.equals(vm.agentStates()[0].StateStart(), moment.utc(state1.StateStart, resources.DateTimeFormatForMoment).add(-600, 'minutes').format(resources.DateTimeFormatForMoment));
				assert.equals(vm.agentStates()[0].Activity(), state1.Activity);
				assert.equals(vm.agentStates()[0].NextActivity(), state1.NextActivity);
				assert.equals(vm.agentStates()[0].NextActivityStartTime(), moment.utc(state1.NextActivityStartTime, resources.TimeFormatForMoment).add(-600, 'minutes').format(resources.TimeFormatForMoment));
				assert.equals(vm.agentStates()[0].Alarm(), state1.Alarm);
				assert.equals(vm.agentStates()[0].AlarmColor(), 'rgba(' + hexToRgb(state1.AlarmColor) + ', 0.6)');
				assert.equals(vm.agentStates()[0].AlarmStart(), moment.utc(state1.AlarmStart, resources.DateTimeFormatForMoment).add(-600, 'minutes').format(resources.DateTimeFormatForMoment));
				assert.equals(vm.agentStates()[1].PersonId, state2.PersonId);
				assert.equals(vm.agentStates()[1].State(), state2.State);
				assert.equals(vm.agentStates()[1].StateStart(), moment.utc(state1.StateStart, resources.DateTimeFormatForMoment).add(-600, 'minutes').format(resources.DateTimeFormatForMoment));
				assert.equals(vm.agentStates()[1].Activity(), state2.Activity);
				assert.equals(vm.agentStates()[1].NextActivity(), state2.NextActivity);
				assert.equals(vm.agentStates()[1].NextActivityStartTime(), moment.utc(state2.NextActivityStartTime, resources.TimeFormatForMoment).add(-600, 'minutes').format(resources.TimeFormatForMoment));
				assert.equals(vm.agentStates()[1].Alarm(), state2.Alarm);
				assert.equals(vm.agentStates()[1].AlarmColor(), 'rgba(' + hexToRgb(state2.AlarmColor) + ', 0.6)');
				assert.equals(vm.agentStates()[1].AlarmStart(), moment.utc(state2.AlarmStart, resources.DateTimeFormatForMoment).add(-600, 'minutes').format(resources.DateTimeFormatForMoment));
			},

			"should order by agent name" : function() {
				var state1 = {
					PersonId: 'guid1',
					State: 'Ready',
					StateStart: moment('2014-01-21 12:00').format(resources.DateTimeFormatForMoment),
					Activity: 'Phone',
					NextActivity: 'Lunch',
					NextActivityStartTime: moment('2014-01-21 13:00').format(resources.DateTimeFormatForMoment),
					Alarm: 'Adhering',
					AlarmColor: '#333',
					AlarmStart: moment('2014-01-21 12:15').format(resources.DateTimeFormatForMoment)
				};
				var state2 = {
					PersonId: 'guid2',
					State: 'Pause',
					StateStart: moment('2014-01-21 12:00').format(resources.DateTimeFormatForMoment),
					Activity: 'Phone',
					NextActivity: 'Lunch',
					NextActivityStartTime: moment('2014-01-21 13:00').format(resources.DateTimeFormatForMoment),
					Alarm: 'Not Adhering',
					AlarmColor: '#fff',
					AlarmStart: moment('2014-01-21 12:15').format(resources.DateTimeFormatForMoment)
				};
				var agent1 = { PersonId: "guid1", Name: "John", SiteId: "gui1", SiteName: "site", TeamId: "guid2", TeamName: "team", TimeZoneOffsetMinutes: -600 };
				var agent2 = { PersonId: "guid2", Name: "Bill", SiteId: "gui1", SiteName: "site", TeamId: "guid2", TeamName: "team", TimeZoneOffsetMinutes: -600 };
				var vm = viewModel();
				vm.fillAgents([agent1,agent2]);
				vm.fillAgentsStates([state1, state2]);
				assert.equals(vm.agentStates()[0].Name, agent2.Name);
				assert.equals(vm.agentStates()[1].Name, agent1.Name);
			},

		});		
	};
});