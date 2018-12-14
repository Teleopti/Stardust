'use strict';

rtaTester.describe('RtaAgentsController', function (it, fit, xit) {
	var vm;

	[
		{
			time: "2016-05-26T00:30:00",
			expect: ["2016-05-26T00:00:00", "2016-05-26T01:00:00", "2016-05-26T02:00:00", "2016-05-26T03:00:00"]
		},
		{
			time: "2016-05-26T11:30:00",
			expect: ["2016-05-26T11:00:00", "2016-05-26T12:00:00", "2016-05-26T13:00:00", "2016-05-26T14:00:00"]
		},
		{
			time: "2016-05-26T15:30:00",
			expect: ["2016-05-26T15:00:00", "2016-05-26T16:00:00", "2016-05-26T17:00:00", "2016-05-26T18:00:00"]
		},
		{
			time: "2016-05-26T23:30:00",
			expect: ["2016-05-26T23:00:00", "2016-05-27T00:00:00", "2016-05-27T01:00:00", "2016-05-27T02:00:00"]
		}
	].forEach(function (example) {
		it('should display time line for ' + example.time, function (t) {
			t.stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
			t.backend
				.withTime(example.time)
				.withAgentState({
					PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
					TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				});

			var vm = t.createController();
			expect(vm.timeline.length).toEqual(example.expect.length);
			example.expect.forEach(function (e, i) {
				expect(vm.timeline[i].Time).toEqual(moment(e).format('LT'));
			});
		});
	});

	[
		{
			time: "2016-05-26T12:00:00",
			expect: ["25%", "50%", "75%", "100%"]
		},
		{
			time: "2016-05-26T12:30:00",
			expect: ["12.5%", "37.5%", "62.5%", "87.5%"]
		}
	].forEach(function (example) {

		it('should position time line for ' + example.time, function (t) {
			t.stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
			t.backend
				.withTime(example.time)
				.withAgentState({
					PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
					TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				});

			vm = t.createController();

			expect(vm.timeline.length).toEqual(example.expect.length);
			example.expect.forEach(function (e, i) {
				expect(vm.timeline[i].Offset).toEqual(e);
			});
		});

	});

	it('should display scheduled activity', function (t) {
		t.stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];

		t.backend
			.withTime("2016-05-26T12:00:00")
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",

				Shift: [{
					Color: "#80FF80",
					StartTime: "2016-05-26T12:00:00",
					EndTime: "2016-05-26T14:00:00"
				}]
			});

		var vm = t.createController();
		t.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].Shift.length).toEqual(1);
		expect(vm.agentStates[0].Shift[0].Color).toEqual("#80FF80");
		expect(vm.agentStates[0].Shift[0].Offset).toEqual("25%");
		expect(vm.agentStates[0].Shift[0].Width).toEqual("50%");
	});

	it('should display all activities', function (t) {
		t.stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];

		t.backend
			.withTime("2016-05-26T09:00:00")
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				Shift: [{
					Color: "#80FF80",
					StartTime: "2016-05-26T08:00:00",
					EndTime: "2016-05-26T10:00:00"
				},
					{
						Color: "#0000FF",
						StartTime: "2016-05-26T10:00:00",
						EndTime: "2016-05-26T12:00:00"
					}
				]
			});

		var vm = t.createController();
		t.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].Shift.length).toEqual(2);
		expect(vm.agentStates[0].Shift[0].Color).toEqual("#80FF80");
		expect(vm.agentStates[0].Shift[0].Offset).toEqual("0%");
		expect(vm.agentStates[0].Shift[0].Width).toEqual("50%");
		expect(vm.agentStates[0].Shift[1].Color).toEqual("#0000FF");
		expect(vm.agentStates[0].Shift[1].Offset).toEqual("50%");
		expect(vm.agentStates[0].Shift[1].Width).toEqual("50%");
	});

	it('should display all activities', function (t) {
		t.stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		t.backend
			.withTime("2014-01-21T12:45:00")
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				Shift: [{
					Color: "#80FF80",
					StartTime: "2014-01-21T12:00:00",
					EndTime: "2014-01-21T13:00:00"
				},
					{
						Color: "#0000FF",
						StartTime: "2014-01-21T13:00:00",
						EndTime: "2014-01-21T13:30:00"
					}
				]
			});

		var vm = t.createController();
		t.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].Shift.length).toEqual(2);
	});

	it('should not display past activity before display window', function (t) {
		t.stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		t.backend
			.withTime("2016-05-30T13:00:00")
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				Shift: [{
					Color: "#80FF80",
					StartTime: "2016-05-30T08:00:00",
					EndTime: "2016-05-30T12:00:00"
				}]
			});

		var vm = t.createController();
		t.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].Shift.length).toEqual(0);
	});

	it('should not display future activities outside of display window', function (t) {
		t.stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		t.backend
			.withTime("2016-05-30T08:00:00")
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				Shift: [{
					Color: "#80FF80",
					StartTime: "2016-05-30T15:00:00",
					EndTime: "2016-05-30T21:00:00"
				}]
			});

		var vm = t.createController();
		t.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].Shift.length).toEqual(0);
	});

	it('should not display future activities outside of display window', function (t) {
		t.stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		t.backend
			.withTime("2016-05-30T08:00:00")
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				Shift: [{
					StartTime: "2016-05-30T10:00:00",
					EndTime: "2016-05-30T11:00:00"
				},
					{
						StartTime: "2016-05-30T11:00:00",
						EndTime: "2016-05-30T12:00:00"
					},
					{
						StartTime: "2016-05-30T12:00:00",
						EndTime: "2016-05-30T13:00:00"
					},
					{
						StartTime: "2016-05-30T13:00:00",
						EndTime: "2016-05-30T14:00:00"
					}
				]
			});

		var vm = t.createController();
		t.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].Shift.length).toEqual(1);
	});

	it('should cut activities that are larger than display window', function (t) {
		t.stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		t.backend
			.withTime("2016-05-30T11:00:00")
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				Shift: [{
					Color: "#80FF80",
					StartTime: "2016-05-30T08:00:00",
					EndTime: "2016-05-30T17:00:00"
				}]
			});

		var vm = t.createController();
		t.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].Shift[0].Offset).toEqual('0%');
		expect(vm.agentStates[0].Shift[0].Width).toEqual('100%');
	});

	it('should cut activities starting before display window', function (t) {
		t.stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		t.backend
			.withTime("2016-05-30T15:00:00")
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				Shift: [{
					Color: "#80FF80",
					StartTime: "2016-05-30T08:00:00",
					EndTime: "2016-05-30T17:00:00"
				}]
			});

		var vm = t.createController();
		t.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].Shift[0].Offset).toEqual('0%');
		expect(vm.agentStates[0].Shift[0].Width).toEqual('75%');
	});

	it('should produce an activity name', function (t) {
		t.stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		t.backend
			.withTime("2016-05-30T11:00:00")
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				Shift: [{
					Name: 'Phone',
					StartTime: "2016-05-30T08:00:00",
					EndTime: "2016-05-30T17:00:00"
				}]
			});

		var vm = t.createController();
		t.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].Shift[0].Name).toEqual('Phone');
	});

});
