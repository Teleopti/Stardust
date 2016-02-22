'use strict';

describe('TeamSchedule ScheduleTable ControllerTest', function() {
	var controller;

	beforeEach(function() {
		module('wfm.teamSchedule');
		module(function($provide) {
			$provide.service('Toggle', function() {
				return {
					WfmTeamSchedule_RemovePlannedAbsence_36705: true
				};
			});
		});
	});

	beforeEach(inject(function ($controller) {
		controller = setUpController($controller);
	}));

	it("can select and deselect one person", inject(function () {
		var schedules = [
			createSchedule('1111', '2015-01-01', null, [{ startHour: 8, endHour: 16 }]),
			createSchedule('2222', '2015-01-01', null, [{ startHour: 8, endHour: 16 }]),
			createSchedule('3333', '2015-01-01', null, [{ startHour: 8, endHour: 16 }])
		];

		controller.scheduleVm = { Schedules: schedules };
		setupPersonIdSelectionDic(schedules, controller);

		controller.updatePersonIdSelection(schedules[2]);
		expect(controller.isPersonSelected(schedules[0])).toEqual(false);
		expect(controller.isPersonSelected(schedules[1])).toEqual(false);
		expect(controller.isPersonSelected(schedules[2])).toEqual(true);

		controller.updatePersonIdSelection(schedules[2]);
		expect(controller.isPersonSelected(schedules[0])).toEqual(false);
		expect(controller.isPersonSelected(schedules[1])).toEqual(false);
		expect(controller.isPersonSelected(schedules[2])).toEqual(false);
	}));

	it('can select and deselect current page', inject(function() {
		var schedules = [
			createSchedule('1111', '2015-01-01', null, [{ startHour: 8, endHour: 16 }]),
			createSchedule('2222', '2015-01-01', null, [{ startHour: 8, endHour: 16 }]),
			createSchedule('3333', '2015-01-01', null, [{ startHour: 8, endHour: 16 }])
		];

		controller.scheduleVm = { Schedules: schedules };
		setupPersonIdSelectionDic(schedules, controller);

		controller.toggleAllSelectionInCurrentPage();
		var selectedPersonList = getSelectedPersonIdList(controller);

		expect(selectedPersonList.length).toEqual(3);
		expect(selectedPersonList[0]).toEqual("1111");
		expect(selectedPersonList[1]).toEqual("2222");
		expect(selectedPersonList[2]).toEqual("3333");

		controller.toggleAllSelectionInCurrentPage();
		selectedPersonList = getSelectedPersonIdList(controller);
		expect(selectedPersonList.length).toEqual(0);
	}));

	it('can select and deselect person absence', inject(function () {
		var personAbsence1 = {
			ParentPersonAbsence: "PersonAbsenceId-111",
			Start: "2016-02-19 08:00",
			Selected: false,
			ToggleSelection: function() {
				this.Selected = !this.Selected;
			}
		};
		var personAbsence2 = {
			ParentPersonAbsence: "PersonAbsenceId-222",
			Start: "2016-02-19 15:00",
			Selected: false,
			ToggleSelection: function () {
				this.Selected = !this.Selected;
			}
		};
		var personAbsence3 = {
			ParentPersonAbsence: "PersonAbsenceId-111",
			Start: "2016-02-19 15:30",
			Selected: false,
			ToggleSelection: function () {
				this.Selected = !this.Selected;
			}
		}
		var allProjections = [personAbsence1, personAbsence2, personAbsence3];
		var schedules = [
			{
				"PersonId": "1234",
				"Date": "2016-02-19",
				"Shifts": [
					{
						"Projections": allProjections
					}
				]
			}
		];

		controller.scheduleVm = { Schedules: schedules };
		controller.selectedPersonAbsences = [];

		controller.ToggleProjectionSelection(personAbsence2, allProjections);
		expect(personAbsence1.Selected).toEqual(false);
		expect(personAbsence2.Selected).toEqual(true);
		expect(personAbsence3.Selected).toEqual(false);

		controller.ToggleProjectionSelection(personAbsence1, allProjections);
		expect(personAbsence1.Selected).toEqual(true);
		expect(personAbsence2.Selected).toEqual(true);
		expect(personAbsence3.Selected).toEqual(true);

		controller.ToggleProjectionSelection(personAbsence3, allProjections);
		expect(personAbsence1.Selected).toEqual(false);
		expect(personAbsence2.Selected).toEqual(true);
		expect(personAbsence3.Selected).toEqual(false);
	}));

	it("can display person selection status correctly", inject(function () {
		var schedules = [
			createSchedule('1111', '2015-01-01', null, [{ startHour: 8, endHour: 16 }]),
			createSchedule('2222', '2015-01-01', null, [{ startHour: 8, endHour: 16 }])
		];

		controller.scheduleVm = { Schedules: schedules };
		setupPersonIdSelectionDic(schedules, controller);
		controller.personSelection['1111'].isSelected = true;

		expect(controller.isPersonSelected(schedules[0])).toEqual(true);
		expect(controller.isPersonSelected(schedules[1])).toEqual(false);
		expect(controller.isAllInCurrentPageSelected()).toEqual(false);
	}));

	it("can display current page selection status correctly when all people in current page are selected", inject(function() {
		
		var schedules = [
			createSchedule('1111', '2015-01-01', null, [{ startHour: 8, endHour: 16 }]),
			createSchedule('2222', '2015-01-01', null, [{ startHour: 8, endHour: 16 }])
		];

		controller.scheduleVm = { Schedules: schedules };
		setupPersonIdSelectionDic(schedules, controller);
		controller.toggleAllSelectionInCurrentPage();

		expect(controller.isPersonSelected(schedules[0])).toEqual(true);
		expect(controller.isPersonSelected(schedules[0])).toEqual(true);
		expect(controller.isAllInCurrentPageSelected()).toEqual(true);
	}));

	function setUpController($controller) {
		return $controller('scheduleTableCtrl');
	}

	function createSchedule(personId, belongsToDate, dayOff, projectionInfoArray) {

		var dateMoment = moment(belongsToDate);
		var projections = [];

		var fakeSchedule = {
			PersonId: personId,
			Date: dateMoment.format('YYYY-MM-DD'),
			DayOff: dayOff == null ? null : createDayOff(),
			Projection: createProjection()
		};

		function createProjection() {

			if (dayOff == null) {
				projectionInfoArray.forEach(function (projectionInfo) {
					var dateMomentCopy = moment(dateMoment);

					projections.push({
						Start: dateMomentCopy.add(projectionInfo.startHour, 'hours').format('YYYY-MM-DD HH:mm'),
						Minutes: moment.duration(projectionInfo.endHour - projectionInfo.startHour, 'hours').asMinutes()
					});
				});
			}

			return projections;
		};

		function createDayOff() {
			return {
				DayOffName: 'Day off',
				Start: dateMoment.format('YYYY-MM-DD HH:mm'),
				Minutes: 1440
			};

		};

		return fakeSchedule;
	}

	function setupPersonIdSelectionDic(schedules, controller) {
		if (controller.personSelection == undefined)
			controller.personSelection = {};

		schedules.forEach(function (personSchedule) {
			if (controller.personSelection[personSchedule.PersonId] === undefined) {
				controller.personSelection[personSchedule.PersonId] = { isSelected: false };
			}
		});
	}

	function getSelectedPersonIdList(controller) {
		var result = [];
		for (var key in controller.personSelection) {
			if (controller.personSelection[key].isSelected) {
				result.push(key);
			}
		}
		return result;
	}
});