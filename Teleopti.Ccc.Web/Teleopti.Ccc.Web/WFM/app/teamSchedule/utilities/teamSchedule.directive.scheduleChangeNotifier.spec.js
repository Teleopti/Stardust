describe('schedule change notifier test', function() {
	var $compile,
		$rootScope,
		mockSignalRBackendServer = {},
		teamsToggles;


	beforeEach(function() {
		module('wfm.teamSchedule');
		module(function($provide) {
			$provide.service('signalRSVC', setupMockSignalRService);
			$provide.service('teamsToggles', setupMockTeamsTogglesService);
			$provide.service('ScheduleManagement', setupFakeScheduleManagement);
		});
	});

	beforeEach(inject(function (_$rootScope_, _$compile_, _teamsToggles_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		teamsToggles = _teamsToggles_;
	}));

	it("should reload schedule when schedule changed by others", inject(function () {

		var html =
			'<schedule-change-notifier schedule-date="scheduleDate" last-command-track-id="commandId" on-notification="cb"></schedule-change-notifier>';
		var scope = $rootScope.$new();

		scope.scheduleDate = new Date('2015-10-26');
		scope.commandId = '';

		var cbWithPersonIds = [];

		scope.cb = function(personIds) {
			cbWithPersonIds = personIds;
		}

		$compile(html)(scope);
		scope.$apply();
		
		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "221B-Baker-SomeoneElse",
				"StartDate": "D2015-10-25T00:00:00",
				"EndDate": "D2015-10-27T00:00:00"
			}
		]);

		expect(cbWithPersonIds[0]).toEqual("221B-Baker-SomeoneElse");
	}));


	it("should not reload schedule when other people schedule changed", inject(function () {
		var html =
			'<schedule-change-notifier schedule-date="scheduleDate" last-command-track-id="commandId" on-notification="cb"></schedule-change-notifier>';
		var scope = $rootScope.$new();

		scope.scheduleDate = new Date('2015-10-26');
		scope.commandId = '';

		var cbInvoked = false;
		scope.cb = function () {
			cbInvoked = true;
		}

		$compile(html)(scope);
		scope.$apply();
	
		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "221B-Baker-otherPeople",
				"StartDate": "D2015-10-25T00:00:00",
				"EndDate": "D2015-10-27T00:00:00"
			}
		]);

		expect(cbInvoked).toBeFalsy();
	}));

	it("should not reload schedule when schedule changed beyond the date", inject(function () {
		var html =
			'<schedule-change-notifier schedule-date="scheduleDate" last-command-track-id="commandId" on-notification="cb"></schedule-change-notifier>';
		var scope = $rootScope.$new();

		scope.scheduleDate = new Date('2015-10-29');
		scope.commandId = '';

		var cbInvoked = false;
		scope.cb = function () {
			cbInvoked = true;
		}

		$compile(html)(scope);
		scope.$apply();

		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "221B-Baker-SomeoneElse",
				"StartDate": "D2015-10-25T00:00:00",
				"EndDate": "D2015-10-27T00:00:00"
			}
		]);

		expect(cbInvoked).toBeFalsy();
	}));

	it("should reload schedule when schedule changed from yesterday", inject(function () {

		var html =
			'<schedule-change-notifier schedule-date="scheduleDate" last-command-track-id="commandId" on-notification="cb"></schedule-change-notifier>';
		var scope = $rootScope.$new();

		scope.scheduleDate = new Date('2015-10-25');
		scope.commandId = '';

		var cbWithPersonIds = [];

		scope.cb = function (personIds) {
			cbWithPersonIds = personIds;
		}

		$compile(html)(scope);

		scope.$apply();

		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "221B-Baker-SomeoneElse",
				"StartDate": "D2015-10-24T00:00:00",
				"EndDate": "D2015-10-24T00:00:00"
			}
		]);

		expect(cbWithPersonIds[0]).toEqual("221B-Baker-SomeoneElse");
	}));


	it("should reload schedule when schedule changed from tomorrow", inject(function () {
		var html =
			'<schedule-change-notifier schedule-date="scheduleDate" last-command-track-id="commandId" on-notification="cb"></schedule-change-notifier>';
		var scope = $rootScope.$new();

		scope.scheduleDate = new Date('2015-10-25');
		scope.commandId = '';

		var cbWithPersonIds = [];

		scope.cb = function (personIds) {
			cbWithPersonIds = personIds;
		}

		$compile(html)(scope);

		scope.$apply();

		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "221B-Baker-SomeoneElse",
				"StartDate": "D2015-10-26T00:00:00",
				"EndDate": "D2015-10-27T00:00:00"
			}
		]);

		expect(cbWithPersonIds[0]).toEqual("221B-Baker-SomeoneElse");
	}));




	function setupMockSignalRService() {
		mockSignalRBackendServer.subscriptions = [];

		return {
			subscribe: function (options, eventHandler, errorHandler) {
				mockSignalRBackendServer.subscriptions.push(options);
				mockSignalRBackendServer.notifyClients = eventHandler;
			},
			subscribeBatchMessage: function (options, messageHandler, timeout) {
				mockSignalRBackendServer.notifyClients = messageHandler;
			}
		};
	}

	function setupFakeScheduleManagement() {
		return {
			groupScheduleVm: {
				Schedules: [
					{
						PersonId: "221B-Baker-SomeoneElse"
					}
				]
			}
		}
	}

	function setupMockTeamsTogglesService() {

		toggles = {			
		};

		return {
			all: function() {
				return toggles;
			},
			setToggle: function(toggle) {
				toggles[toggle] = true;
			}			
		};
	}
})