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
		"InternalNotes": "test",
		"PublicNotes": "public"
	};
	var schedule2 = {
		"PersonId": "221B-Sherlock",
		"Name": "Sherlock Holmes",
		"Date": scheduleDate,
		"InternalNotes": null,
		"PublicNotes": null
	};

	var schedule3 = {
		"PersonId": "221B-Baker-SomeoneElse",
		"Name": "SomeoneElse",
		"Date": yesterday,
		"InternalNotes": null,
		"PublicNotes": null
	};

	it("Can create note dictionary for selected date", inject(function () {
		target.resetScheduleNotes([schedule1, schedule3, schedule2], scheduleDateMoment);
		var note1 = target.getNoteForPerson("221B-Baker-SomeoneElse");
		var note2 = target.getNoteForPerson("221B-Sherlock");
		expect(note1.internalNotes).toEqual("test");
		expect(note1.publicNotes).toEqual("public");
		expect(note2.publicNotes).toEqual(null);
		expect(note2.internalNotes).toEqual(null);
	}));
});