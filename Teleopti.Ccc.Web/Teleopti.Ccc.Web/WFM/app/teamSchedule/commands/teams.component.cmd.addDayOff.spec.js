describe('<add-day-off>',
	function () {
		'use strict';
		var $rootScope,
			$compile,
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

		beforeEach(inject(function (_$rootScope_, _$compile_, _$q_, _$httpBackend_) {
			$rootScope = _$rootScope_;
			$compile = _$compile_;
			$q = _$q_;
			$httpBackend = _$httpBackend_;
			$httpBackend.expectGET('../ToggleHandler/AllToggles').respond(200, 'mock');

		}));

		it("should render adding day off command panel correctly", function () {
			var panel = setUp("2018-01-09");
			var form = panel[0].querySelectorAll("form");
			expect(form.length).toEqual(1);
			var template = form[0].querySelectorAll(".dayoff-selector");
			expect(template.length).toEqual(1);
			var picker = form[0].querySelectorAll('.teamschedule-datepicker');
			expect(picker.length).toEqual(2);
			var applyButton = panel[0].querySelectorAll("#applyDayOff");
			expect(applyButton.length).toEqual(1);
		});

		it("should get available day off templates", function () {
			var panel = setUp("2018-01-09");

			var templatesDropdown = panel[0].querySelector(".dayoff-selector");
			var templates = templatesDropdown.querySelectorAll("md-option");

			expect(templates.length).toEqual(2);
			expect(templates[0].innerText.trim()).toEqual('template1');
			expect(templates[1].innerText.trim()).toEqual('template2');
		});

		it("should set default day off type from the available template", function () {
			var panel = setUp("2018-01-09");
			var templatesDropdown = panel[0].querySelector(".dayoff-selector");
			var selectedTemplate = templatesDropdown.querySelector("md-select-value");
			expect(selectedTemplate.innerText.trim()).toEqual("template1");
		});

		it("should set default date range", function () {
			var date = "2018-01-09";
			var panel = setUp(date);
			var datePicker = panel[0].querySelectorAll('.teamschedule-datepicker');
			expect(datePicker[0].querySelector('input').value).toEqual('1/9/18');
			expect(datePicker[1].querySelector('input').value).toEqual('1/9/18');
		});

		it("should disable apply button unless without template", function () {
			var date = "2018-01-09";
			var panel = setUp(date);
			var scope = panel.isolateScope();
			var ctrl = scope.$ctrl;

			fakePersonSelectionService.setFakeCheckedPersonInfoList();
			scope.$apply();

			expectApplyButtonStatus(false, panel);

			ctrl.selectedTemplateId = "";
			scope.$apply();

			expectApplyButtonStatus(true, panel);
		});

		it("should disable apply button unless without start date", function () {
			var date = "2018-01-09";
			var panel = setUp(date);
			var scope = panel.isolateScope();
			var ctrl = scope.$ctrl;

			fakePersonSelectionService.setFakeCheckedPersonInfoList();
			ctrl.selectedTemplateId = "template1";
			scope.$apply();

			expectApplyButtonStatus(false, panel);

			ctrl.dateRange.startDate = "";
			scope.$apply();

			expectApplyButtonStatus(true, panel);
		});

		it("should disable apply button unless without end date", function () {
			var date = "2018-01-09";
			var panel = setUp(date);
			var scope = panel.isolateScope();
			var ctrl = scope.$ctrl;

			ctrl.selectedTemplateId = "template1";
			fakePersonSelectionService.setFakeCheckedPersonInfoList();
			scope.$apply();

			expectApplyButtonStatus(false, panel);

			ctrl.dateRange.endDate = "";
			scope.$apply();

			expectApplyButtonStatus(true, panel);
		});

		it("should disable apply button and show error message unless the date range is not correct", function () {
			var date = "2018-01-09";
			var panel = setUp(date);
			var scope = panel.isolateScope();
			var ctrl = scope.$ctrl;
			ctrl.dateRange = {
				startDate: moment("2017-01-11").toDate(),
				endDate: moment("2017-01-10").toDate()
			};
			fakePersonSelectionService.setFakeCheckedPersonInfoList();
			ctrl.selectedTemplateId = "template1";
			scope.$apply();

			expectApplyButtonStatus(true, panel);
			var errorMessage = panel[0].querySelectorAll(".text-danger");
			expect(errorMessage.length).toEqual(1);

			ctrl.dateRange = {
				startDate: moment("2017-01-10").toDate(),
				endDate: moment("2017-01-10").toDate()
			};
			scope.$apply();

			expectApplyButtonStatus(false, panel);
			errorMessage = panel[0].querySelectorAll(".text-danger");
			expect(errorMessage.length).toEqual(0);
		});

		it("should disable apply button and show progress linear unless is not processing", function () {
			var date = "2018-01-09";
			var panel = setUp(date);
			var scope = panel.isolateScope();
			var ctrl = scope.$ctrl;

			ctrl.runningCommand = true;
			ctrl.selectedTemplateId = "template1";
			fakePersonSelectionService.setFakeCheckedPersonInfoList();
			scope.$apply();

			expectApplyButtonStatus(true, panel);

			var progressLinear = panel[0].querySelectorAll("md-progress-linear");
			expect(progressLinear.length).toEqual(1);

			ctrl.runningCommand = false;
			scope.$apply();

			expectApplyButtonStatus(false, panel);
			progressLinear = panel[0].querySelectorAll("md-progress-linear");
			expect(progressLinear.length).toEqual(0);
		});

		it("should disable apply button unless some agents are selected", function () {
			var panel = setUp("2018-01-09");
			var scope = panel.isolateScope();
			var ctrl = scope.$ctrl;

			ctrl.selectedTemplateId = "template1";
			scope.$apply();

			expectApplyButtonStatus(true, panel);

			fakePersonSelectionService.setFakeCheckedPersonInfoList();
			scope.$apply();

			expectApplyButtonStatus(false, panel);
		});


		it('should not call addDayOff if apply button is disabled', function () {
			var panel = setUp("2018-01-09");
			var scope = panel.isolateScope();
			var ctrl = scope.$ctrl;

			ctrl.selectedTemplateId = "template1";
			scope.$apply();

			expectApplyButtonStatus(true, panel);

			var applyButton = panel[0].querySelector("#applyDayOff");
			applyButton.click();
			expect(fakeDayOffService.lastPostData).toEqual(null);
		});

		it('should show success notification and reset active command when add day off apply succeed', function () {
			var result = setUpAndApplyDayOff();
			expect(fakeNoticeService.successMessage).toEqual('SuccessfulMessageForAddingDayOff');
			expect(!!result.scope.$ctrl.containerCtrl.activeCmd).toEqual(false);
		});

		it('should show warning and success notification and reset active command when add day off apply with warning', function () {
			var result = setUpAndApplyDayOff({
				data: [{
					PersonId: 'agent1', WarningMessages: ['warning']
				}]
			});
			
			expect(fakeNoticeService.successMessage).toEqual('SuccessfulMessageForAddingDayOff');
			expect(fakeNoticeService.warningMessage).toEqual('warning : agent1');
			expect(!!result.scope.$ctrl.containerCtrl.activeCmd).toEqual(false);
		});

		it('should show error notification and reset active command when add day off apply with error', function () {
			var result = setUpAndApplyDayOff({
				data: [{
					PersonId: 'agent1', ErrorMessages: ['error' ]
				}]
			});
			expect(fakeNoticeService.successMessage).toEqual('');
			expect(fakeNoticeService.errorMessage).toEqual('error : agent1');
			expect(!!result.scope.$ctrl.containerCtrl.activeCmd).toEqual(false);
		});

		commonTestsInDifferentLocale();

		function commonTestsInDifferentLocale() {

			it('should call add day off when click apply with correct data', function () {
				var date = "2018-01-09";
				var panel = setUp(date);
				var scope = panel.isolateScope();
				var ctrl = scope.$ctrl;

				ctrl.selectedTemplateId = "template1";
				fakePersonSelectionService.setFakeCheckedPersonInfoList();
				scope.$apply();

				var applyButton = panel[0].querySelector("#applyDayOff");
				applyButton.click();

				var dayOffData = fakeDayOffService.lastPostData;
				expect(dayOffData.StartDate).toEqual(date);
				expect(dayOffData.EndDate).toEqual(date);
				expect(dayOffData.TemplateId).toEqual("template1");
				expect(dayOffData.PersonIds).toEqual(personList.map(function (person) { return person.PersonId; }));
				expect(dayOffData.TrackedCommandInfo.TrackId).toEqual(ctrl.trackId);
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

		function setUp(inputDate) {
			var date = inputDate || '2016-06-15';

			var html = '<teamschedule-command-container date="curDate" timezone="timezone"></teamschedule-command-container>';
			var scope = $rootScope.$new();

			scope.curDate = date;

			var container = $compile(html)(scope);
			scope.$apply();

			var vm = container.isolateScope().vm;
			vm.setReady(true);
			vm.setActiveCmd('AddDayOff');
			scope.$apply();

			var element = angular.element(container[0].querySelector("add-day-off"));
			return element;
		}

		function setUpAndApplyDayOff(applyResponse) {
			var panel = setUp("2018-01-09");
			var scope = panel.isolateScope();
			var ctrl = scope.$ctrl;

			ctrl.selectedTemplateId = "template1";
			fakePersonSelectionService.setFakeCheckedPersonInfoList();
			scope.$apply();

			fakeDayOffService.setApplyResponse(applyResponse);

			var applyButton = panel[0].querySelector("#applyDayOff");
			applyButton.click();
			return {
				panel: panel,
				scope: scope
			};
		}

		function expectApplyButtonStatus(isDisabled, panel) {
			var applyButton = panel[0].querySelector("#applyDayOff");
			if (isDisabled) {
				expect(applyButton.disabled).toEqual(true);
				expect(applyButton.className.indexOf('wfm-btn-primary-disabled') > -1).toEqual(true);
			} else {
				expect(applyButton.disabled).toEqual(false);
				expect(applyButton.className.indexOf('wfm-btn-primary-disabled') > -1).toEqual(false);
				expect(applyButton.className.indexOf('wfm-btn-primary') > -1).toEqual(true);
			}
		}

		function FakeDayOffService() {
			var self = this;
			var applyResponse = null;
			this.lastPostData = null;
			this.getAllDayOffTemplates = function () {
				return $q(function (resolve, reject) {
					resolve([
						{ Id: 'template1', Name: 'template1' },
						{ Id: 'template2', Name: 'template2' }
					]);
				});
			}
			this.setApplyResponse = function (response) {
				applyResponse = response;
			}
			this.addDayOff = function (input) {
				this.lastPostData = input;
				return $q(function (resolve, reject) {
					resolve(applyResponse || { data: [] });
				});
			}
		}

		var personList = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				ScheduleStartTime: null,
				ScheduleEndTime: null
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

