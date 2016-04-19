'use strict';

describe('[TeamSchedule ScheduleTable ControllerTest]', function () {
	var controller;
	var scope, personSelection;

	beforeEach(function () {
		module('wfm.teamSchedule');
		module(function ($provide) {
			$provide.service('Toggle', function () {
				return {
					WfmTeamSchedule_RemoveAbsence_36705: true
				};
			});
		});
	});

	beforeEach(inject(function ($controller, $rootScope, PersonSelection) {
		scope = $rootScope.$new();
		personSelection = PersonSelection;
		controller = setUpController($controller);
	}));

	it("can select and deselect one person", inject(function () {
		var schedules = [
			createSchedule('1111', '2015-01-01', null, [{ startHour: 8, endHour: 16 }]),
			createSchedule('2222', '2015-01-01', null, [{ startHour: 8, endHour: 16 }]),
			createSchedule('3333', '2015-01-01', null, [{ startHour: 8, endHour: 16 }])
		];

		controller.scheduleVm = { Schedules: schedules };
		schedules[2].IsSelected = true;
		controller.updatePersonSelection(schedules[2]);
		var checkedPersonIds = personSelection.getCheckedPersonIds();
		expect(checkedPersonIds.length).toEqual(1);
		expect(checkedPersonIds[0]).toEqual('3333');

		schedules[2].IsSelected = false;
		controller.updatePersonSelection(schedules[2]);
		checkedPersonIds = personSelection.getCheckedPersonIds();
		expect(checkedPersonIds.length).toEqual(0);
	}));

	it('can select and deselect current page', inject(function () {
		var schedules = [
			createSchedule('1111', '2015-01-01', null, [{ startHour: 8, endHour: 16 }]),
			createSchedule('2222', '2015-01-01', null, [{ startHour: 8, endHour: 16 }]),
			createSchedule('3333', '2015-01-01', null, [{ startHour: 8, endHour: 16 }])
		];

		controller.scheduleVm = { Schedules: schedules };

		controller.updateAllSelectionInCurrentPage(true);
		scope.$apply();
		var selectedPersonIds = personSelection.getCheckedPersonIds();

		expect(selectedPersonIds.length).toEqual(3);
		expect(selectedPersonIds[0]).toEqual("1111");
		expect(selectedPersonIds[1]).toEqual("2222");
		expect(selectedPersonIds[2]).toEqual("3333");

		controller.updateAllSelectionInCurrentPage(false);
		scope.$apply();
		selectedPersonIds = personSelection.getCheckedPersonIds();
		expect(selectedPersonIds.length).toEqual(0);
	}));

	it('can select and deselect person absence', inject(function () {
		var personAbsence1 = {
			ParentPersonAbsence: "PersonAbsenceId-111",
			ShiftLayerId: null,
			Start: "2016-02-19 08:00",
			Minutes: 640,
			Selected: false,
			ToggleSelection: function () {
				this.Selected = !this.Selected;
			}
		};
		var personAbsence2 = {
			ParentPersonAbsence: "PersonAbsenceId-222",
			ShiftLayerId: null,
			Start: "2016-02-19 15:00",
			Minutes: 60,
			Selected: false,
			ToggleSelection: function () {
				this.Selected = !this.Selected;
			}
		};
		var personAbsence3 = {
			ParentPersonAbsence: "PersonAbsenceId-111",
			ShiftLayerId: null,
			Start: "2016-02-19 15:30",
			Minutes: 60,
			Selected: false,
			ToggleSelection: function () {
				this.Selected = !this.Selected;
			}
		}
		var allProjections = [personAbsence1, personAbsence2, personAbsence3];
		var schedule = {
			"PersonId": "1234",
			"Date": moment("2016-02-19"),
			"Shifts": [
				{
					"Date": moment("2016-02-19"),
					"Projections": allProjections
				}
			],
			ScheduleEndTime: function () {
				return moment("2016-02-19 16:30")
			},
			AbsenceCount: function () {
				return 3;
			},
			AllowSwap: function(){
				return true;
			}
		};

		controller.scheduleVm = { Schedules: [schedule] };

		controller.ToggleProjectionSelection(personAbsence2, schedule, schedule.Date);
		var selectedPersonInfoList = personSelection.getSelectedPersonInfoList();
		expect(selectedPersonInfoList.length).toEqual(1);
		expect(selectedPersonInfoList[0].personId).toEqual(schedule.PersonId);
		expect(selectedPersonInfoList[0].selectedAbsences.length).toEqual(1);
		expect(selectedPersonInfoList[0].selectedActivities.length).toEqual(0);
		expect(personAbsence1.Selected).toEqual(false);
		expect(personAbsence2.Selected).toEqual(true);
		expect(personAbsence3.Selected).toEqual(false);

		controller.ToggleProjectionSelection(personAbsence1, schedule, schedule.Date);
		selectedPersonInfoList = personSelection.getSelectedPersonInfoList();
		expect(selectedPersonInfoList[0].selectedAbsences.length).toEqual(2);
		expect(selectedPersonInfoList[0].selectedActivities.length).toEqual(0);
		expect(personAbsence1.Selected).toEqual(true);
		expect(personAbsence2.Selected).toEqual(true);
		expect(personAbsence3.Selected).toEqual(true);

		controller.ToggleProjectionSelection(personAbsence3, schedule, schedule.Date);
		selectedPersonInfoList = personSelection.getSelectedPersonInfoList();
		expect(selectedPersonInfoList[0].selectedAbsences.length).toEqual(1);
		expect(selectedPersonInfoList[0].selectedActivities.length).toEqual(0);
		expect(personAbsence1.Selected).toEqual(false);
		expect(personAbsence2.Selected).toEqual(true);
		expect(personAbsence3.Selected).toEqual(false);

		controller.ToggleProjectionSelection(personAbsence2, schedule, schedule.Date);
		selectedPersonInfoList = personSelection.getSelectedPersonInfoList();
		expect(selectedPersonInfoList.length).toEqual(0);
	}));

	it('can select and deselect person activities', inject(function () {
		var personActivity1 = {
			ShiftLayerId: '111',
			ParentPersonAbsence: null,
			Start: "2016-02-19 08:00",
			Minutes: 640,
			Selected: false,
			ToggleSelection: function () {
				this.Selected = !this.Selected;
			}
		};
		var personActivity2 = {
			ShiftLayerId: '222',
			ParentPersonAbsence: null,
			Start: "2016-02-19 15:00",
			Minutes: 60,
			Selected: false,
			ToggleSelection: function () {
				this.Selected = !this.Selected;
			}
		};
		var personActivity3 = {
			ShiftLayerId: '333',
			ParentPersonAbsence: null,
			Start: "2016-02-19 15:30",
			Minutes: 60,
			Selected: false,
			ToggleSelection: function () {
				this.Selected = !this.Selected;
			}
		};
		var allProjections = [personActivity1, personActivity2, personActivity3];
		var schedule = {
			"PersonId": "1234",
			"Date": moment("2016-02-19"),
			"Shifts": [
				{
					"Date": moment("2016-02-19"),
					"Projections": allProjections
				}
			],
			ScheduleEndTime: function () {
				return moment("2016-02-19 16:30")
			},
			ActivityCount: function () {
				return 3;
			},
			AbsenceCount: function () {
				return 0;
			}, 
			AllowSwap: function(){
				return true;
			}
		};

		controller.scheduleVm = { Schedules: [schedule] };

		controller.ToggleProjectionSelection(personActivity1, schedule, schedule.Date);
		var selectedPersonInfoList = personSelection.getSelectedPersonInfoList();
		expect(selectedPersonInfoList.length).toEqual(1);
		expect(selectedPersonInfoList[0].personId).toEqual(schedule.PersonId);
		expect(selectedPersonInfoList[0].selectedActivities.length).toEqual(1);
		expect(selectedPersonInfoList[0].selectedAbsences.length).toEqual(0);
		expect(personActivity1.Selected).toEqual(true);
		expect(personActivity2.Selected).toEqual(false);
		expect(personActivity3.Selected).toEqual(false);

		controller.ToggleProjectionSelection(personActivity2, schedule, schedule.Date);
		selectedPersonInfoList = personSelection.getSelectedPersonInfoList();
		expect(selectedPersonInfoList[0].selectedActivities.length).toEqual(2);
		expect(selectedPersonInfoList[0].selectedAbsences.length).toEqual(0);
		expect(personActivity1.Selected).toEqual(true);
		expect(personActivity2.Selected).toEqual(true);
		expect(personActivity3.Selected).toEqual(false);

		controller.ToggleProjectionSelection(personActivity3, schedule, schedule.Date);
		selectedPersonInfoList = personSelection.getSelectedPersonInfoList();
		expect(selectedPersonInfoList[0].selectedActivities.length).toEqual(3);
		expect(selectedPersonInfoList[0].selectedAbsences.length).toEqual(0);
		expect(personActivity1.Selected).toEqual(true);
		expect(personActivity2.Selected).toEqual(true);
		expect(personActivity3.Selected).toEqual(true);

		controller.ToggleProjectionSelection(personActivity1, schedule, schedule.Date);
		selectedPersonInfoList = personSelection.getSelectedPersonInfoList();
		expect(selectedPersonInfoList[0].selectedActivities.length).toEqual(2);
		expect(selectedPersonInfoList[0].selectedAbsences.length).toEqual(0);
		expect(personActivity1.Selected).toEqual(false);
		expect(personActivity2.Selected).toEqual(true);
		expect(personActivity3.Selected).toEqual(true);

	}));

	it('can select and deselect same activities in different layers', inject(function () {
		var personActivity1 = {
			ShiftLayerId: '111',
			ParentPersonAbsence: null,
			Start: "2016-02-19 08:00",
			Minutes: 640,
			Selected: false,
			ToggleSelection: function () {
				this.Selected = !this.Selected;
			}
		};
		var personActivity2 = {
			ShiftLayerId: '222',
			ParentPersonAbsence: null,
			Start: "2016-02-19 15:00",
			Minutes: 60,
			Selected: false,
			ToggleSelection: function () {
				this.Selected = !this.Selected;
			}
		};
		var personActivity3 = {
			ShiftLayerId: '111',
			ParentPersonAbsence: null,
			Start: "2016-02-19 15:30",
			Minutes: 60,
			Selected: false,
			ToggleSelection: function () {
				this.Selected = !this.Selected;
			}
		};
		var allProjections = [personActivity1, personActivity2, personActivity3];
		var schedule = {
			"PersonId": "1234",
			"Date": moment("2016-02-19"),
			"Shifts": [
				{
					"Date": moment("2016-02-19"),
					"Projections": allProjections
				}
			],
			ScheduleEndTime: function () {
				return moment("2016-02-19 16:30")
			},
			ActivityCount: function () {
				return 2;
			},
			AbsenceCount: function () {
				return 0;
			},
			AllowSwap: function(){
				return true;
			}
		};

		controller.scheduleVm = { Schedules: [schedule] };

		controller.ToggleProjectionSelection(personActivity1, schedule, schedule.Date);
		var selectedPersonInfoList = personSelection.getSelectedPersonInfoList();
		expect(selectedPersonInfoList.length).toEqual(1);
		expect(selectedPersonInfoList[0].personId).toEqual(schedule.PersonId);
		expect(selectedPersonInfoList[0].selectedActivities.length).toEqual(1);
		expect(personActivity1.Selected).toEqual(true);
		expect(personActivity2.Selected).toEqual(false);
		expect(personActivity3.Selected).toEqual(true);

		controller.ToggleProjectionSelection(personActivity2, schedule, schedule.Date);
		selectedPersonInfoList = personSelection.getSelectedPersonInfoList();
		expect(selectedPersonInfoList[0].selectedActivities.length).toEqual(2);
		expect(personActivity1.Selected).toEqual(true);
		expect(personActivity2.Selected).toEqual(true);
		expect(personActivity3.Selected).toEqual(true);

		controller.ToggleProjectionSelection(personActivity3, schedule, schedule.Date);
		selectedPersonInfoList = personSelection.getSelectedPersonInfoList();
		expect(selectedPersonInfoList[0].selectedActivities.length).toEqual(1);
		expect(personActivity1.Selected).toEqual(false);
		expect(personActivity2.Selected).toEqual(true);
		expect(personActivity3.Selected).toEqual(false);

		controller.ToggleProjectionSelection(personActivity2, schedule, schedule.Date);
		selectedPersonInfoList = personSelection.getSelectedPersonInfoList();
		expect(selectedPersonInfoList.length).toEqual(0);
		expect(personActivity1.Selected).toEqual(false);
		expect(personActivity2.Selected).toEqual(false);
		expect(personActivity3.Selected).toEqual(false);

	}));


	it("can select all people in current page", inject(function () {
		var schedules = [
			createSchedule('1111', '2015-01-01', null, [{ startHour: 8, endHour: 16 }]),
			createSchedule('2222', '2015-01-01', null, [{ startHour: 8, endHour: 16 }])
		];

		controller.scheduleVm = { Schedules: schedules };
		controller.updateAllSelectionInCurrentPage(true);
		scope.$apply();

		var selectedPersonInfoList = personSelection.getSelectedPersonInfoList();
		expect(controller.scheduleVm.Schedules[0].IsSelected).toEqual(true);
		expect(controller.scheduleVm.Schedules[1].IsSelected).toEqual(true);
		expect(selectedPersonInfoList.length).toEqual(2);
	}));

	it("can initialize current page selection status correctly when all people in current page are selected", inject(function () {

		var schedules = [
			createSchedule('1111', '2015-01-01', null, [{ startHour: 8, endHour: 16 }]),
			createSchedule('2222', '2015-01-01', null, [{ startHour: 8, endHour: 16 }])
		];
		schedules[0].IsSelected = true;
		schedules[1].IsSelected = true;
		personSelection.personInfo['1111'] = { checked: true };
		personSelection.personInfo['2222'] = { checked: true };
		controller.scheduleVm = { Schedules: schedules };
		scope.$apply();

		expect(controller.toggleAllInCurrentPage).toEqual(true);
	}));

	it("cannot select overtime layer", function () {
		var personActivity1 = {
			ShiftLayerId: '111',
			ParentPersonAbsence: null,
			Start: "2016-02-19 08:00",
			Selected: false,
			ToggleSelection: function () {
				this.Selected = !this.Selected;
			}
		};
		var overtimeActivity = {
			ShiftLayerId: '222',
			ParentPersonAbsence: null,
			IsOvertime: true,
			Start: "2016-02-19 15:00",
			Selected: false,
			ToggleSelection: function () {
				this.Selected = !this.Selected;
			}
		};
		var allProjections = [personActivity1, overtimeActivity];
		var schedule = {
			"PersonId": "1234",
			"Date": moment("2016-02-19"),
			"Shifts": [
				{
					"Date": moment("2016-02-19"),
					"Projections": allProjections
				}
			],
			ScheduleEndTime: function () {
				return moment("2016-02-19 16:30")
			},
			ActivityCount: function () {
				return 3;
			},
			AbsenceCount: function () {
				return 0;
			},
			AllowSwap: function(){
				return true;
			}
		};

		controller.scheduleVm = { Schedules: [schedule] };
		controller.selectedPersonProjections = [];

		controller.ToggleProjectionSelection(personActivity1, schedule, schedule.Date);
		var selectedPersonInfoList = personSelection.getSelectedPersonInfoList();
		expect(selectedPersonInfoList.length).toEqual(1);
		expect(selectedPersonInfoList[0].personId).toEqual(schedule.PersonId);
		expect(selectedPersonInfoList[0].selectedActivities.length).toEqual(1);
		expect(personActivity1.Selected).toEqual(true);
		expect(overtimeActivity.Selected).toEqual(false);

		controller.ToggleProjectionSelection(overtimeActivity, schedule, schedule.Date);
		selectedPersonInfoList = personSelection.getSelectedPersonInfoList();
		expect(selectedPersonInfoList.length).toEqual(1);
		expect(selectedPersonInfoList[0].personId).toEqual(schedule.PersonId);
		expect(selectedPersonInfoList[0].selectedActivities.length).toEqual(1);
		expect(personActivity1.Selected).toEqual(true);
		expect(overtimeActivity.Selected).toEqual(false);
	});

	function setUpController($controller) {
		return $controller('scheduleTableCtrl', { $scope: scope, personSelectionSvc: personSelection });
	}

	function createSchedule(personId, belongsToDate, dayOff, projectionInfoArray) {

		var dateMoment = moment(belongsToDate);
		var projections = [];

		var fakeSchedule = {
			PersonId: personId,
			Date: dateMoment,
			DayOff: dayOff == null ? null : createDayOff(),
			Shifts: [{
				Date: dateMoment,
				Projections: createProjection(),
				AbsenceCount: 0,
				ActivityCount: 0
			}],
			ScheduleEndTime: function () { return dateMoment.endOf('day') },
			AllowSwap: function () { return false; }
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