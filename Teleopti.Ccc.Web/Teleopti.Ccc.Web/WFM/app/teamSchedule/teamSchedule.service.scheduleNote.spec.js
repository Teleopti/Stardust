"use strict";

describe("teamschedule schedule note management service tests", function () {
	var target;

	beforeEach(function () {
		module("wfm.teamSchedule");
	});

	beforeEach(inject(function (ScheduleNoteManagementService) {
		target = ScheduleNoteManagementService;
	}));

	var scheduleDate = "2016-01-02";
	var yesterday = "2016-01-01";
	var scheduleDateMoment = moment(scheduleDate);
	var schedule1 = {
		"PersonId": "221B-Baker-SomeoneElse",
		"Name": "SomeoneElse",
		"Date": scheduleDate,
		"InternalNotes":"test"
	};
	var schedule2 = {
		"PersonId": "221B-Sherlock",
		"Name": "Sherlock Holmes",
		"Date": scheduleDate,
		"InternalNotes": null
	};

	var schedule3 = {
		"PersonId": "221B-Baker-SomeoneElse",
		"Name": "SomeoneElse",
		"Date": yesterday,
		"InternalNotes": null
	};

	it("Can create note dictionary for selected date", inject(function () {
		target.resetScheduleNotes([schedule1, schedule3, schedule2], scheduleDateMoment);
		var note1 = target.getInternalNoteForPerson("221B-Baker-SomeoneElse");
		var note2 = target.getInternalNoteForPerson("221B-Sherlock");
		expect(note1).toEqual("test");
		expect(note2).toEqual(null);
	}));

	it("Can set schedule note for person", function() {
		target.resetScheduleNotes([schedule1, schedule3, schedule2], scheduleDateMoment);
		target.setInternalNoteForPerson("221B-Baker-SomeoneElse", "newNotes");
		target.setInternalNoteForPerson("221B-Sherlock", "newNotes for sherlock");

		var note1 = target.getInternalNoteForPerson("221B-Baker-SomeoneElse");
		var note2 = target.getInternalNoteForPerson("221B-Sherlock");
		expect(note1).toEqual("newNotes");
		expect(note2).toEqual("newNotes for sherlock");
	});
});