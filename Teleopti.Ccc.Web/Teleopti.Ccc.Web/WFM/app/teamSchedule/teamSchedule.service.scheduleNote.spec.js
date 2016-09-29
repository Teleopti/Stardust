"use strict";

describe("teamschedule schedule note management service tests", function () {
	var target, $httpBackend, requestHandler;

	beforeEach(function () {
		module("wfm.teamSchedule");
	});

	beforeEach(inject(function (ScheduleNoteManagementService, _$httpBackend_) {
		target = ScheduleNoteManagementService;
		$httpBackend = _$httpBackend_;

		requestHandler = $httpBackend.when('POST', '../api/TeamScheduleCommand/EditScheduleNote')
							.respond(function() {
				return [200, []];
			});
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

	it("Can set schedule note for person", function () {
		$httpBackend.expectPOST("../api/TeamScheduleCommand/EditScheduleNote").respond(200, []);
		target.resetScheduleNotes([schedule1, schedule3, schedule2], scheduleDateMoment);
		target.setInternalNoteForPerson("221B-Baker-SomeoneElse", "newNotes", scheduleDateMoment);
		target.setInternalNoteForPerson("221B-Sherlock", "newNotes for sherlock", scheduleDateMoment);
		$httpBackend.flush();

		var note1 = target.getInternalNoteForPerson("221B-Baker-SomeoneElse");
		var note2 = target.getInternalNoteForPerson("221B-Sherlock");
		expect(note1).toEqual("newNotes");
		expect(note2).toEqual("newNotes for sherlock");
		$httpBackend.verifyNoOutstandingRequest();
	});

	it("wont set schedule note for person when server respond with error", function () {
		$httpBackend.expectPOST("../api/TeamScheduleCommand/EditScheduleNote").respond(200, [[{ PersonId: '221B-Baker-SomeoneElse', ErrorMessages:'error' }]]);
		target.resetScheduleNotes([schedule1, schedule3, schedule2], scheduleDateMoment);
		target.setInternalNoteForPerson("221B-Baker-SomeoneElse", "newNotes", scheduleDateMoment);
		$httpBackend.flush();

		var note1 = target.getInternalNoteForPerson("221B-Baker-SomeoneElse");
		expect(note1).toEqual("test");
		$httpBackend.verifyNoOutstandingRequest();
	});
});