describe('move shift component test',
	function () {
		'use strict';
		var $rootScope,
			$compile,
			personSelectionService,
			scheduleManagementSvc,
			activityService;

		personSelectionService = new FakePersonSelectionService();
		scheduleManagementSvc = new FakeScheduleManagementService();
		activityService = new FakeActivityService();

		beforeEach(module('wfm.templates'));
		beforeEach(module('wfm.teamSchedule'));

		beforeEach(module(function ($provide) {
			$provide.service('PersonSelection',
				function () {
					return personSelectionService;
				});
			$provide.service('ScheduleManagement',
				function () {
					return scheduleManagementSvc;
				});
			$provide.service('Toggle',
				function () {
					return {};
				});
			$provide.service('ActivityService',
				function () {
					return activityService;
				});
			$provide.service('CurrentUserInfo',
				function () {
					return {
						CurrentUserInfo: function () {
							return {
								DefaultTimeZone: "Etc/UTC",
								DefaultTimeZoneName: "Etc/UTC",
								DateFormatLocale: "en-GB"
							};
						}
					};
				});
		}));

		beforeEach(inject(function (_$rootScope_, _$compile_) {
			$rootScope = _$rootScope_;
			$compile = _$compile_;
		}));


		it('should display the error message and disable the button for one agent when in different timezone', function () {
			var timezone1 = {
				IanaId: 'Etc/UTC',
				DisplayName: 'UTC'
			};
			var currentTimezone = 'Europe/Berlin';

			var selectedAgents = [
				{
					PersonId: 'agent1',
					Checked: true,
					Name: 'agent1',
					ScheduleStartTime: '2016-06-15T08:00:00Z',
					ScheduleEndTime: '2016-06-15T17:00:00Z',
					SelectedActivities: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}
			];
			personSelectionService.setFakeSelectedPersonInfoList(selectedAgents);

			scheduleManagementSvc.setPersonScheduleVm('agent1',
				{
					Date: '2016-06-15',
					PersonId: 'agent1',
					Timezone: timezone1,
					Shifts: [
						{
							Date: '2016-06-15',
							Projections: [
								{
									Start: '2016-06-15 08:00',
									End: '2016-06-15 17:00',
									Minutes: 540
								}
							],
							ProjectionTimeRange: {
								Start: '2016-06-15 08:00',
								End: '2016-06-15 17:00'
							}
						}
					],
					ExtraShifts: []
				});
			var element = setUp(moment('2016-06-15').toDate(), currentTimezone).element;
			var applyButton = angular.element(element[0].querySelector(".move-shift .form-submit"));
			var errorMessage = element[0].querySelector(".move-shift .has-agent-in-different-timezone");
			expect(errorMessage).toBeTruthy();
			expect(applyButton.attr('disabled')).toBe('disabled');
		});

		it('should display the error message and disable the button for multiple agent when in different timezone', function () {
			var currentTimeZone = 'Europe/Berlin';
			var timezone1 = {
				IanaId: 'Etc/UTC',
				DisplayName: 'UTC'
			};
			var timezone2 = {
				IanaId: 'Europe/Berlin',
				DisplayName: 'UTC'
			};
			var selectedAgents = [
				{
					PersonId: 'agent1',
					Name: 'agent1',
					Checked: true,
					ScheduleStartTime: '2016-06-15T08:00:00Z',
					ScheduleEndTime: '2016-06-15T17:00:00Z',
					SelectedActivities: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}, {
					PersonId: 'agent2',
					Name: 'agent2',
					Checked: true,
					ScheduleStartTime: '2016-06-15T19:00:00Z',
					ScheduleEndTime: '2016-06-16T08:00:00Z',
					SelectedActivities: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}];
			personSelectionService.setFakeSelectedPersonInfoList(selectedAgents);

			scheduleManagementSvc.setPersonScheduleVm('agent1', {
				Date: '2016-06-15',
				PersonId: 'agent1',
				Timezone: timezone1,
				ExtraShifts: [],
				Shifts: [
					{
						Date: '2016-06-15',
						Projections: [
							{
								Start: '2016-06-15 08:00',
								End: '2016-06-15 17:00',
								Minutes: 540
							}],
						ProjectionTimeRange: {
							Start: '2016-06-15 08:00',
							End: '2016-06-15 17:00'
						}
					}]
			});
			scheduleManagementSvc.setPersonScheduleVm('agent2', {
				Date: '2016-06-15',
				PersonId: 'agent2',
				Timezone: timezone2,
				ExtraShifts: [],
				Shifts: [
					{
						Date: '2016-06-15',
						Projections: [
							{
								Start: '2016-06-15 08:00',
								End: '2016-06-15 17:00',
								Minutes: 540
							}],
						ProjectionTimeRange: {
							Start: '2016-06-15 08:00',
							End: '2016-06-15 17:00'
						}
					}]
			});

			var element = setUp(moment('2016-06-15').toDate(), currentTimeZone).element;
			var applyButton = angular.element(element[0].querySelector(".move-shift .form-submit"));
			var errorMessage = element[0].querySelector(".move-shift .has-agent-in-different-timezone");
			expect(errorMessage).toBeTruthy();
			expect(applyButton.attr('disabled')).toBe(undefined);
		});

		it('should display the error message and disable the button for one agent whose schedule is full day absence', function () {
			var timezone1 = {
				IanaId: 'Etc/UTC',
				DisplayName: 'UTC'
			};
			var currentTimezone = 'Etc/UTC';

			var selectedAgents = [
				{
					PersonId: 'agent1',
					Name: 'agent1',
					Checked: true,
					ScheduleStartTime: '2016-06-15T08:00:00Z',
					ScheduleEndTime: '2016-06-15T17:00:00Z',
					SelectedActivities: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}];
			personSelectionService.setFakeSelectedPersonInfoList(selectedAgents);

			scheduleManagementSvc.setPersonScheduleVm('agent1', {
				Date: '2016-06-15',
				PersonId: 'agent1',
				Timezone: timezone1,
				IsFullDayAbsence: true,
				Shifts: [
					{
						Date: '2016-06-15',
						Projections: [
							{
								Start: '2016-06-15 08:00',
								End: '2016-06-15 17:00',
								Minutes: 540
							}],
						ProjectionTimeRange: {
							Start: '2016-06-15 08:00',
							End: '2016-06-15 17:00'
						}
					}]
			});
			var element = setUp(moment('2016-06-15').toDate(), currentTimezone).element;
			var applyButton = angular.element(element[0].querySelector(".move-shift .form-submit"));
			var errorMessage = element[0].querySelector(".move-shift .invalid-agent");
			expect(errorMessage).toBeTruthy();
			expect(applyButton.attr('disabled')).toBe('disabled');
		});


		it('should display the error message and disable the button for one agent whose schedule is day off', function () {
			var timezone1 = {
				IanaId: 'Etc/UTC',
				DisplayName: 'UTC'
			};
			var currentTimezone = 'Etc/UTC';

			var selectedAgents = [
				{
					PersonId: 'agent1',
					Name: 'agent1',
					Checked: true,
					ScheduleStartTime: '2016-06-15T08:00:00Z',
					ScheduleEndTime: '2016-06-15T17:00:00Z',
					SelectedActivities: [],
					SelectedDayOffs: [
						{
							Date: '2016-06-15',
							DayOffName: 'Day Off'
						}
					]
				}
			];
			personSelectionService.setFakeSelectedPersonInfoList(selectedAgents);

			scheduleManagementSvc.setPersonScheduleVm('agent1', {
				Date: '2016-06-15',
				PersonId: 'agent1',
				Timezone: timezone1,
				IsFullDayAbsence: true,
				DayOffs: [
					{
						Date: '2016-06-15',
						DayOffName: 'Day Off'
					}]
			});
			var element = setUp(moment('2016-06-15').toDate(), currentTimezone).element;
			var applyButton = angular.element(element[0].querySelector(".move-shift .form-submit"));
			var errorMessage = element[0].querySelector(".move-shift .invalid-agent");
			expect(errorMessage).toBeTruthy();
			expect(applyButton.attr('disabled')).toBe('disabled');
		});

		commonTestsInDifferentLocale();

		function commonTestsInDifferentLocale() {
			it('should apply command only to checked agents', function () {
				var timezone1 = {
					IanaId: 'Etc/UTC',
					DisplayName: 'UTC'
				};
				var currentTimezone = 'Etc/UTC';

				var selectedAgents = [
					{
						PersonId: 'agent1',
						Name: 'agent1',
						Checked: true,
						ScheduleStartTime: '2016-06-15T08:00:00Z',
						ScheduleEndTime: '2016-06-15T17:00:00Z',
						SelectedActivities: ['472e02c8-1a84-4064-9a3b-9b5e015ab3c6'],
						SelectedDayOffs: []
					},
					{
						PersonId: 'agent2',
						Name: 'agent2',
						ScheduleStartTime: '2016-06-15T08:00:00Z',
						ScheduleEndTime: '2016-06-15T17:00:00Z',
						SelectedActivities: ['472e02c8-1a84-4064-9a3b-9b5e015ab3c6'],
						SelectedDayOffs: []
					}
				];
				personSelectionService.setFakeSelectedPersonInfoList(selectedAgents);

				scheduleManagementSvc.setPersonScheduleVm('agent1', {
					Date: '2016-06-15',
					PersonId: 'agent1',
					Timezone: timezone1,
					Shifts: [
						{
							Date: '2016-06-15',
							Projections: [
								{
									Start: '2016-06-15 08:00',
									End: '2016-06-15 17:00',
									Minutes: 540
								}],
							ProjectionTimeRange: {
								Start: '2016-06-15 08:00',
								End: '2016-06-15 17:00'
							}
						}],
					ExtraShifts: []
				});
				scheduleManagementSvc.setPersonScheduleVm('agent2', {
					Date: '2016-06-15',
					PersonId: 'agent2',
					Timezone: timezone1,
					Shifts: [
						{
							Date: '2016-06-15',
							Projections: [
								{
									Start: '2016-06-15 08:00',
									End: '2016-06-15 17:00',
									Minutes: 540
								}
							],
							ProjectionTimeRange: {
								Start: '2016-06-15 08:00',
								End: '2016-06-15 17:00'
							}
						}
					],
					ExtraShifts: []
				});
				var compiledResult = setUp(moment('2016-06-15').toDate(), currentTimezone);
				var element = compiledResult.element;
				var scope = compiledResult.scope;
				var ctrl = element.scope().$ctrl;
				ctrl.moveToTime = '2016-06-15 09:00';
				var applyButton = angular.element(element[0].querySelector(".move-shift .form-submit"));
				applyButton[0].click();
				scope.$apply();

				var lastRequestedData = activityService.lastRequestedData();
				expect(lastRequestedData.PersonIds.length).toBe(1);
				expect(lastRequestedData.PersonIds[0]).toBe('agent1');
				expect(lastRequestedData.Date).toBe('2016-06-15');
				expect(lastRequestedData.NewShiftStart).toBe('2016-06-15T09:00');
				expect(!!lastRequestedData.TrackedCommandInfo.TrackId).toBe(true);
			});

			it('should apply with correct time range based on the selected time zone', function () {
				var timezone1 = {
					IanaId: 'Etc/UTC',
					DisplayName: 'UTC'
				};
				var currentTimezone = 'Etc/UTC';

				var selectedAgents = [
					{
						PersonId: 'agent1',
						Name: 'agent1',
						Checked: true,
						ScheduleStartTime: '2018-03-25T01:00:00Z',
						ScheduleEndTime: '2018-03-25T23:00:00Z',
						SelectedActivities: ['472e02c8-1a84-4064-9a3b-9b5e015ab3c6'],
						SelectedDayOffs: []
					}
				];
				personSelectionService.setFakeSelectedPersonInfoList(selectedAgents);

				scheduleManagementSvc.setPersonScheduleVm('agent1', {
					Date: '2018-03-25',
					PersonId: 'agent1',
					Timezone: timezone1,
					Shifts: [
						{
							Date: '2018-03-25',
							Projections: [
								{
									Start: '2018-03-25 01:00',
									End: '2018-03-25 23:00',
									Minutes: 540
								}],
							ProjectionTimeRange: {
								Start: '2018-03-25 01:00',
								End: '2018-03-25 23:00'
							}
						}],
					ExtraShifts: []
				});
			
				var compiledResult = setUp(moment('2018-03-25').toDate(), currentTimezone);
				var element = compiledResult.element;
				var scope = compiledResult.scope;
				var ctrl = element.scope().$ctrl;
				ctrl.moveToTime = '2018-03-25 02:00';
				var applyButton = angular.element(element[0].querySelector(".move-shift .form-submit"));
				applyButton[0].click();
				scope.$apply();

				var lastRequestedData = activityService.lastRequestedData();
				expect(lastRequestedData.NewShiftStart).toBe('2018-03-25T02:00');
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
			beforeEach(function () {
				moment.locale('fa-IR');
			});

			afterEach(function () {
				moment.locale('en');
			});

			commonTestsInDifferentLocale();
		});

		function setUp(inputDate, timeZone) {
			var date;
			var html = '<teamschedule-command-container date="curDate" timezone="timezone"></teamschedule-command-container>';
			var scope = $rootScope.$new();
			if (inputDate == null)
				date = moment('2016-06-15').toDate();
			else
				date = inputDate;

			scope.curDate = date;
			scope.timezone = timeZone;

			var container = $compile(html)(scope);
			scope.$apply();

			var vm = container.isolateScope().vm;
			vm.setReady(true);
			vm.setActiveCmd('MoveShift');
			scope.$apply();

			var element = angular.element(container[0].querySelector(".move-shift"));
			return {
				element: element,
				scope: scope
			};
		}

		function FakePersonSelectionService() {
			var fakePersonList = [];

			this.setFakeSelectedPersonInfoList = function (input) {
				fakePersonList = input;
			}

			this.getCheckedPersonInfoList = function () {
				return fakePersonList.filter(function (p) { return p.Checked; });
			}

			this.getSelectedPersonIdList = function () {
				return fakePersonList.map(function (p) { return p.PersonId; });
			};
		}
		function FakeScheduleManagementService() {
			var savedPersonScheduleVm = {};

			this.setPersonScheduleVm = function (personId, vm) {
				savedPersonScheduleVm[personId] = vm;
			}

			this.findPersonScheduleVmForPersonId = function (personId) {
				return savedPersonScheduleVm[personId];
			}

			this.schedules = function () {
				return null;
			};

			this.newService = function () {
				return new FakeScheduleManagementService(savedPersonScheduleVm);
			};

			function FakeScheduleManagementService(personSchedules) {
				var savedPersonScheduleVm = personSchedules;

				this.setPersonScheduleVm = function (personId, vm) {
					savedPersonScheduleVm[personId] = vm;
				}

				this.findPersonScheduleVmForPersonId = function (personId) {
					return savedPersonScheduleVm[personId];
				}

				this.schedules = function () {
					return null;
				};
			}
		}

		function FakeActivityService() {
			var lastRequestData;

			this.lastRequestedData = function () { return lastRequestData; }

			this.moveShift = function (requestData) {
				lastRequestData = requestData;
				return {
					then: function (cb) { cb({ data: [] }); }
				}
			}
		}
	});