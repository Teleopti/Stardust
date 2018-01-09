fdescribe('<add-day-off>',
	function () {
		'use strict';
		var $rootScope,
			$compile,
			$document,
			$q,
			$httpBackend,
			fakeDayOffService;

		beforeEach(module('wfm.templates', 'wfm.teamSchedule'));

		beforeEach(module(function ($provide) {
			$provide.service('DayOffService',
				function () {
					fakeDayOffService = new FakeDayOffService();
					return fakeDayOffService;
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
			var applyButton = form[0].querySelectorAll("#applyDayOff");
			expect(applyButton.length).toEqual(1);
		});
		it("should get available day off templates", function () {
			var panel = setUp("2018-01-09");

			var templatesDropdown = panel[0].querySelector(".dayoff-selector");
			templatesDropdown.click();
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
		

		it("should disable apply button without template selected", function () {
			var date = "2018-01-09";
			var panel = setUp(date);
			var applyButton = panel[0].querySelector("#applyDayOff");
			expect(applyButton.disabled).toEqual(true);
		});

		it("should disable apply button without start date", function () {
			var date = "2018-01-09";
			var panel = setUp(date);

			var ctrl = panel.isolateScope().$ctrl;
			ctrl.dateRange.startDate = "";

			var templatesDropdown = panel[0].querySelector(".dayoff-selector");
			templatesDropdown.click();
			var templates = templatesDropdown.querySelectorAll("md-option");
			templates[0].click();

			var applyButton = panel[0].querySelector("#applyDayOff");
			expect(applyButton.disabled).toEqual(true);
		});

		it("should disable apply button without end date", function () {
			var date = "2018-01-09";
			var panel = setUp(date);

			var ctrl = panel.isolateScope().$ctrl;
			ctrl.dateRange.endDate = "";

			var templatesDropdown = panel[0].querySelector(".dayoff-selector");
			templatesDropdown.click();
			var templates = templatesDropdown.querySelectorAll("md-option");
			templates[0].click();
			
			var applyButton = panel[0].querySelector("#applyDayOff");
			expect(applyButton.disabled).toEqual(true);
		});
		it("should disable apply button if the date range is not correct", function () {
			var date = "2018-01-09";
			var panel = setUp(date);

			var ctrl = panel.isolateScope().$ctrl;
			ctrl.dateRange = {
				startDate: moment("2017-01-11").toDate(),
				endDate: moment("2017-01-10").toDate()
			};

			var templatesDropdown = panel[0].querySelector(".dayoff-selector");
			templatesDropdown.click();
			var templates = templatesDropdown.querySelectorAll("md-option");
			templates[0].click();

			var applyButton = panel[0].querySelector("#applyDayOff");
			expect(applyButton.disabled).toEqual(true);
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

			document.body.append(container[0]);

			var element = angular.element(container[0].querySelector("add-day-off"));
			return element;
		}

		function FakeDayOffService() {
			this.getAvailableTemplates = function () {
				return $q(function (resolve, reject) {
					resolve([
						{ Id: 'template1', Name: 'template1' },
						{ Id: 'template2', Name: 'template2' }
					]);
				});
			}
		}

	});