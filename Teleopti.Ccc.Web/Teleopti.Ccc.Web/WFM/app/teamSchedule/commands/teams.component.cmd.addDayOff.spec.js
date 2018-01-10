fdescribe('<add-day-off>',
	function () {
		'use strict';
		var $rootScope,
			$compile,
			$document,
			$q,
			$httpBackend,
			fakeDayOffService,
			fakePersonSelectionService;

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
		}));

		beforeEach(inject(function (_$rootScope_, _$compile_, _$document_, _$q_, _$httpBackend_) {
			$rootScope = _$rootScope_;
			$compile = _$compile_;
			$document = _$document_;
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

		it("should set default date range", function () {
			var date = "2018-01-09";
			var panel = setUp(date);
			var datePicker = panel[0].querySelectorAll('.teamschedule-datepicker');
			expect(moment(new Date(datePicker[0].querySelector('input').value)).format("YYYY-MM-DD")).toEqual(date);
			expect(moment(new Date(datePicker[1].querySelector('input').value)).format("YYYY-MM-DD")).toEqual(date);
		});

		it("should disable apply button unless without template", function () {
			var date = "2018-01-09";
			var panel = setUp(date);
			var scope = panel.isolateScope();
			var ctrl = scope.$ctrl;

			fakePersonSelectionService.setFakeCheckedPersonInfoList();
			scope.$apply();

			var applyButton = panel[0].querySelector("#applyDayOff");
			expect(applyButton.disabled).toEqual(true);

			ctrl.selectedTemplateId = "template1";
			scope.$apply();

			applyButton = panel[0].querySelector("#applyDayOff");
			expect(applyButton.disabled).toEqual(false);
		});

		it("should disable apply button unless without start date", function () {
			var date = "2018-01-09";
			var panel = setUp(date);
			var scope = panel.isolateScope();
			var ctrl = scope.$ctrl;

			fakePersonSelectionService.setFakeCheckedPersonInfoList();
			ctrl.selectedTemplateId = "template1";
			scope.$apply();

			var applyButton = panel[0].querySelector("#applyDayOff");
			expect(applyButton.disabled).toEqual(false);

			ctrl.dateRange.startDate = "";
			scope.$apply();

			applyButton = panel[0].querySelector("#applyDayOff");
			expect(applyButton.disabled).toEqual(true);
		});

		it("should disable apply button unless without end date", function () {
			var date = "2018-01-09";
			var panel = setUp(date);
			var scope = panel.isolateScope();
			var ctrl = scope.$ctrl;

			ctrl.selectedTemplateId = "template1";
			fakePersonSelectionService.setFakeCheckedPersonInfoList();
			scope.$apply();

			var applyButton = panel[0].querySelector("#applyDayOff");
			expect(applyButton.disabled).toEqual(false);
			
			ctrl.dateRange.endDate = "";
			scope.$apply();

			applyButton = panel[0].querySelector("#applyDayOff");
			expect(applyButton.disabled).toEqual(true);
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

			var applyButton = panel[0].querySelector("#applyDayOff");
			expect(applyButton.disabled).toEqual(true);
			var errorMessage = panel[0].querySelectorAll(".text-danger");
			expect(errorMessage.length).toEqual(1);

			ctrl.dateRange = {
				startDate: moment("2017-01-10").toDate(),
				endDate: moment("2017-01-10").toDate()
			};
			scope.$apply();

			applyButton = panel[0].querySelector("#applyDayOff");
			expect(applyButton.disabled).toEqual(false);
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

			var applyButton = panel[0].querySelector("#applyDayOff");
			expect(applyButton.disabled).toEqual(true);
			var progressLinear = panel[0].querySelectorAll("md-progress-linear");
			expect(progressLinear.length).toEqual(1);

			ctrl.runningCommand = false;
			scope.$apply();

			applyButton = panel[0].querySelector("#applyDayOff");
			expect(applyButton.disabled).toEqual(false);
			progressLinear = panel[0].querySelectorAll("md-progress-linear");
			expect(progressLinear.length).toEqual(0);
		});

		it("should disable apply button unless some agents are selected", function () {
			var panel = setUp("2018-01-09");
			var scope = panel.isolateScope();
			var ctrl = scope.$ctrl;

			ctrl.selectedTemplateId = "template1";
			scope.$apply();

			var applyButton = panel[0].querySelector("#applyDayOff");
			expect(applyButton.disabled).toEqual(true);

			fakePersonSelectionService.setFakeCheckedPersonInfoList();
			scope.$apply();

			applyButton = panel[0].querySelector("#applyDayOff");
			expect(applyButton.disabled).toEqual(false);
		});

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
		});

		function setUp(inputDate) {
			var date;
			var html = '<teamschedule-command-container date="curDate" timezone="timezone"></teamschedule-command-container>';
			var scope = $rootScope.$new();
			if (inputDate == null)
				date = moment('2016-06-15').toDate();
			else
				date = new Date(inputDate);

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

		function FakeDayOffService() {
			this.lastPostData = null;
			this.getAvailableTemplates = function () {
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
		}

		function FakePersonSelectionService() {
			var fakePersonList = [];

			this.setFakeCheckedPersonInfoList = function (input) {
				fakePersonList = input || [
					{
						PersonId: 'agent1',
						Name: 'agent1',
						ScheduleStartTime: null,
						ScheduleEndTime: null
					}];
			}

			this.getCheckedPersonInfoList = function () {
				return fakePersonList;
			}
		}

	});