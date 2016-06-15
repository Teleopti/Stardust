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

	it('should set pause to true when pressing button', function() {

		$controllerBuilder.createController()
			.apply('agentsInAlarm = true')
			.apply(function() {
				scope.togglePolling();
			});

		expect(scope.pause).toBe(true);
	});

	it('should stop polling agent states when paused', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TimeInState: 15473
			});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false')
			.wait(5000)
			.apply('pause = true');

		$fakeBackend.clearStates();
		$interval.flush(5000);

		expect(scope.agents[0].TimeInState).toEqual(15473);
	});

	it('should restart polling when unpausing', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		});

		var c = $controllerBuilder.createController()
			.apply('agentsInAlarm = false')
			.apply('pause = true')
			.wait(5000)
			.apply('pause = false');

		$fakeBackend.withState({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TimeInState: 15473
		})

		expect(scope.agents[0].TimeInState).toBe(undefined)

		c.wait(5000);
		expect(scope.agents[0].TimeInState).toEqual(15473)
	});

	it('should display time from when paused', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withTime('2016-06-13T16:00:00')
			.withAgent({
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			});

		$controllerBuilder.createController()
			.wait(5000)
			.apply(function() {
				scope.togglePolling();
			})
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
		var c = $controllerBuilder.createController()
			.wait(5000)
			.apply(function() {
				scope.togglePolling();
			})
			.apply(function() {
				scope.togglePolling();
			});

		expect(scope.pausedAt).toBeNull();
	});

	it('should trigger notice when pausing', function() {
		spyOn(NoticeService, 'warning');
		$controllerBuilder
			.createController()
			.apply(function() {
				scope.togglePolling();
			})

		expect(NoticeService.warning).toHaveBeenCalled();
	});

	it('should trigger notice when unpausing', function() {
		spyOn(NoticeService, 'info')
		$controllerBuilder
			.createController()
			.apply('pause = true')
			.apply(function() {
				scope.togglePolling();
			})

		expect(NoticeService.info).toHaveBeenCalled();
	});

	it('should destroy notice when unpausing', function() {
		var destroyer = {
			destroy: function() {}
		};
		spyOn(NoticeService, 'warning').and.returnValue(destroyer);
		spyOn(destroyer, 'destroy');

		var c = $controllerBuilder
			.createController()
			.apply(function() {
				scope.togglePolling();
			})
			.apply(function() {
				scope.togglePolling();
			});

		expect(destroyer.destroy).toHaveBeenCalled();
	})

	it('should display time in notice when pausing', function() {
		$fakeBackend.withTime('2016-06-15T09:00:46');
		spyOn(NoticeService, 'warning');
		$controllerBuilder
			.createController()
			.apply(function() {
				scope.togglePolling();
			})

		expect(NoticeService.warning.calls.mostRecent().args[0]).toContain("2016-06-15 09:00:46");
	});

});
