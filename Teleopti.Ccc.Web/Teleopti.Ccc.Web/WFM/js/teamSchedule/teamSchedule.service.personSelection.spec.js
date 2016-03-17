"use strict";

describe("PersonSelection", function() {
	var target;

	beforeEach(function() {
		module("wfm.teamSchedule");
	});

	beforeEach(inject(function(PersonSelection) {
		target = PersonSelection;
	}));

	var scheduleDate = "2016-03-20";
	var personId1 = "221B-Sherlock";
	var personId2 = "221B-SomeoneElse";

	var schedule1 = {
		PersonId: personId1,
		Shifts: [],
		AllowSwap: function() { return false }
	};

	var newSchedule1 = {
		PersonId: personId1,
		Shifts: [
			{
				Date: scheduleDate,
				AbsenceCount: function () {
					return 1;
				}
			}
		],
		AllowSwap: function () { return false }
	};

	var schedule2 = {
		PersonId: personId2,
		Shifts: [],
		AllowSwap: function () { return true }
	};

	it("Can update correct person info", inject(function () {
		target.updatePersonInfo([schedule1, schedule2]);
		expect(target.isAnyAgentSelected()).toEqual(false);

		var person1 = target.personInfo[personId1];
		expect(person1.isSelected).toEqual(false);
		expect(person1.allowSwap).toEqual(schedule1.AllowSwap());
		expect(person1.personAbsenceCount).toEqual(0);

		var person2 = target.personInfo[personId2];
		expect(person2.isSelected).toEqual(false);
		expect(person2.allowSwap).toEqual(schedule2.AllowSwap());
		expect(person2.personAbsenceCount).toEqual(0);


		target.setScheduleDate(scheduleDate);
		target.updatePersonInfo([newSchedule1]);

		person1 = target.personInfo[personId1];
		expect(person1.isSelected).toEqual(false);
		expect(person1.allowSwap).toEqual(newSchedule1.AllowSwap());
		expect(person1.personAbsenceCount).toEqual(1);
	}));

	it("Should clear person info", inject(function () {
		target.updatePersonInfo([schedule1]);
		target.clearPersonInfo();
		expect(target.personInfo).toEqual({});
	}));

	it("Should reset person info", inject(function () {
		target.updatePersonInfo([schedule1]);
		target.resetPersonInfo([schedule2]);

		var person1 = target.personInfo[personId1];
		expect(person1).toEqual(undefined);

		var person2 = target.personInfo[personId2];
		expect(person2.isSelected).toEqual(false);
		expect(person2.allowSwap).toEqual(schedule2.AllowSwap());
		expect(person2.personAbsenceCount).toEqual(0);
	}));

	it("Should select person", inject(function () {
		target.updatePersonInfo([schedule1]);
		target.selectAllPerson([schedule2]);

		var person1 = target.personInfo[personId1];
		expect(person1.isSelected).toEqual(true);

		var person2 = target.personInfo[personId2];
		expect(person2.isSelected).toEqual(true);
	}));

	it("Should get selected person ids", inject(function () {
		target.updatePersonInfo([schedule1, schedule2]);
		target.personInfo[schedule1.PersonId].isSelected = true;
		expect(target.isAnyAgentSelected()).toEqual(true);

		var selectedPersonId = target.getSelectedPersonIdList();
		expect(selectedPersonId.length).toEqual(1);
		expect(selectedPersonId[0]).toEqual(personId1);
	}));

	it("Should get selected person info", inject(function () {
		target.setScheduleDate(scheduleDate);
		target.updatePersonInfo([newSchedule1, schedule2]);
		target.personInfo[personId1].isSelected = true;
		expect(target.isAnyAgentSelected()).toEqual(true);

		var selectedPerson = target.getSelectedPersonInfoList();
		expect(selectedPerson.length).toEqual(1);

		var person = selectedPerson[0];
		expect(person.personId).toEqual(personId1);
		expect(person.allowSwap).toEqual(newSchedule1.AllowSwap());
		expect(person.personAbsenceCount).toEqual(1);
	}));
});
