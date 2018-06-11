﻿(function () {

	var $compile,
		$rootScope,
		fakeCurrentUserInfo;

	describe("<teamschedule-datepicker>", function () {

		beforeEach(module('wfm.templates'));
		beforeEach(module('wfm.teamSchedule'));

		beforeEach(inject(function (_$rootScope_, _$compile_) {
			$compile = _$compile_;
			$rootScope = _$rootScope_;
		}));

		it('should render datepicker correctly', function () {
			var result = setUp();
			var vm = result.commandControl;
			expect(vm).not.toBeNull();
		});

		it('should get date from outside controller', function () {
			var result = setUp(moment('2016-06-01').toDate());
			var vm = result.commandControl;

			var dateStringInput = angular.element(result.container[0].querySelector("#teamschedule-datepicker-input"));
			expect(moment(vm.selectedDate).format('YYYY-MM-DD')).toBe('2016-06-01');
			expect(moment(new Date(dateStringInput.val())).format('YYYY-MM-DD')).toBe('2016-06-01');
		});

		it('should update date string when selected day is changed', function () {
			var result = setUp('2016-06-01');
			var vm = result.commandControl;

			var inputEl = result.container[0].querySelector(".teamschedule-datepicker #teamschedule-datepicker-input");
			var dateStringInput = angular.element(inputEl);

			expect(vm.selectedDate).toBe('2016-06-01');
			expect(moment(new Date(dateStringInput.val())).format('YYYY-MM-DD')).toBe('2016-06-01');

			inputEl.value = '6/2/16';
			dateStringInput.triggerHandler('change');
			result.scope.$apply();

			expect(vm.selectedDate).toBe('2016-06-02');
			expect(moment(new Date(dateStringInput.val())).format('YYYY-MM-DD')).toBe('2016-06-02');
		});

		it('should remember preselected the date when selected day is changed with incorrect date', function () {
			var result = setUp(moment('2016-06-01').toDate());
			var vm = result.commandControl;

			expect(moment(vm.selectedDate).format('YYYY-MM-DD')).toBe('2016-06-01');

			vm.selectedDate = "";
			vm.onDateInputChange();
			expect(moment(vm.selectedDate).format("YYYY-MM-DD")).toBe('2016-06-01');
		});


	});

	function setUp(inputDate) {
		var date;
		var html = '<team-schedule-datepicker selected-date="curDate" on-date-change="onScheduleDateChanged()" />';
		var scope = $rootScope.$new();

		if (inputDate == null)
			date = moment('2016-06-15').toDate();
		else
			date = inputDate;

		scope.curDate = date;
		scope.onScheduleDateChanged = function () {

		};

		var container = $compile(html)(scope);
		scope.$apply();

		var commandControl = angular.element(container[0].querySelector(".teamschedule-datepicker")).scope().vm;

		var obj = {
			container: container,
			commandControl: commandControl,
			scope: scope
		};

		return obj;
	}
})();