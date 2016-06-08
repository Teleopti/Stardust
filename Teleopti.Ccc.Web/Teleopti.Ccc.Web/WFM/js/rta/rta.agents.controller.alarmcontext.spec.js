'use strict';
describe('RtaAgentsCtrl', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		scope;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function() {
		module(function ($provide) {
			$provide.service('$stateParams', function () {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;

		scope = $controllerBuilder.setup('RtaAgentsCtrl');

		$fakeBackend.clear();

	}));

	[
	{
		time: "2016-05-26T00:30:00",
		expect: ["00:00", "01:00", "02:00", "03:00"]
	},
	{
		time: "2016-05-26T11:30:00",
		expect: ["11:00", "12:00", "13:00", "14:00"]
	},
	{
		time: "2016-05-26T15:30:00",
		expect: ["15:00", "16:00", "17:00", "18:00"]
	},
	{
		time: "2016-05-26T23:30:00",
		expect: ["23:00", "00:00", "01:00", "02:00"]
	}].forEach(function(example) {

		it('should display time line for ' + example.time, function () {
			stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
			$fakeBackend
				.withTime(example.time)
				.withAgent({
					PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
					TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				});

			$controllerBuilder.createController();

			expect(scope.timeline.length).toEqual(example.expect.length);
			example.expect.forEach(function (e, i) {
				expect(scope.timeline[i].Time).toEqual(e);
			});
		});

	});

	[
	{
		time: "2016-05-26T12:00:00",
		expect: ["25%", "50%", "75%", "100%"],
	},
	{
		time: "2016-05-26T12:30:00",
		expect: ["12.5%", "37.5%", "62.5%", "87.5%"],
	}
	].forEach(function (example) {

		it('should position time line for ' + example.time, function () {
			stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
			$fakeBackend
				.withTime(example.time)
				.withAgent({
					PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
					TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				});

			$controllerBuilder.createController();

			expect(scope.timeline.length).toEqual(example.expect.length);
			example.expect.forEach(function (e, i) {
				expect(scope.timeline[i].Offset).toEqual(e);
			});
		});

	});

	it('should display the width of the alarm', function () {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";

		$fakeBackend
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TimeInAlarm: 1800
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].AlarmWidth).toEqual("12.5%");
	});


	it('should cut alarm to fit schedule window', function () {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";

		$fakeBackend
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TimeInAlarm: 4000
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].AlarmWidth).toEqual("25%");
	});

	it('should display scheduled activity', function () {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";

		$fakeBackend
			.withTime("2016-05-26T12:00:00")
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				Shift: [
					{
						Color: "#80FF80",
						StartTime: "2016-05-26T12:00:00",
						EndTime: "2016-05-26T14:00:00"
					}
				]
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].Shift.length).toEqual(1);
		expect(scope.agents[0].Shift[0].Color).toEqual("#80FF80");
		expect(scope.agents[0].Shift[0].Offset).toEqual("25%");
		expect(scope.agents[0].Shift[0].Width).toEqual("50%");
	});

	it('should display all activities', function () {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";

		$fakeBackend
			.withTime("2016-05-26T09:00:00")
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				Shift: [
					{
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

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].Shift.length).toEqual(2);
		expect(scope.agents[0].Shift[0].Color).toEqual("#80FF80");
		expect(scope.agents[0].Shift[0].Offset).toEqual("0%");
		expect(scope.agents[0].Shift[0].Width).toEqual("50%");
		expect(scope.agents[0].Shift[1].Color).toEqual("#0000FF");
		expect(scope.agents[0].Shift[1].Offset).toEqual("50%");
		expect(scope.agents[0].Shift[1].Width).toEqual("50%");
	});

	it('should display all activities', function () {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";

		$fakeBackend
			.withTime("2014-01-21T12:45:00")
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				Shift: [
					{
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

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].Shift.length).toEqual(2);
	});

	it('should not display past activity before display window', function () {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend
			.withTime("2016-05-30T13:00:00")
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				Shift: [
					{
						Color: "#80FF80",
						StartTime: "2016-05-30T08:00:00",
						EndTime: "2016-05-30T12:00:00"
					}
				]
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].Shift.length).toEqual(0);
	});

	it('should not display future activities outside of display window', function () {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend
			.withTime("2016-05-30T08:00:00")
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				Shift: [
					{
						Color: "#80FF80",
						StartTime: "2016-05-30T15:00:00",
						EndTime: "2016-05-30T21:00:00"
					}
				]
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].Shift.length).toEqual(0);
	});

	it('should not display future activities outside of display window', function () {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend
			.withTime("2016-05-30T08:00:00")
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				Shift: [
					{
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

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].Shift.length).toEqual(1);
	});

	xit('should cut activities that are larger than display window', function () {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend
			.withTime("2016-05-30T11:00:00")
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				Shift: [
					{
						Color: "#80FF80",
						StartTime: "2016-05-30T08:00:00",
						EndTime: "2016-05-30T17:00:00"
					}
				]
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].Shift[0].Width).toEqual('100%');
	});

	it('should produce an activity name', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend
			.withTime("2016-05-30T11:00:00")
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				Shift: [
					{
						Name: 'Phone',
						StartTime: "2016-05-30T08:00:00",
						EndTime: "2016-05-30T17:00:00"
					}
				]
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].Shift[0].Name).toEqual('Phone');
	});
});
