'use strict';

describe('teamschedule note editor directive tests', function () {
	var controller, noteMgmt, fakeNoticeService, $httpBackend, $compile, $rootScope;

	beforeEach(function () {
		module('wfm.templates');
		module('wfm.teamSchedule');
		module(function ($provide) {
			$provide.service('NoticeService', function () {
				fakeNoticeService = new FakeNoticeService();
				return fakeNoticeService;
			});
			$provide.service('teamsToggles',
				function() {
					return {
						all: function() {
							return {
								WfmTeamSchedule_DisplayAndEditPublicNote_44783: true
							}
						}
					}
				});
		});
	});

	beforeEach(inject(function (_$rootScope_, _$compile_, _$httpBackend_) {
		$httpBackend = _$httpBackend_;
		$compile = _$compile_;
		$rootScope = _$rootScope_;
	}));

	beforeEach(inject(function (ScheduleNoteManagementService) {
		noteMgmt = ScheduleNoteManagementService;
		
	}));

	function FakeNoticeService() {
		var error = '';
		this.error = function(msg) {
			error = msg;
		}
		this.reset = function() {
			error = '';
		}
		this.getLastError = function() {
			return error;
		}
	}

	var scheduleDate = "2016-01-02";
	var scheduleDateMoment = moment(scheduleDate);

	var schedule = {
		"PersonId": "221B-Sherlock",
		"Name": "Sherlock Holmes",
		"Date": scheduleDate,
		"InternalNotes": null,
		"PublicNotes":null
	};

	it("Can set schedule note for person", inject(function () {
		$httpBackend.expectPOST("../api/TeamScheduleCommand/EditScheduleNote").respond(200, []);
		noteMgmt.resetScheduleNotes([schedule], scheduleDateMoment);

		controller = setUp();
		controller.internalNotes = "newNotes for sherlock";
		controller.publicNotes = 'new public note';
		controller.submit();
		$httpBackend.flush();

		expect(noteMgmt.getNoteForPerson("221B-Sherlock").internalNotes).toEqual("newNotes for sherlock");
		expect(noteMgmt.getNoteForPerson("221B-Sherlock").publicNotes).toEqual('new public note');
		$httpBackend.verifyNoOutstandingRequest();
	}));

	it("Should not set schedule note for person when http responds error", inject(function () {
		$httpBackend.expectPOST("../api/TeamScheduleCommand/EditScheduleNote").respond(200, [{ PersonId: '221B-Sherlock', ErrorMessages: ['error'] }]);
		noteMgmt.resetScheduleNotes([schedule], scheduleDateMoment);

		controller = setUp();
		controller.internalNotes = "newNotes for sherlock";
		controller.submit();
		$httpBackend.flush();

		expect(noteMgmt.getNoteForPerson("221B-Sherlock").internalNotes).toEqual(null);
		expect(fakeNoticeService.getLastError()).toEqual('error');
		$httpBackend.verifyNoOutstandingRequest();
	}));

	function setUp() {
		var scope = $rootScope.$new();
		var html = '<note-editor note-input-option="noteInputOption"></note-editor>';
		scope.noteInputOption = {
			personId: "221B-Sherlock",
			selectedDate: new Date(scheduleDate),
			showEditor: true
		};

		var element = $compile(html)(scope);
		scope.$apply();
		return element.isolateScope().vm;
	}
});
