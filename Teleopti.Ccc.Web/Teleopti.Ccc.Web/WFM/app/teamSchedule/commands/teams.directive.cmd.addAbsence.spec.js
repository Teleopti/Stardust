describe('teamschedule add absence diretive test', function () {
	'use strict';

	var fakeAbsenceService,
		fakeScheduleManagementSvc,
		fakePermissions,
		scheduleHelper,
		$compile,
		$rootScope,
		$httpBackend,
		fakePersonSelectionService,
		serviceDateFormatHelper,
		fakeCommandCheckService,
		currentUserInfo;

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(function () {
		fakeAbsenceService = new FakePersonAbsence();
		fakeScheduleManagementSvc = new FakeScheduleManagementService();
		fakePermissions = new FakePermissions();
		scheduleHelper = new FakeScheduleHelper();
		fakePersonSelectionService = new FakePersonSelectionService();
		fakeCommandCheckService = new FakeCommandCheckService();
		currentUserInfo = new FakeCurrentUserInfo();
		module(function ($provide) {
			$provide.service('PersonAbsence', function () {
				return fakeAbsenceService;
			});
			$provide.service('ScheduleManagement', function () {
				return fakeScheduleManagementSvc;
			});
			$provide.service('ScheduleHelper', function () {
				return scheduleHelper;
			});
			$provide.service('teamsPermissions', function () {
				return fakePermissions;
			});
			$provide.service('PersonSelection', function () {
				return fakePersonSelectionService;
			});
			$provide.service('CommandCheckService', function () {
				return fakeCommandCheckService;
			});
			$provide.service('CurrentUserInfo', function () {
				return currentUserInfo;
			});
		});
	});


	beforeEach(inject(function (_$rootScope_, _$compile_, _$httpBackend_,_serviceDateFormatHelper_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET('../ToggleHandler/AllToggles').respond(200, 'mock');
		serviceDateFormatHelper = _serviceDateFormatHelper_;
	}));

	it('add-absence should render correctly', function () {
		var result = setUp();
		expect(result.commandScope.vm).not.toBeNull();
	});

	it('should display full day absence check box with only full day absence permission', function () {
		fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: false, IsAddFullDayAbsenceAvailable: true });

		var result = setUp(new Date('2015-01-01 10:00:00'));
		var checkBoxInput = result.container[0].querySelectorAll('md-checkbox');
		expect(angular.element(checkBoxInput[0]).hasClass("md-checked")).toBe(true);
		expect(checkBoxInput[0].disabled).toBe(true);
	});

	it('should not display full day absence check box with only intraday absence permission', function () {
		fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: true, IsAddFullDayAbsenceAvailable: false });

		var result = setUp(new Date('2015-01-01 10:00:00'));
		var checkBoxInput = result.container[0].querySelectorAll('md-checkbox');
		expect(checkBoxInput.length).toBe(0);
	});

	it('should not allow to add intraday absence when startime is early or equal to endtime', function () {
		fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: true, IsAddFullDayAbsenceAvailable: false });

		var result = setUp(new Date('2015-01-01 10:00:00'));
		var vm = result.commandScope.vm;
		vm.timeRange = {};
		vm.timeRange.startTime = new Date('2015-01-01 10:00:00');
		vm.timeRange.endTime = new Date('2015-01-01 10:00:00');
		vm.selectedAbsenceId = getAvailableAbsenceTypes()[0].Id;
		vm.isFullDayAbsence = false;
		vm.selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				ScheduleStartTime: null,
				ScheduleEndTime: null
			}];
		vm.getCurrentTimezone = function () { return 'Europe/Stockholm'; };
		fakePersonSelectionService.setFakeCheckedPersonInfoList(vm.selectedAgents);
		vm.containerCtrl.scheduleManagementSvc.setPersonScheduleVm('agent1', {
			Date: '2015-01-01',
			PersonId: 'agent1',
			Timezone: {
				IanaId: 'Europe/Stockholm'
			},
			Shifts: [
				{
					Date: '2015-01-01',
					Projections: [
					],
					ProjectionTimeRange: null
				}]
		});
		result.commandScope.$apply();
		var applyButton = result.container[0].querySelectorAll('#applyAbsence');
		expect(applyButton[0].disabled).toBe(true);
	});

	commonTestsInDifferentLocale();

	function commonTestsInDifferentLocale() {
		it('should handle default start and end time attribute', function () {
			scheduleHelper.setEarliestStartTime(new Date('2015-01-01 10:00:00'));

			fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: true, IsAddFullDayAbsenceAvailable: true });

			var result = setUp(moment('2015-01-01T00:00:00').toDate());

			var startDateString = result.container[0].querySelectorAll('team-schedule-datepicker input')[0].value,
				endDateString = result.container[0].querySelectorAll('team-schedule-datepicker input')[1].value,
				startTimeString = result.container[0].querySelectorAll('.uib-timepicker input')[0].value
					+ result.container[0].querySelectorAll('.uib-timepicker input')[1].value,

				endTimeString = result.container[0].querySelectorAll('.uib-timepicker input')[2].value
					+ result.container[0].querySelectorAll('.uib-timepicker input')[3].value;

			expect(serviceDateFormatHelper.getDateOnly(new Date(startDateString))).toBe('2015-01-01');
			expect(serviceDateFormatHelper.getDateOnly(new Date(endDateString))).toBe('2015-01-01');

			expect(startTimeString).toBe('1000');
			expect(endTimeString).toBe('1100');
		});

		it('should apply add fullday absence with correct data', function () {

			fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: true, IsAddFullDayAbsenceAvailable: true });
			var result = setUp(moment('2015-01-01T00:00:00').toDate());
			var vm = result.commandScope.vm;
			vm.timeRange = {};
			vm.timeRange.startTime = new Date('2015-01-01');
			vm.timeRange.endTime = new Date('2015-01-01');
			vm.selectedAbsenceId = getAvailableAbsenceTypes()[0].Id;
			vm.isFullDayAbsence = true;
			vm.selectedAgents = [
				{
					PersonId: 'agent1',
					Name: 'agent1',
					ScheduleStartTime: null,
					ScheduleEndTime: null
				}];
			vm.getCurrentTimezone = function () { return 'Europe/Stockholm'; };
			fakePersonSelectionService.setFakeCheckedPersonInfoList(vm.selectedAgents);
			vm.containerCtrl.scheduleManagementSvc.setPersonScheduleVm('agent1', {
				Date: '2015-01-01',
				PersonId: 'agent1',
				Timezone: {
					IanaId: 'Europe/Stockholm'
				},
				Shifts: [
					{
						Date: '2015-01-01',
						Projections: [
						],
						ProjectionTimeRange: null
					}]
			});
			result.commandScope.$apply();

			var panel = result.container;
			var applyButton = panel[0].querySelector('#applyAbsence');
			applyButton.click();

			var lastAbsence = fakeAbsenceService.getAddAbsenceCalledWith();
			expect(lastAbsence.PersonIds[0]).toEqual('agent1');
			expect(lastAbsence.Date).toEqual('2015-01-01');

		});

		it('should apply add intraday absence with correct data', function () {

			fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: true, IsAddFullDayAbsenceAvailable: true });
			var result = setUp(moment('2015-01-01T00:00:00').toDate());
			var vm = result.commandScope.vm;
			vm.timeRange = {};
			vm.timeRange.startTime = new Date('2015-01-01 10:00:00');
			vm.timeRange.endTime = new Date('2015-01-01 10:30:00');
			vm.selectedAbsenceId = getAvailableAbsenceTypes()[0].Id;
			vm.isFullDayAbsence = false;
			vm.selectedAgents = [
				{
					PersonId: 'agent1',
					Name: 'agent1',
					ScheduleStartTime: null,
					ScheduleEndTime: null
				}];
			vm.getCurrentTimezone = function () { return 'Europe/Stockholm'; };
			fakePersonSelectionService.setFakeCheckedPersonInfoList(vm.selectedAgents);
			vm.containerCtrl.scheduleManagementSvc.setPersonScheduleVm('agent1', {
				Date: '2015-01-01',
				PersonId: 'agent1',
				Timezone: {
					IanaId: 'Europe/Stockholm'
				},
				Shifts: [
					{
						Date: '2015-01-01',
						Projections: [],
						ProjectionTimeRange: {
							Start: '2015-01-01 08:00',
							End: '2015-01-01 17:00'
						}
					}]
			});
			result.commandScope.$apply();

			var panel = result.container;
			var applyButton = panel[0].querySelector('#applyAbsence');
			applyButton.click();

			var lastAbsence = fakeAbsenceService.getAddAbsenceCalledWith();
			expect(lastAbsence.PersonIds[0]).toEqual('agent1');
			expect(lastAbsence.Date).toEqual('2015-01-01');
			expect(lastAbsence.Start).toEqual('2015-01-01T10:00');
			expect(lastAbsence.End).toEqual('2015-01-01T10:30');
		});
	}

	describe('in locale ar-AE', function () {
		beforeAll(function () {
			moment.locale('ar-AE');
		});

		afterAll(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});

	describe('in locale fa-IR', function () {
		beforeAll(function () {
			moment.locale('fa-IR');
		});

		afterAll(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});


	function setUp(inputDate, inputConfigurations, timeZone) {
		var date, configurations;
		var html = '<teamschedule-command-container date="curDate" configurations="configurations" timezone="timezone"></teamschedule-command-container>';
		var scope = $rootScope.$new();

		if (inputDate == null)
			date = moment('2016-06-15').toDate();
		else
			date = inputDate;

		if (inputConfigurations == null) {
			configurations = {};
		} else {
			configurations = inputConfigurations;
		}

		scope.curDate = date;
		scope.configurations = configurations;
		fakeAbsenceService.setAvailableAbsenceTypes(getAvailableAbsenceTypes());
		scope.timezone = timeZone || "Europe/Stockholm";

		var container = $compile(html)(scope);
		scope.$apply();

		var vm = container.isolateScope().vm;
		vm.setReady(true);
		vm.setActiveCmd('AddAbsence');
		scope.$apply();

		var commandScope = angular.element(container[0].querySelector('.add-absence')).scope();

		var obj = {
			container: container,
			commandScope: commandScope,
			scope: scope
		};
		//document.body.append(container[0])

		return obj;
	}

	function getAvailableAbsenceTypes() {
		return [
			{
				Id: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6',
				Name: 'Sick'
			},
			{
				Id: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6',
				Name: 'Holiday'
			}
		];
	}


	function FakeScheduleManagementService() {

		this.schedules = function () {
			return null;
		};

		this.newService = function () {
			return new FakeScheduleManagementService();
		};

		var savedPersonScheduleVm = {};

		this.setPersonScheduleVm = function (personId, vm) {
			savedPersonScheduleVm[personId] = vm;
		}

		this.findPersonScheduleVmForPersonId = function (personId) {
			return savedPersonScheduleVm[personId];
		}
	}

	function FakeScheduleHelper() {
		var earliestStartTime = null;
		var latestStartTime = null;
		var latestEndTime = null;

		this.setEarliestStartTime = function (date) {
			earliestStartTime = date;
		};

		this.setLatestStartTime = function (date) {
			latestStartTime = date;
		};

		this.setLatestEndTime = function (date) {
			latestEndTime = date;
		};

		this.getEarliestStartOfSelectedSchedules = function () {
			return earliestStartTime;
		};

		this.getLatestStartOfSelectedSchedules = function () {
			return latestStartTime;
		};

		this.getLatestPreviousDayOvernightShiftEnd = function () {
			return latestEndTime;
		};
	}

	function FakePermissions() {
		var _permissions = {}

		this.all = function () {
			return _permissions;
		}

		this.setPermissions = function (permissions) {
			_permissions = permissions;
		}
	}

	function FakePersonAbsence() {
		var availableAbsenceTypes = [];
		var targetAbsence = null;
		var fakeResponse = { data: [] };

		this.loadAbsences = function () {
			return {
				then: function (cb) {
					cb(availableAbsenceTypes);
				}
			};
		};

		this.addAbsence = function (input) {
			targetAbsence = input;
			return {
				then: (function (cb) {
					cb(fakeResponse);
				})
			};
		};
		
		this.getAddAbsenceCalledWith = function () {
			return targetAbsence;
		};

		this.setAvailableAbsenceTypes = function (absences) {
			availableAbsenceTypes = absences;
		};
	}

	function FakePersonSelectionService() {
		var fakePersonList = [];

		this.setFakeCheckedPersonInfoList = function (input) {
			fakePersonList = input;
		}

		this.getCheckedPersonInfoList = function () {
			return fakePersonList;
		}
	}
	function FakeCommandCheckService() {
		var checkStatus = false;

		
		this.getCommandCheckStatus = function () {
			return checkStatus;
		}

		this.resetCommandCheckStatus = function () {
			checkStatus = false;
		}

		
		this.checkPersonalAccounts = function (requestedData) {
			return {
				then: function (cb) {
					cb(requestedData);
				}
			}
		};
	}

	function FakeCurrentUserInfo() {
		this.CurrentUserInfo = function () {
			return {
				DefaultTimeZone: "Europe/Stockholm"
			};
		};
	}

});