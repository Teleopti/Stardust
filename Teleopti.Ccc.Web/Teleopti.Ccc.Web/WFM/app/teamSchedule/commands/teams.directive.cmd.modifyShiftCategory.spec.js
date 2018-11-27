(function() {
	'use strict';
	var $rootScope,
		$compile;

	var personSelectionService = new FakePersonSelectionService();
	var scheduleManagementSvc = new FakeScheduleManagementService();
	var shiftCategorySvc = new FakeShiftCategoryService();

	function setUp(inputDate, inputTimezone) {
		var date;
		var html = '<teamschedule-command-container date="curDate" timezone="curTimezone"></teamschedule-command-container>';
		var scope = $rootScope.$new();
		if (inputDate == null)
			date = moment('2016-06-15').toDate();
		else
			date = inputDate;
		scope.curDate = date;
		scope.curTimezone = inputTimezone;

		var container = $compile(html)(scope);
		scope.$apply();

		var vm = container.isolateScope().vm;
		vm.setReady(true);
		vm.setActiveCmd('EditShiftCategory');
		scope.$apply();

		var element = angular.element(container[0].querySelector(".edit-shift-category"));
		return {
			element: element,
			scope: scope
		};
	}

	function FakePersonSelectionService() {
		var fakePersonList = [];

		this.setFakeSelectedPersonInfoList = function(input) {
			fakePersonList = input;
		}

		this.getCheckedPersonInfoList = function() {
			return fakePersonList.filter(function(p) { return p.Checked; });
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

	function FakeShiftCategoryService() {
		var lastRequestData;

		this.lastRequestedData = function() { return lastRequestData; }

		this.fetchShiftCategories = function() {
			return{
				then: function(cb) {
					cb({
						data: [
							{
								DisplayColor: "#FFC080",
								Id: "ct1",
								Name: "Day",
								ShortName: "DY"
							},
							{
								DisplayColor:"#80FF80",
								Id:"ct2",
								Name:"Early",
								ShortName:"AM"
							}
						]
					});
				}
			}
		}
		this.modifyShiftCategories = function(requestData) {
			lastRequestData = requestData;
			return {
				then:function(cb) {
					cb({data:[]});
				}
			}
		}
	}

	describe('edit shift category component test',
		function() {
			beforeEach(module('wfm.templates'));
			beforeEach(module('wfm.teamSchedule'));

			var timezone1 = {
				IanaId: 'Etc/UTC',
				DisplayName: 'UTC'
			};

			var selectedAgents = [
				{
					PersonId: 'agent1',
					Checked: true,
					Name: 'agent1',
					Timezone:timezone1,
					ScheduleStartTime: '2016-06-15T08:00:00Z',
					ScheduleEndTime: '2016-06-15T17:00:00Z',
					SelectedActivities: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
				}
			];
			beforeEach(function() {
				personSelectionService.setFakeSelectedPersonInfoList(selectedAgents);

				scheduleManagementSvc.setPersonScheduleVm('agent1',
					{
						Date: '2016-06-15',
						PersonId: 'agent1',
						IsFullDayAbsence: true,
						Timezone: timezone1,
						DayOffs: [],
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
			});
			

			beforeEach(module(function($provide) {
				$provide.service('PersonSelection',
					function() {
						return personSelectionService;
					});
				$provide.service('ScheduleManagement',
					function () {
						return scheduleManagementSvc;
					});
				$provide.service('Toggle',
					function() {
						return {};
					});
				$provide.service('ShiftCategoryService', function() { return shiftCategorySvc; });
			}));

			beforeEach(inject(function(_$rootScope_, _$compile_) {
				$rootScope = _$rootScope_;
				$compile = _$compile_;
			}));
			afterEach(function() {
				personSelectionService.setFakeSelectedPersonInfoList([]);
			});


			it('should display the error message and disable the button for one agent with full day absence',
				function() {
					var currentTimezone = 'Etc/UTC';
					var compiledResult = setUp(moment.tz('2016-06-15', currentTimezone).toDate(), currentTimezone);
					var element = compiledResult.element;
					var scope = compiledResult.scope;
					var ctrl = element.scope().vm;
					ctrl.selectedShiftCategoryId = 'ct1';
					scope.$apply();

					var applyButton = angular.element(element[0].querySelector(".edit-shift-category .form-submit"));
					var errorMessage = element[0].querySelector(".edit-shift-category .text-danger");
					expect(errorMessage).toBeTruthy();
					expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
					expect(applyButton.attr('disabled')).toBe('disabled');
				});
		});

	describe('edit shift category component test',
		function() {
			beforeEach(module('wfm.templates'));
			beforeEach(module('wfm.teamSchedule'));

			var timezone1 = {
				IanaId: 'Etc/UTC',
				DisplayName: 'UTC'
			};

			var selectedAgents = [
				{
					PersonId: 'agent1',
					Checked: true,
					Name: 'agent1',
					Timezone: timezone1,
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
			beforeEach(function() {
				personSelectionService.setFakeSelectedPersonInfoList(selectedAgents);
				scheduleManagementSvc.setPersonScheduleVm('agent1',
					{
						Date: '2016-06-15',
						PersonId: 'agent1',
						IsFullDayAbsence: true,
						Timezone: timezone1,
						DayOffs: [
							{
								Date: '2016-06-15',
								DayOffName: 'Day Off'
							}
						],
						Shifts: [],
						ExtraShifts: []
					});
			});
			

			beforeEach(module(function($provide) {
				$provide.service('PersonSelection',
					function() {
						return personSelectionService;
					});
				$provide.service('ScheduleManagement',
					function () {
						return scheduleManagementSvc;
					});
				$provide.service('Toggle',
					function() {
						return {};
					});
				$provide.service('ShiftCategoryService', function() { return shiftCategorySvc; });
			}));

			beforeEach(inject(function(_$rootScope_, _$compile_) {
				$rootScope = _$rootScope_;
				$compile = _$compile_;
			}));
			afterEach(function () {
				personSelectionService.setFakeSelectedPersonInfoList([]);
			});

			it('should display the error message and disable the button for one agent with day off',
				function() {
					var currentTimezone = 'Etc/UTC';
					var compiledResult = setUp(moment.tz('2016-06-15', currentTimezone).toDate(), currentTimezone);
					var element = compiledResult.element;
					var scope = compiledResult.scope;
					var ctrl = element.scope().vm;
					ctrl.selectedShiftCategoryId = 'ct1';
					scope.$apply();

					var applyButton = angular.element(element[0].querySelector(".edit-shift-category .form-submit"));
					var errorMessage = element[0].querySelector(".edit-shift-category .text-danger");
					expect(errorMessage).toBeTruthy();
					expect(applyButton.attr('disabled')).toBe('disabled');
				});
		});

	describe('edit shift category component test',
		function () {
			beforeEach(module('wfm.templates'));
			beforeEach(module('wfm.teamSchedule'));

			var timezone1 = {
				IanaId: 'Etc/UTC',
				DisplayName: 'UTC'
			};

			var selectedAgents = [
				{
					PersonId: 'agent1',
					Checked: true,
					Name: 'agent1',
					Timezone: timezone1,
					ScheduleStartTime: '2016-06-15T08:00:00Z',
					ScheduleEndTime: '2016-06-15T17:00:00Z',
					SelectedActivities: ['activity1'],
					SelectedDayOffs: []
				},
				{
					PersonId: 'agent2',
					Checked: true,
					Name: 'agent2',
					Timezone: timezone1,
					ScheduleStartTime: '2016-06-15T08:00:00Z',
					ScheduleEndTime: '2016-06-15T17:00:00Z',
					SelectedActivities: ['activity1'],
					SelectedDayOffs: []
				}
			];
			beforeEach(function() {
				personSelectionService.setFakeSelectedPersonInfoList(selectedAgents);
				scheduleManagementSvc.setPersonScheduleVm('agent1',
					{
						Date: '2016-06-15',
						PersonId: 'agent1',
						IsFullDayAbsence: true,
						Timezone: timezone1,
						DayOffs: [],
						Shifts: [],
						ExtraShifts: []
					});

				scheduleManagementSvc.setPersonScheduleVm('agent2',
					{
						Date: '2016-06-15',
						PersonId: 'agent2',
						IsFullDayAbsence: false,
						Timezone: timezone1,
						DayOffs: [],
						Shifts: [],
						ExtraShifts: []
					});
			});

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
				$provide.service('ShiftCategoryService', function () { return shiftCategorySvc; });
			}));

			beforeEach(inject(function (_$rootScope_, _$compile_) {
				$rootScope = _$rootScope_;
				$compile = _$compile_;
			}));
			afterEach(function () {
				personSelectionService.setFakeSelectedPersonInfoList([]);
			});

			it('should apply command to valid agents only',
				function () {
					var currentTimezone = 'Etc/UTC';
					var compiledResult = setUp(moment.tz('2016-06-15', currentTimezone).toDate(), currentTimezone);
					var element = compiledResult.element;
					var scope = compiledResult.scope;
					var ctrl = element.scope().vm;
					ctrl.selectedShiftCategoryId = 'ct1';
					scope.$apply();

					var applyButton = angular.element(element[0].querySelector(".edit-shift-category .form-submit"));
					applyButton[0].click();
					var requestedData = shiftCategorySvc.lastRequestedData();
					expect(requestedData.PersonIds.length).toBe(1);
					expect(requestedData.PersonIds[0]).toBe('agent2');
				});
		});
})();