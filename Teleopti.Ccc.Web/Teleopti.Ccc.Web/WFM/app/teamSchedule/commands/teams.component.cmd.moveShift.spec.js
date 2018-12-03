describe('<move-shift>',
	function () {
		'use strict';
		var $rootScope,
			$compile,
			scheduleManagement,
			activityService,
			personSelection;

		activityService = new FakeActivityService();

		beforeEach(module('wfm.templates'));
		beforeEach(module('wfm.teamSchedule'));

		beforeEach(module(function ($provide) {
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

		beforeEach(inject(function (_$rootScope_, _$compile_, ScheduleManagement, PersonSelection) {
			$rootScope = _$rootScope_;
			$compile = _$compile_;
			scheduleManagement = ScheduleManagement;
			personSelection = PersonSelection;
		}));


		it('should display the error message and disable the button for one agent when in different timezone', function () {
			scheduleManagement.resetSchedules(
				[{
					Date: '2016-06-15',
					PersonId: 'agent1',
					Timezone: {
						IanaId: 'Etc/UTC'
					},
					Projection: [
						{
							"ShiftLayerIds": ["layer1"],
							StartInUtc: '2016-06-15 08:00',
							EndInUtc: '2016-06-15 17:00'
						}
					]
				}]
				, '2016-06-15');
			var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];

			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			personSelection.toggleAllPersonProjections(personSchedule, '2016-06-15');

			var element = setUp(moment('2016-06-15').toDate(), 'Europe/Berlin').element;
			var applyButton = angular.element(element[0].querySelector(".move-shift .form-submit"));
			var errorMessage = element[0].querySelector(".move-shift .has-agent-in-different-timezone");
			expect(errorMessage).toBeTruthy();
			expect(applyButton.attr('disabled')).toBe('disabled');
		});

		it('should display the error message but do not disable the button for multiple agent when in different timezone', function () {
			scheduleManagement.resetSchedules(
				[{
					Date: '2016-06-15',
					PersonId: 'agent1',
					Timezone: {
						IanaId: 'Etc/UTC'
					},
					Projection: [
						{
							ShiftLayerIds: ["layer1"],
							StartInUtc: '2016-06-15 08:00',
							EndInUtc: '2016-06-15 17:00'
						}
					]
				},
				{
					Date: '2016-06-15',
					PersonId: 'agent2',
					Timezone: {
						IanaId: 'Europe/Berlin'
					},
					Projection: [
						{
							ShiftLayerIds: ["layer1"],
							StartInUtc: '2016-06-15 08:00',
							EndInUtc: '2016-06-15 17:00'
						}
					]
				}]
				, '2016-06-15');

			scheduleManagement.groupScheduleVm.Schedules.forEach(function (personSchedule) {
				personSchedule.IsSelected = true;
				personSelection.updatePersonSelection(personSchedule);
				personSelection.toggleAllPersonProjections(personSchedule, '2016-06-15');
			});

			var element = setUp(moment('2016-06-15').toDate(), 'Europe/Berlin').element;
			var applyButton = angular.element(element[0].querySelector(".move-shift .form-submit"));
			var errorMessage = element[0].querySelector(".move-shift .has-agent-in-different-timezone");
			expect(errorMessage).toBeTruthy();
			expect(applyButton.attr('disabled')).toBe(undefined);
		});

		it('should display the error message and disable the button for one agent whose schedule is full day absence', function () {
			scheduleManagement.resetSchedules(
				[{
					Date: '2016-06-15',
					PersonId: 'agent1',
					Timezone: {
						IanaId: 'Etc/UTC'
					},
					Projection: [
						{
							"ShiftLayerIds": ["layer1"],
							StartInUtc: '2016-06-15 08:00',
							EndInUtc: '2016-06-15 17:00'
						}
					],
					IsFullDayAbsence: true,
				}]
				, '2016-06-15');
			var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];

			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			personSelection.toggleAllPersonProjections(personSchedule, '2016-06-15');

			var element = setUp(moment('2016-06-15').toDate(), 'Etc/UTC').element;
			var applyButton = angular.element(element[0].querySelector(".move-shift .form-submit"));
			var errorMessage = element[0].querySelector(".move-shift .invalid-agent");
			expect(errorMessage).toBeTruthy();
			expect(applyButton.attr('disabled')).toBe('disabled');
		});

		it('should display the error message and disable the button for one agent whose schedule is day off', function () {
			scheduleManagement.resetSchedules(
				[{
					Date: '2016-06-15',
					PersonId: 'agent1',
					Timezone: {
						IanaId: 'Etc/UTC'
					},
					Projection: [],
					DayOffs: [{ Date: '2016-06-15' }]
				}] , '2016-06-15');
			var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];

			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			personSelection.toggleAllPersonProjections(personSchedule, '2016-06-15');

			var element = setUp(moment('2016-06-15').toDate(), 'Etc/UTC').element;
			var applyButton = angular.element(element[0].querySelector(".move-shift .form-submit"));
			var errorMessage = element[0].querySelector(".move-shift .invalid-agent");
			expect(errorMessage).toBeTruthy();
			expect(applyButton.attr('disabled')).toBe('disabled');
		});

		commonTestsInDifferentLocale();

		function commonTestsInDifferentLocale() {
			it('should apply command to checked agents', function () {
				scheduleManagement.resetSchedules(
					[{
						Date: '2016-06-15',
						PersonId: 'agent1',
						Timezone: {
							IanaId: 'Etc/UTC'
						},
						Projection: [
							{
								"ShiftLayerIds": ["layer1"],
								StartInUtc: '2016-06-15 08:00',
								EndInUtc: '2016-06-15 17:00'
							}
						]
					}], '2016-06-15');
				var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];

				personSchedule.IsSelected = true;
				personSelection.updatePersonSelection(personSchedule);
				personSelection.toggleAllPersonProjections(personSchedule, '2016-06-15');

				var result = setUp(moment('2016-06-15').toDate(), 'Etc/UTC');
				var element = result.element;
				var scope = element.scope();

				var ctrl = scope.$ctrl;
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

			it('should apply with correct time range based on the selected time zone ', function () {

				scheduleManagement.resetSchedules(
					[{
						Date: '2018-03-25',
						PersonId: 'agent1',
						Timezone: {
							IanaId: 'Europe/Berlin'
						},
						Projection: [
							{
								StartInUtc: '2018-03-25 01:00',
								EndInUtc: '2018-03-25 05:00'
							}]
					}], '2018-03-25');
				var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];

				personSchedule.IsSelected = true;
				personSelection.updatePersonSelection(personSchedule);
				personSelection.toggleAllPersonProjections(personSchedule, '2018-03-25');

				var result = setUp(moment('2018-03-25').toDate(), 'Europe/Berlin');
				var element = result.element;
				var scope = element.scope();

				var ctrl = scope.$ctrl;
				ctrl.moveToTime = '2018-03-25 09:00';
				var applyButton = angular.element(element[0].querySelector(".move-shift .form-submit"));
				applyButton[0].click();
				scope.$apply();


				var lastRequestedData = activityService.lastRequestedData();
				expect(lastRequestedData.NewShiftStart).toBe('2018-03-25T07:00');
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
			vm.scheduleManagementSvc = scheduleManagement;
			vm.setReady(true);
			vm.setActiveCmd('MoveShift');
			scope.$apply();

			var element = angular.element(container[0].querySelector(".move-shift"));
			return {
				element: element,
				scope: scope
			};
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