"use strict";

describe("PersonSelection", function() {
	var target;

	beforeEach(function() {
		module("wfm.teamSchedule");
	});

	beforeEach(inject(function(PersonSelection) {
		target = PersonSelection;
	}));

	var schedule1 = {
		PersonId: "221B-Sherlock",
		AllowSwap: function() { return false }
	};

	var schedule2 = {
		PersonId: "221B-SomeoneElse",
		AllowSwap: function () { return true }
	};

	it("Can update correct person info", inject(function () {
		target.updatePersonInfo([schedule1, schedule2]);
		expect(target.isAnyAgentSelected()).toEqual(false);

		var person1 = target.personInfo[schedule1.PersonId];
		expect(person1.isSelected).toEqual(false);
		expect(person1.allowSwap).toEqual(schedule1.AllowSwap());

		var person2 = target.personInfo[schedule2.PersonId];
		expect(person2.isSelected).toEqual(false);
		expect(person2.allowSwap).toEqual(schedule2.AllowSwap());

		var newSchedule = {
			PersonId: schedule1.PersonId,
			AllowSwap: function () { return true }
		};

		target.updatePersonInfo([newSchedule]);

		person1 = target.personInfo[schedule1.PersonId];
		expect(person1.isSelected).toEqual(false);
		expect(person1.allowSwap).toEqual(newSchedule.AllowSwap());
	}));

	it("Should clear person info", inject(function () {
		target.updatePersonInfo([schedule1]);
		target.clearPersonInfo();
		expect(target.personInfo).toEqual({});
	}));

	it("Should reset person info", inject(function () {
		target.updatePersonInfo([schedule1]);
		target.resetPersonInfo([schedule2]);

		var person1 = target.personInfo[schedule1.PersonId];
		expect(person1).toEqual(undefined);

		var person2 = target.personInfo[schedule2.PersonId];
		expect(person2.isSelected).toEqual(false);
		expect(person2.allowSwap).toEqual(schedule2.AllowSwap());
	}));

	it("Should select person", inject(function () {
		target.updatePersonInfo([schedule1]);
		target.selectAllPerson([schedule2]);

		var person1 = target.personInfo[schedule1.PersonId];
		expect(person1.isSelected).toEqual(true);

		var person2 = target.personInfo[schedule2.PersonId];
		expect(person2.isSelected).toEqual(true);
	}));

	it("Should get selected person", inject(function () {
		target.updatePersonInfo([schedule1, schedule2]);
		target.personInfo[schedule1.PersonId].isSelected = true;
		expect(target.isAnyAgentSelected()).toEqual(true);

		var selectedPersonId = target.getSelectedPersonIdList();
		expect(selectedPersonId.length).toEqual(1);
		expect(selectedPersonId[0]).toEqual(schedule1.PersonId);
	}));
});
