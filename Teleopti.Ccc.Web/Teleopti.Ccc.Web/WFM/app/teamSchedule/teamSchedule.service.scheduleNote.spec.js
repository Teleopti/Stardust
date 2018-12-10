"use strict";

describe("teamschedule schedule note management service tests", function () {
	var target, $httpBackend;

	beforeEach(function () {
		module("wfm.teamSchedule");
	});

	beforeEach(inject(function (ScheduleNoteManagementService, _$httpBackend_) {
		target = ScheduleNoteManagementService;
		$httpBackend = _$httpBackend_;
	}));

	var scheduleDate = "2016-01-02";
	var yesterday = "2016-01-01";

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

	function commonTestsInDifferentLocale() {
		it("Can create note dictionary for selected date", inject(function () {
			target.resetScheduleNotes([schedule1, schedule3, schedule2], scheduleDate);
			var note1 = target.getNoteForPerson("221B-Baker-SomeoneElse");
			var note2 = target.getNoteForPerson("221B-Sherlock");
			expect(note1.internalNotes).toEqual("test");
			expect(note1.publicNotes).toEqual("public");
			expect(note2.publicNotes).toEqual(null);
			expect(note2.internalNotes).toEqual(null);
		}));

		it('should subimit note for person with correct data', function () {
			target.submitNoteForPerson('personId', { internalNotes: ['internal note'], publicNotes: ['public note'] }, '2018-02-26');

			$httpBackend.expect('POST', '../api/TeamScheduleCommand/EditScheduleNote', {
				SelectedDate: '2018-02-26',
				PersonId: 'personId',
				InternalNote: ['internal note'],
				PublicNote: ['public note']

			}).respond(200, { success: true });

			$httpBackend.flush();
		});
	}

	commonTestsInDifferentLocale();

	describe('in locale ar-AE', function () {
		beforeAll(function () {
			moment.locale('ar-AE');
		});

		afterAll(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});

	describe('in locale fa-IR', function () {
		beforeAll(function () {
			moment.locale('fa-IR');
		});

		afterAll(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});
});
