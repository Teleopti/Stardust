"use strict";

describe("teamschedule person selection tests", function() {
	var target;

	beforeEach(function() {
		module("wfm.teamSchedule");
	});

	beforeEach(inject(function(PersonSelection) {
		target = PersonSelection;
	}));

	var scheduleDate = moment("2016-03-20");
	var personId1 = "221B-Sherlock";
	var personId2 = "221B-SomeoneElse";

	var schedule1 = {
		PersonId: personId1,
		Date: scheduleDate,
		Shifts: [],
		AllowSwap: function () { return false; },
		ScheduleStartTime: function () {
			return "2016-03-20 08:00";
		},
		ScheduleEndTime: function () {
			return "2016-03-20 18:00";
		}
	};

	var newSchedule1 = {
		PersonId: personId1,
		Date: scheduleDate,
		Shifts: [
			{
				Date: scheduleDate,
				AbsenceCount: function () {
					return 1;
				},
				ActivityCount: function() {
					return 1;
				}
			}
		],
		AllowSwap: function () { return false; },
		ScheduleStartTime: function() {
			return "2016-03-20 08:00";
		},
		ScheduleEndTime: function () {
			return "2016-03-20 18:00";
		}
	};

	var schedule2 = {
		PersonId: personId2,
		Date: scheduleDate,
		Shifts: [],
		AllowSwap: function () { return true; },
		ScheduleStartTime: function () {
			return "2016-03-20 08:00";
		},
		ScheduleEndTime: function () {
			return "2016-03-20 18:00";
		}
	};

	it("Can update correct person info", inject(function () {
		
		target.updatePersonInfo([schedule1, schedule2]);
		expect(target.isAnyAgentSelected()).toEqual(false);

		target.personInfo[personId1] = {
			Checked: true,
			SelectedAbsences:[],
			SelectedActivities:[],
			PersonAbsenceCount:0,
			PersonActivityCount:0,
			AllowSwap:true
			}
		target.updatePersonInfo([schedule1, schedule2]);
		var person1 = target.personInfo[personId1];
		expect(person1.Checked).toEqual(true);
		expect(person1.AllowSwap).toEqual(schedule1.AllowSwap());
		expect(person1.PersonAbsenceCount).toEqual(0);
		expect(person1.PersonActivityCount).toEqual(0);

		target.updatePersonInfo([newSchedule1]);

		person1 = target.personInfo[personId1];
		expect(person1.Checked).toEqual(true);
		expect(person1.AllowSwap).toEqual(newSchedule1.AllowSwap());
		expect(person1.PersonAbsenceCount).toEqual(0);
	}));

	it("Should clear person info", inject(function () {
		target.updatePersonInfo([schedule1]);
		target.clearPersonInfo();
		expect(target.personInfo).toEqual({});
	}));

	it("can select/deselect one person", inject(function () {
		schedule1.IsSelected = true;
		target.updatePersonSelection(schedule1);

		var person1 = target.personInfo[personId1];
		expect(person1.Checked).toEqual(true);
		expect(target.getSelectedPersonInfoList().length).toEqual(1);
		
		schedule1.IsSelected = false;
		target.updatePersonSelection(schedule1);

		expect(target.getSelectedPersonInfoList().length).toEqual(0);
	}));
	
	it("can select several people", inject(function () {
		schedule1.IsSelected = true;
		schedule2.IsSelected = true;
		target.selectAllPerson([schedule1, schedule2]);

		expect(target.personInfo[personId1].Checked).toEqual(true);
		expect(target.personInfo[personId2].Checked).toEqual(true);
		expect(target.getSelectedPersonInfoList().length).toEqual(2);
	}));

	it("Should get selected person ids", inject(function () {
		schedule1.IsSelected = true;
		schedule2.IsSelected = true;
		target.selectAllPerson([schedule1, schedule2]);
		expect(target.isAnyAgentSelected()).toEqual(true);

		var selectedPersonId = target.getSelectedPersonIdList();
		expect(selectedPersonId.length).toEqual(2);
		expect(selectedPersonId[0]).toEqual(personId1);
		expect(selectedPersonId[1]).toEqual(personId2);
	}));

	it("Should get selected person info", inject(function () {
		newSchedule1.IsSelected = true;
		target.updatePersonSelection(newSchedule1);
		expect(target.isAnyAgentSelected()).toEqual(true);

		var selectedPerson = target.getSelectedPersonInfoList();
		expect(selectedPerson.length).toEqual(1);

		var person = selectedPerson[0];
		expect(person.PersonId).toEqual(personId1);
		expect(person.AllowSwap).toEqual(newSchedule1.AllowSwap());
		expect(person.PersonAbsenceCount).toEqual(0);
	}));
});
