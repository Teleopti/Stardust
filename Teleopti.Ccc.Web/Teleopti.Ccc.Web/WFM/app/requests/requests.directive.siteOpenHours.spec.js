'use strict';
describe('requestsSiteOpenHoursDirectiveTests', function() {
	var $compile, $rootScope;

	var requestsNotificationService, requestsDataService, requestsPermissions;

	beforeEach(function() {
		module('wfm.templates');
		module('wfm.requests');

		requestsNotificationService = new FakeRequestsNotificationService();
		requestsDataService = new FakeRequestsDataService();
		requestsPermissions = new FakeRequestsPermissions();

		module(function($provide) {
			$provide.service('requestsNotificationService', function() {
				return requestsNotificationService;
			});
			$provide.service('requestsDataService', function() {
				return requestsDataService;
			});
			$provide.service('requestsPermissions', function() {
				return requestsPermissions;
			});
			$provide.service('workingHoursPickerDirective', function() {
				return null;
			});
			$provide.service('showWeekdaysFilter', function() {
				return null;
			});
		});
	});

	beforeEach(inject(function(_$rootScope_, _$compile_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
	}));

	it('should get open hours and open modal', function() {
		var openHoursHandleResult = [{ Id: '1', OpenHours: [] }, { Id: '2', OpenHours: [] }];
		requestsDataService.setOpenHoursHandleResult(openHoursHandleResult);

		var test = setUpTarget();

		expect(test.targetScope.sites.length).toEqual(2);
	});

	it('should save open hours and close modal', function() {
		var test = setUpTarget();
		test.targetScope.sites = [
			{
				Id: '1',
				Name: 'BTS',
				NumberOfAgents: 10,
				OpenHours: [
					{
						EndTime: '',
						StartTime: '',
						WeekDaySelections: []
					}
				]
			}
		];

		expect(requestsDataService.getDataBeforeSend()).not.toBeDefined();
		expect(test.targetScope.shouldShowSites).not.toBeDefined();

		test.targetScope.save();

		expect(requestsDataService.getDataBeforeSend().length).toEqual(1);
		expect(test.targetScope.shouldShowSites).toBeFalsy();
	});

	it('should notify operation result', function() {
		var test = setUpTarget();
		test.targetScope.sites = [
			{
				Id: '1',
				Name: 'BTS',
				NumberOfAgents: 10,
				OpenHours: [
					{
						EndTime: '',
						StartTime: '',
						WeekDaySelections: []
					}
				]
			}
		];
		var openHoursHandleResult = 4;
		requestsDataService.setOpenHoursHandleResult(openHoursHandleResult);

		test.targetScope.save();

		expect(requestsNotificationService.getNotificationResult()).toEqual(openHoursHandleResult);
	});

	it('should format data before saving open hours', function() {
		var test = setUpTarget();
		var openHoursFormatResult = [
			{
				OpenHours: [
					{
						EndTime: '17:00:00',
						StartTime: '08:00:00',
						WeekDay: 1
					}
				]
			}
		];
		test.targetScope.sites = [
			{
				OpenHours: [
					{
						EndTime: '2016-08-09 17:00',
						StartTime: '2016-08-09 08:00',
						WeekDaySelections: [
							{ Checked: true, WeekDay: 1 },
							{ Checked: false, WeekDay: 2 },
							{ Checked: false, WeekDay: 3 },
							{ Checked: false, WeekDay: 4 },
							{ Checked: false, WeekDay: 5 },
							{ Checked: false, WeekDay: 6 },
							{ Checked: false, WeekDay: 7 }
						]
					}
				]
			}
		];

		test.targetScope.save();

		expect(requestsDataService.getDataBeforeSend()[0].OpenHours.WeekDay).toEqual(
			openHoursFormatResult[0].OpenHours.WeekDay
		);
		expect(requestsDataService.getDataBeforeSend()[0].OpenHours.EndTime).toEqual(
			openHoursFormatResult[0].OpenHours.EndTime
		);
		expect(requestsDataService.getDataBeforeSend()[0].OpenHours.StartTime).toEqual(
			openHoursFormatResult[0].OpenHours.StartTime
		);
		expect(requestsDataService.getDataBeforeSend()[0].OpenHours.length).toEqual(7);
	});

	it('should deformat data after get open hours', function() {
		var openHoursHandleResult = [
			{
				OpenHours: [
					{
						EndTime: '17:00:00',
						StartTime: '08:00:00',
						WeekDay: 1
					}
				]
			}
		];
		requestsDataService.setOpenHoursHandleResult(openHoursHandleResult);

		var test = setUpTarget();

		expect(test.targetScope.sites[0].OpenHours[0].EndTime.getHours()).toEqual(17);
		expect(test.targetScope.sites[0].OpenHours[0].EndTime.getMinutes()).toEqual(0);
		expect(test.targetScope.sites[0].OpenHours[0].StartTime.getHours()).toEqual(8);
		expect(test.targetScope.sites[0].OpenHours[0].StartTime.getMinutes()).toEqual(0);
		expect(test.targetScope.sites[0].OpenHours.length).toEqual(1);
	});

	it('should deformat closed week day', function() {
		var openHoursHandleResult = [
			{
				OpenHours: [
					{
						EndTime: '17:00:00',
						StartTime: '08:00:00',
						WeekDay: 1,
						IsClosed: true
					}
				]
			}
		];
		requestsDataService.setOpenHoursHandleResult(openHoursHandleResult);

		var test = setUpTarget();
		expect(test.targetScope.sites[0].OpenHours.length).toEqual(1);
	});

	it('should not deformat default time period', function() {
		var openHoursHandleResult = [
			{
				OpenHours: [
					{
						EndTime: '00:00:00',
						StartTime: '00:00:00',
						WeekDay: 1,
						IsClosed: true
					},
					{
						EndTime: '00:00:00',
						StartTime: '23:00:00',
						WeekDay: 2,
						IsClosed: true
					}
				]
			}
		];
		requestsDataService.setOpenHoursHandleResult(openHoursHandleResult);

		var test = setUpTarget();
		expect(test.targetScope.sites[0].OpenHours.length).toEqual(1);
	});

	it('should notify nothing changed', function() {
		var openHoursHandleResult = [{ id: 1, OpenHours: [] }];
		requestsDataService.setOpenHoursHandleResult(openHoursHandleResult);
		var test = setUpTarget();

		test.targetScope.save();
		expect(requestsNotificationService.getNotificationResult()).toEqual('nothing changed');
	});

	function setUpTarget() {
		requestsPermissions.set({ HasEditSiteOpenHoursPermission: true });
		var targetScope = $rootScope.$new();
		var targetElem = $compile('<requests-site-open-hours></requests-site-open-hours>')(targetScope);
		targetScope.$digest();
		return { targetScope: targetElem.isolateScope().requestsSiteOpenHours, targetElem: targetElem };
	}

	function FakeRequestsNotificationService() {
		var _notificationResult;
		this.notifySaveSiteOpenHoursSuccess = function(persistedSitesCount) {
			_notificationResult = persistedSitesCount;
		};
		this.getNotificationResult = function() {
			return _notificationResult;
		};
		this.notifyNothingChanged = function() {
			_notificationResult = 'nothing changed';
		};
	}

	function FakeRequestsDataService() {
		var _handleResult, _dataBeforeSend;
		var successCallback = function(callback) {
			callback(_handleResult);
		};
		this.setOpenHoursHandleResult = function(handleResult) {
			_handleResult = handleResult;
		};
		this.getSitesPromise = function() {
			return { success: successCallback };
		};
		this.maintainOpenHoursPromise = function(sites) {
			_dataBeforeSend = sites;
			return { success: successCallback };
		};
		this.getDataBeforeSend = function() {
			return _dataBeforeSend;
		};
	}

	function FakeRequestsPermissions() {
		var permissions;

		this.set = function setPermissions(data) {
			permissions = data;
		};

		this.all = function getPermissions() {
			return permissions;
		};
	}
});
