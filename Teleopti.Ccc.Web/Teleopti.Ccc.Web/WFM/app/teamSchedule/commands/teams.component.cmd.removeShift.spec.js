describe('<remove-shift>', function () {
	'use strict';
	var $rootScope,
		$compile,
		$document,
		$httpBackend,
		fakeActivityService,
		fakePersonSelectionService,
		fakeNoticeService;

	beforeEach(module('wfm.templates', 'wfm.teamSchedule'));

	beforeEach(module(function ($provide) {
		$provide.service('ActivityService',
			function () {
				fakeActivityService = new FakeActivityService();
				return fakeActivityService;
			});
		$provide.service('PersonSelection', function () {
			fakePersonSelectionService = new FakePersonSelectionService();
			return fakePersonSelectionService;
		});
		$provide.service('NoticeService',
			function () {
				fakeNoticeService = new FakeNoticeService();
				return fakeNoticeService;
			});

	}));

	beforeEach(inject(function (_$rootScope_, _$compile_, _$document_, _$httpBackend_) {
		$rootScope = _$rootScope_;
		$compile = _$compile_;
		$document = _$document_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET('../ToggleHandler/AllToggles').respond(200, 'mock');
	}));

	afterEach(function () {
		$document[0].querySelector(".modal-box")
			&& $document[0].querySelector(".modal-box").remove();
	});

	it("should show confirm dialog", function () {
		var dialog = setUp("2018-02-01").dialog;
		var title = dialog.querySelector('.modal-inner-content h3');
		var contentMessage = dialog.querySelector('.modal-inner-content').innerHTML;
		var button = dialog.querySelectorAll("button");

		expect(title.innerHTML.trim()).toEqual("RemoveShift");
		expect(contentMessage.indexOf("AreYouSureToRemoveSelectedShift") > -1).toEqual(true);
		expect(button.length).toEqual(2);
		expect(button[0].innerHTML).toEqual("Cancel");
		expect(button[1].innerHTML).toEqual("Apply");
	});
	it("should disapper the dialog if click cancel button", function () {
		var dialog = setUp("2018-01-12").dialog;
		var cancelButton = dialog.querySelectorAll("button")[0];
		cancelButton.click();

		dialog = $document[0].querySelector(".modal-box");
		expect(dialog).toEqual(null);
	});

	it("should show success notification and reset active cmd when remove shift successed", function () {
		var document = setUp("2018-02-01");
		var modal = document.dialog;

		var checkedPersonInfos = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: true,
				ScheduleStartTime: null,
				ScheduleEndTime: null,
				SelectedActivities: ['activity']
			}
		];
		fakePersonSelectionService.setFakeCheckedPersonInfoList(checkedPersonInfos);

		var button = modal.querySelectorAll("button");
		var ctrl = document.removeElement.isolateScope().$ctrl;
		button[1].click();

		expect(fakeNoticeService.successMessage).toEqual('FinishedRemoveShift');
		expect(!!ctrl.containerCtrl.activeCmd).toEqual(false);
	});

	it("should show warning and success notification and reset active command when remove shift apply with warning", function () {
		var document = setUp("2018-02-01");
		var modal = document.dialog;

		var checkedPersonInfos = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: true,
				ScheduleStartTime: null,
				ScheduleEndTime: null,
				SelectedActivities: ['activity']
			}
		];
		fakePersonSelectionService.setFakeCheckedPersonInfoList(checkedPersonInfos);

		fakeActivityService.fakeRemoveShiftResponse({
			data: [{
				PersonId: 'agent1', WarningMessages: ['warning']
			}]});

		var button = modal.querySelectorAll("button");
		var ctrl = document.removeElement.isolateScope().$ctrl;
		button[1].click();

		expect(fakeNoticeService.successMessage).toEqual("FinishedRemoveShift");
		expect(fakeNoticeService.warningMessage).toEqual("warning : agent1");
		expect(!!ctrl.containerCtrl.activeCmd).toEqual(false);
	});

	it("should show error notification and reset active command when remove shift apply with error", function () {
		var document = setUp("2018-02-01");
		var modal = document.dialog;

		var checkedPersonInfos = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Checked: true,
				ScheduleStartTime: null,
				ScheduleEndTime: null,
				SelectedActivities: ['activity']
			}
		];
		fakePersonSelectionService.setFakeCheckedPersonInfoList(checkedPersonInfos);

		fakeActivityService.fakeRemoveShiftResponse({
			data: [{
				PersonId: 'agent1', ErrorMessages: ['error']
			}]
		});

		var button = modal.querySelectorAll("button");
		var ctrl = document.removeElement.isolateScope().$ctrl;
		button[1].click();

		expect(!!fakeNoticeService.successMessage).toEqual(false);
		expect(fakeNoticeService.errorMessage).toEqual("error : agent1");
		expect(!!ctrl.containerCtrl.activeCmd).toEqual(false);
	});

	commonTestsInDifferentLocale();

	function commonTestsInDifferentLocale() {

		it("should apply the command with person whose shift is day off excluded", function () {
			var document = setUp("2018-02-01");
			var modal = document.dialog;

			var checkedPersonInfos = [
				{
					PersonId: 'agent1',
					Name: 'agent1',
					Checked: true,
					ScheduleStartTime: null,
					ScheduleEndTime: null,
					SelectedActivities: ['activity']
				},
				{
					PersonId: 'agent2',
					Name: 'agent2',
					Checked: true,
					ScheduleStartTime: null,
					ScheduleEndTime: null,
					SelectedDayOffs: [],
					SelectedActivities: ['activity']
				},
				{
					PersonId: 'agent3',
					Checked: true,
					Name: 'agent3',
					ScheduleStartTime: null,
					ScheduleEndTime: null,
					SelectedDayOffs: [{ Date: '2018-02-01' }]
				}
			];
			fakePersonSelectionService.setFakeCheckedPersonInfoList(checkedPersonInfos);

			var button = modal.querySelectorAll("button");
			var ctrl = document.removeElement.isolateScope().$ctrl;
			button[1].click();

			var removeShiftData = fakeActivityService.lastRequestedData();
			expect(removeShiftData.Date).toEqual('2018-02-01');
			expect(removeShiftData.PersonIds.length).toEqual(2);
			expect(removeShiftData.PersonIds[0]).toEqual('agent1');
			expect(removeShiftData.PersonIds[1]).toEqual('agent2');
			expect(removeShiftData.TrackedCommandInfo.TrackId).toEqual(ctrl.trackId);
		});

		it("should apply the command with person whose shift is full day absence excluded", function () {
			var document = setUp("2018-02-01");
			var modal = document.dialog;

			var checkedPersonInfos = [
				{
					PersonId: 'agent1',
					Name: 'agent1',
					Checked: true,
					ScheduleStartTime: null,
					ScheduleEndTime: null,
					SelectedActivities: ['activity']
				},
				{
					PersonId: 'agent2',
					Name: 'agent2',
					Checked: true,
					ScheduleStartTime: null,
					ScheduleEndTime: null,
					SelectedDayOffs: [],
					SelectedActivities: ['activity']
				},
				{
					PersonId: 'agent3',
					Checked: true,
					Name: 'agent3',
					ScheduleStartTime: null,
					ScheduleEndTime: null,
					IsFullDayAbsence: true,
					SelectedDayOffs: [],
					SelectedAbsences: ['absence']
				}
			];
			fakePersonSelectionService.setFakeCheckedPersonInfoList(checkedPersonInfos);

			var button = modal.querySelectorAll("button");
			var ctrl = document.removeElement.isolateScope().$ctrl;
			button[1].click();

			var removeShiftData = fakeActivityService.lastRequestedData();
			expect(removeShiftData.Date).toEqual('2018-02-01');
			expect(removeShiftData.PersonIds.length).toEqual(2);
			expect(removeShiftData.PersonIds[0]).toEqual('agent1');
			expect(removeShiftData.PersonIds[1]).toEqual('agent2');
			expect(removeShiftData.TrackedCommandInfo.TrackId).toEqual(ctrl.trackId);
		});

		it("should apply the command with person whose shift only contains overtime excluded", function () {
			var document = setUp("2018-02-01");
			var modal = document.dialog;

			var checkedPersonInfos = [
				{
					PersonId: 'agent1',
					Name: 'agent1',
					Checked: true,
					ScheduleStartTime: null,
					ScheduleEndTime: null,
					SelectedActivities: ['activity']
				},
				{
					PersonId: 'agent2',
					Name: 'agent2',
					Checked: true,
					ScheduleStartTime: null,
					ScheduleEndTime: null,
					SelectedDayOffs: [],
					SelectedActivities: ['activity']
				},
				{
					PersonId: 'agent3',
					Checked: true,
					Name: 'agent3',
					ScheduleStartTime: null,
					ScheduleEndTime: null,
					SelectedDayOffs: [],
					SelectedActivities: [{ isOvertime: true }]
				}
			];
			fakePersonSelectionService.setFakeCheckedPersonInfoList(checkedPersonInfos);

			var button = modal.querySelectorAll("button");
			var ctrl = document.removeElement.isolateScope().$ctrl;
			button[1].click();

			var removeShiftData = fakeActivityService.lastRequestedData();
			expect(removeShiftData.Date).toEqual('2018-02-01');
			expect(removeShiftData.PersonIds.length).toEqual(2);
			expect(removeShiftData.PersonIds[0]).toEqual('agent1');
			expect(removeShiftData.PersonIds[1]).toEqual('agent2');
			expect(removeShiftData.TrackedCommandInfo.TrackId).toEqual(ctrl.trackId);
		});

		it("should apply the command with person whose schedule is empty excluded", function () {
			var document = setUp("2018-02-01");
			var modal = document.dialog;

			var checkedPersonInfos = [
				{
					PersonId: 'agent1',
					Name: 'agent1',
					Checked: true,
					ScheduleStartTime: null,
					ScheduleEndTime: null,
					SelectedActivities: ['activity']
				},
				{
					PersonId: 'agent2',
					Name: 'agent2',
					Checked: true,
					ScheduleStartTime: null,
					ScheduleEndTime: null,
					SelectedDayOffs: [],
					SelectedActivities: ['activity']
				},
				{
					PersonId: 'agent3',
					Checked: true,
					Name: 'agent3',
					ScheduleStartTime: null,
					ScheduleEndTime: null,
					SelectedDayOffs: [],
					SelectedActivities: []
				}
			];
			fakePersonSelectionService.setFakeCheckedPersonInfoList(checkedPersonInfos);

			var button = modal.querySelectorAll("button");
			var ctrl = document.removeElement.isolateScope().$ctrl;
			button[1].click();

			var removeShiftData = fakeActivityService.lastRequestedData();
			expect(removeShiftData.Date).toEqual('2018-02-01');
			expect(removeShiftData.PersonIds.length).toEqual(2);
			expect(removeShiftData.PersonIds[0]).toEqual('agent1');
			expect(removeShiftData.PersonIds[1]).toEqual('agent2');
			expect(removeShiftData.TrackedCommandInfo.TrackId).toEqual(ctrl.trackId);
		});

		it("should apply the command with person who is unchecked excluded", function () {
			var document = setUp("2018-02-01");
			var modal = document.dialog;

			var checkedPersonInfos = [
				{
					PersonId: 'agent1',
					Name: 'agent1',
					Checked: true,
					ScheduleStartTime: null,
					ScheduleEndTime: null,
					SelectedActivities: ['activity']
				},
				{
					PersonId: 'agent2',
					Name: 'agent2',
					Checked: true,
					ScheduleStartTime: null,
					ScheduleEndTime: null,
					SelectedDayOffs: [],
					SelectedActivities: ['activity']
				},
				{
					PersonId: 'agent3',
					Checked: false,
					Name: 'agent3',
					ScheduleStartTime: null,
					ScheduleEndTime: null,
					SelectedDayOffs: [],
					SelectedActivities: ['activity']
				}
			];
			fakePersonSelectionService.setFakeCheckedPersonInfoList(checkedPersonInfos);

			var button = modal.querySelectorAll("button");
			var ctrl = document.removeElement.isolateScope().$ctrl;
			button[1].click();

			var removeShiftData = fakeActivityService.lastRequestedData();
			expect(removeShiftData.Date).toEqual('2018-02-01');
			expect(removeShiftData.PersonIds.length).toEqual(2);
			expect(removeShiftData.PersonIds[0]).toEqual('agent1');
			expect(removeShiftData.PersonIds[1]).toEqual('agent2');
			expect(removeShiftData.TrackedCommandInfo.TrackId).toEqual(ctrl.trackId);
		});
	}

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


	function setUp(inputDate) {
		var date = inputDate || '2016-06-15';
		var html = '<teamschedule-command-container date="curDate" timezone="timezone"></teamschedule-command-container>';
		var scope = $rootScope.$new();

		scope.curDate = date;

		var container = $compile(html)(scope);
		scope.$apply();

		var vm = container.isolateScope().vm;
		vm.setReady(true);
		vm.setActiveCmd('RemoveShift');
		scope.$apply();

		var popDialog = $document[0].querySelector(".modal-box");
		var removeElement = angular.element(container[0].querySelector("remove-shift"));
		return {
			dialog: popDialog,
			removeElement: removeElement
		}
	}

	function FakeActivityService() {
		var requestedData, curResponse;
		this.fakeRemoveShiftResponse = function (response) {
			curResponse = response;
		}
		this.lastRequestedData = function () {
			return requestedData;
		}
		this.removeShift = function (input) {
			requestedData = input;
			return {
				then: function (cb) { cb(curResponse || {data:[]})}};
		}
	}

	function FakePersonSelectionService() {
		var checkedPersonList = [];

		this.setFakeCheckedPersonInfoList = function (input) {
			checkedPersonList = input || personList;
		}

		this.getCheckedPersonInfoList = function () {
			return checkedPersonList;
		}


	}

	function FakeNoticeService() {
		this.successMessage = '';
		this.errorMessage = '';
		this.warningMessage = '';
		this.success = function (message, time, destroyOnStateChange) {
			this.successMessage = message;
		}
		this.error = function (message, time, destroyOnStateChange) {
			this.errorMessage = message;
		}
		this.warning = function (message, time, destroyOnStateChange) {
			this.warningMessage = message;
		}
	}
});