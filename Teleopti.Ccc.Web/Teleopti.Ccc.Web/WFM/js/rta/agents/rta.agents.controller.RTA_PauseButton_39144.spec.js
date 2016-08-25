'use strict';
describe('RtaAgentsCtrlPauseButton_39144', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		scope,
		NoticeService;

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
		$fakeBackend.withToggle('RTA_PauseButton_39144');

		scope = $controllerBuilder.setup('RtaAgentsCtrl');

	}));

	it('should stop polling agent states when paused', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Phone"
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false')
			.wait(5000)
			.apply('pause = true')
			.apply(function() {
				$fakeBackend
					.clearStates()
					.withState({
						PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
						State: "Ready"
					})
			})
			.wait(5000);

		expect(scope.agents[0].State).toEqual("Phone");
	});

	it('should restart polling when unpausing', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false')
			.apply('pause = true')
			.wait(5000)
			.apply('pause = false')
			.apply(function() {
				$fakeBackend.withState({
					PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
					State: "Ready"
				})
			})
			.wait(5000);

		expect(scope.agents[0].State).toEqual("Ready")
	});

	it('should display time from when paused', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withTime('2016-06-13T16:00:00')
			.withAgent({
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			});

		$controllerBuilder.createController()
			.wait(5000)
			.apply('pause = true')
			.apply(function() {
				$fakeBackend.withTime('2016-06-13T16:00:05')
			})
			.wait(5000);

		expect(scope.pausedAt).toEqual('2016-06-13 16:00:00');
	});

	it('should not display time when not paused', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withTime('2016-06-13T16:00')
			.withAgent({
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			});

		$controllerBuilder.createController()
			.wait(5000)
			.apply('pause = true')
			.apply('pause = false');

		expect(scope.pausedAt).toBeNull();
	});

	it('should trigger notice when pausing', function() {
		spyOn(NoticeService, 'info');
		$controllerBuilder
			.createController()
			.apply('pause = true');

		expect(NoticeService.info).toHaveBeenCalled();
	});

	it('should trigger notice when unpausing', function() {
		spyOn(NoticeService, 'info')
		$controllerBuilder
			.createController()
			.apply('pause = true')
			.apply('pause = false');

		expect(NoticeService.info).toHaveBeenCalled();
	});

	it('should destroy notice when unpausing', function() {
		var destroyer = {
			destroy: function() {}
		};
		spyOn(NoticeService, 'info').and.returnValue(destroyer);
		spyOn(destroyer, 'destroy');

		$controllerBuilder
			.createController()
			.apply('pause = true')
			.apply('pause = false');

		expect(destroyer.destroy).toHaveBeenCalled();
	})

	xit('should display time in notice when pausing', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withTime('2016-06-15T09:00:46')
			.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			});
		spyOn(NoticeService, 'info');
		$controllerBuilder
			.createController()
			.apply('pause = true');

		expect(NoticeService.info.calls.mostRecent().args[0]).toContain("2016-06-15 09:00:46");
	});

	it('should not update when paused and toggling agents in alarm', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Phone"
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false')
			.wait(5000)
			.apply('pause = true')
			.apply(function() {
				$fakeBackend.clearStates()
					.withState({
						PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
						TimeInAlarm: 1,
						State: "Ready"
					})
			})
			.apply('agentsInAlarm = true');

		expect(scope.agents[0].State).toEqual("Phone");
	});

	it('should sort by time in alarm when paused and toggling agents in alarm', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				Name: "Asley Andeen",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withAgent({
				PersonId: "164abe5d-ce1a-48ee-ba3a-9b5e015b2585",
				Name: "Dmitry Pavlov",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Phone",
				TimeInAlarm: 70
			})
			.withState({
				PersonId: "164abe5d-ce1a-48ee-ba3a-9b5e015b2585",
				State: "Phone",
				TimeInAlarm: 90
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false')
			.wait(5000)
			.apply('pause = true')
			.apply('agentsInAlarm = true');

		expect(scope.filteredData[0].Name).toEqual("Dmitry Pavlov");
		expect(scope.filteredData[1].Name).toEqual("Asley Andeen");
	});

});
