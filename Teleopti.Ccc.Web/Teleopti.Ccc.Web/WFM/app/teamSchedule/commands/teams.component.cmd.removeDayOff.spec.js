describe('<remove-day-off>',
	function () {
		'use strict';
		var $rootScope,
			$compile,
			$document,
			$q,
			$httpBackend,
			fakeDayOffService,
			fakePersonSelectionService,
			fakeNoticeService;

		beforeEach(module('wfm.templates', 'wfm.teamSchedule'));

		beforeEach(module(function ($provide) {
			$provide.service('DayOffService',
				function () {
					fakeDayOffService = new FakeDayOffService();
					return fakeDayOffService;
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

		beforeEach(inject(function (_$rootScope_, _$compile_, _$document_, _$q_, _$httpBackend_) {
			$rootScope = _$rootScope_;
			$compile = _$compile_;
			$document = _$document_;
			$q = _$q_;
			$httpBackend = _$httpBackend_;
			$httpBackend.expectGET('../ToggleHandler/AllToggles').respond(200, 'mock');
		}));

		afterEach(function () {
			$document[0].querySelector(".modal-box")
				&& $document[0].querySelector(".modal-box").remove();
		});

		it("should show confirm dialog", function () {
			var dialog = setUp("2018-01-12").dialog;

			var title = dialog.querySelector('.modal-inner-content h3');
			var button = dialog.querySelectorAll("button");

			expect(title.innerHTML.trim()).toEqual("RemoveDayOff");
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

		

		it("should show success notification and reset active cmd when remove day off successed", function () {
			var result = setUpAndApplyRemoveDayOff("2018-01-12");
			expect(fakeNoticeService.successMessage).toEqual('FinishedRemoveDayOff');
			expect(!!result.ctrl.containerCtrl.activeCmd).toEqual(false);
		});
		it('should show warning and success notification and reset active command when remove day off apply with warning', function () {
			var result = setUpAndApplyRemoveDayOff("2018-01-12", {
				data: [{
					PersonId: 'agent1', WarningMessages: ['warning']
				}]
			});
			expect(fakeNoticeService.successMessage).toEqual("FinishedRemoveDayOff");
			expect(fakeNoticeService.warningMessage).toEqual("warning : agent1");
			expect(!!result.ctrl.containerCtrl.activeCmd).toEqual(false);
		});
		it('should show error notification and reset active command when remove day off apply with error', function () {
			var result = setUpAndApplyRemoveDayOff("2018-01-12", {
				data: [{
					PersonId: 'agent1', ErrorMessages: ['error']
				}]
			});
			expect(fakeNoticeService.successMessage).toEqual("");
			expect(fakeNoticeService.errorMessage).toEqual("error : agent1");
			expect(!!result.ctrl.containerCtrl.activeCmd).toEqual(false);
		});

		commonTestsInDifferentLocale();

		function commonTestsInDifferentLocale() {
			it("should call remove day off when click apply button with selected day offs", function () {
				var date = "2018-01-12";
				var document = setUp(date);
				var checkedPersonInfos = [
					{
						PersonId: 'agent1',
						Name: 'agent1',
						ScheduleStartTime: null,
						ScheduleEndTime: null,
						SelectedDayOffs: [{ Date: date }]
					},
					{
						PersonId: 'agent2',
						Name: 'agent2',
						ScheduleStartTime: null,
						ScheduleEndTime: null,
						SelectedDayOffs: []
					},
					{
						PersonId: 'agent3',
						Name: 'agent3',
						ScheduleStartTime: null,
						ScheduleEndTime: null,
						SelectedDayOffs: [{ Date: '2018-01-11' }]
					}
				];
				fakePersonSelectionService.setFakeCheckedPersonInfoList(checkedPersonInfos);

				var dialog = document.dialog;
				var ctrl = document.removeElement.isolateScope().$ctrl;

				var applyButton = dialog.querySelectorAll("button")[1];
				applyButton.click();

				var removeDayOffData = fakeDayOffService.lastPostData;
				expect(removeDayOffData.Date).toEqual(date);
				expect(removeDayOffData.PersonIds.length).toEqual(1);
				expect(removeDayOffData.PersonIds[0]).toEqual('agent1');
				expect(removeDayOffData.TrackedCommandInfo.TrackId).toEqual(ctrl.trackId);
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
			beforeEach(function () {
				moment.locale('fa-IR');
			});

			afterEach(function () {
				moment.locale('en');
			});

			commonTestsInDifferentLocale();
		});


		function setUpAndApplyRemoveDayOff(date, applyResponse) {
			personList[0].SelectedDayOffs.push({ Date: date });
			var document = setUp(date);
			fakePersonSelectionService.setFakeCheckedPersonInfoList();

			var dialog = document.dialog;
			var ctrl = document.removeElement.isolateScope().$ctrl;

			fakeDayOffService.setApplyResponse(applyResponse);
			var applyButton = dialog.querySelectorAll("button")[1];
			applyButton.click();

			return {
				ctrl: ctrl
			}
		}

		function setUp(inputDate) {
			var date = inputDate || '2016-06-15';
			var html = '<teamschedule-command-container date="curDate" timezone="timezone"></teamschedule-command-container>';
			var scope = $rootScope.$new();

			scope.curDate = date;

			var container = $compile(html)(scope);
			scope.$apply();

			var vm = container.isolateScope().vm;
			vm.setReady(true);
			vm.setActiveCmd('RemoveDayOff');
			scope.$apply();

			var popDialog = $document[0].querySelector(".modal-box");
			var removeElement = angular.element(container[0].querySelector("remove-day-off"));
			return {
				dialog: popDialog,
				removeElement: removeElement
			}
		}

		function FakeDayOffService() {
			this.lastPostData = null;
			var applyResponse = null;

			this.getAllDayOffTemplates = function () {
				return $q(function (resolve, reject) {
					resolve([
						{ Id: 'template1', Name: 'template1' },
						{ Id: 'template2', Name: 'template2' }
					]);
				});
			}
			this.addDayOff = function (input) {
				this.lastPostData = input;
			}
			this.removeDayOff = function (input) {
				this.lastPostData = input;
				return $q(function (resolve, reject) {
					resolve(applyResponse || { data: [] });
				});
			}
			this.setApplyResponse = function (response) {
				applyResponse = response;
			}
		}

		var personList = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				ScheduleStartTime: null,
				ScheduleEndTime: null,
				SelectedDayOffs:[]
			}];

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