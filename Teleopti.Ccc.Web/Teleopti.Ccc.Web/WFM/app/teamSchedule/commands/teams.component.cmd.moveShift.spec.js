describe('move shift component test',
	function() {
		'use strict';
		var $rootScope,
			$compile,
			personSelectionService,
			scheduleManagementSvc;

		personSelectionService = new FakePersonSelectionService();
		scheduleManagementSvc = new FakeScheduleManagementService();

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
			$provide.service('teamsToggles',
				function() {
					return { all: function() {} };
				});
			$provide.service('Toggle',
				function() {
					return {};
				});
		}));

		beforeEach(inject(function(_$rootScope_,_$compile_) {
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
					Name: 'agent1',
					ScheduleStartTime: '2016-06-15T08:00:00Z',
					ScheduleEndTime: '2016-06-15T17:00:00Z',
					SelectedActivities: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}];
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
					}]
			});
			var element = setUp(moment('2017-07-05').toDate(), currentTimezone);
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
					ScheduleStartTime: '2016-06-15T08:00:00Z',
					ScheduleEndTime: '2016-06-15T17:00:00Z',
					SelectedActivities: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}, {
					PersonId: 'agent2',
					Name: 'agent2',
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

			var element = setUp(moment('2016-06-15').toDate(), currentTimeZone);
			var applyButton = angular.element(element[0].querySelector(".move-shift .form-submit"));
			var errorMessage = element[0].querySelector(".move-shift .has-agent-in-different-timezone");
			expect(errorMessage).toBeTruthy();
			expect(applyButton.attr('disabled')).toBe(undefined);
		});

		function setUp(inputDate,timeZone) {
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
			return element;
		}

		function FakePersonSelectionService() {
			var fakePersonList = [];

			this.setFakeSelectedPersonInfoList = function (input) {
				fakePersonList = input;
			}

			this.getSelectedPersonInfoList = function () {
				return fakePersonList;
			}

			this.getSelectedPersonIdList = function() {
				return fakePersonList.map(function(p) { return p.PersonId; });
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
});