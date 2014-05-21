define(['buster', 'views/realtimeadherenceagents/vm'], function (buster, viewModel) {
	return function () {
		
		buster.testCase("real time adherence agents viewmodel", {
			"should have no agents if none filled": function () {
				var vm = viewModel();
				assert.equals(vm.agentStates(), []);
			},

			"should fill agent state data": function() {
				var state1 = {
					Name: 'Pierre Baldi',
					State: 'Ready',
					Activity: 'Phone',
					NextActivity: 'Lunch',
					NextActivityStartTime: '2014-01-21 13:00',
					Alarm: 'Adhering',
					AlarmTime: '2014-01-21 12:15'
				};
				var state2 = {
					Name: 'Ashley Andeen',
					State: 'Pause',
					Activity: 'Phone',
					NextActivity: 'Lunch',
					NextActivityStartTime: '2014-01-21 13:00',
					Alarm: 'Not Adhering',
					AlarmTime: '2014-01-21 12:15'
				};
				var vm = viewModel();
				vm.fill([state1, state2]);

				assert.equals(vm.agentStates()[0].Name, state1.Name);
				assert.equals(vm.agentStates()[0].State, state1.State);
				assert.equals(vm.agentStates()[0].Activity, state1.Activity);
				assert.equals(vm.agentStates()[0].NextActivity, state1.NextActivity);
				assert.equals(vm.agentStates()[0].NextActivityStartTime, state1.NextActivityStartTime);
				assert.equals(vm.agentStates()[0].Alarm, state1.Alarm);
				assert.equals(vm.agentStates()[0].AlarmTime, state1.AlarmTime);
				assert.equals(vm.agentStates()[1].Name, state2.Name);
				assert.equals(vm.agentStates()[1].State, state2.State);
				assert.equals(vm.agentStates()[1].Activity, state2.Activity);
				assert.equals(vm.agentStates()[1].NextActivity, state2.NextActivity);
				assert.equals(vm.agentStates()[1].NextActivityStartTime, state2.NextActivityStartTime);
				assert.equals(vm.agentStates()[1].Alarm, state2.Alarm);
				assert.equals(vm.agentStates()[1].AlarmTime, state2.AlarmTime);
			},
		});		
	};
});