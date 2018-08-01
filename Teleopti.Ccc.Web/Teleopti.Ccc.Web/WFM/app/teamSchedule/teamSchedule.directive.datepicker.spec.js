(function () {

	var $compile,
		$rootScope,
		$timeout;

	describe("<teamschedule-datepicker>", function () {
		beforeEach(function () {
			module('wfm.templates');
			module("wfm.teamSchedule");
			module(function ($provide) {
				$provide.service('CurrentUserInfo', function () {
					return {
						CurrentUserInfo: function () {
							return { "FirstDayOfWeek": 0, DefaultTimeZone: 'Europe/Berlin' };
						}
					};
				});
			});
		});

		beforeEach(inject(function (_$rootScope_, _$compile_, _$timeout_) {
			$compile = _$compile_;
			$rootScope = _$rootScope_;
			$timeout = _$timeout_;
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
			expect(vm.selectedDate).toBe('2016-06-01');
			expect(inputEl.value).toBe('6/1/16');

			inputEl.value = '6/2/16';
			angular.element(inputEl).triggerHandler('change');

			$timeout(function () {
				expect(vm.selectedDate).toBe('2016-06-02');
				expect(inputEl.value).toBe('6/2/16');
			}, 300);

			$timeout.flush();

		});

		it('should remember preselected the date when selected day is changed with incorrect date', function () {
			var result = setUp(moment('2016-06-01').toDate());
			var vm = result.commandControl;

			expect(moment(vm.selectedDate).format('YYYY-MM-DD')).toBe('2016-06-01');

			vm.selectedDate = "";
			vm.onDateInputChange();
			$timeout(function () {
				expect(moment(vm.selectedDate).format("YYYY-MM-DD")).toBe('2016-06-01');
			}, 300);
		});


	});

	describe('<teamschedule-datepicker> in ar-OM', function () {
		beforeEach(function () { moment.locale("ar-OM"); });
		afterEach(function () { moment.locale("en"); });
		beforeEach(function () {
			module('wfm.templates');
			module("wfm.teamSchedule");
			module(function ($provide) {
				$provide.service('CurrentUserInfo', function () {
					return {
						CurrentUserInfo: function () {
							return { "FirstDayOfWeek": 0, DefaultTimeZone: 'Europe/Berlin' };
						}
					};
				});
			});
		});

		beforeEach(inject(function (_$rootScope_, _$compile_) {
			$compile = _$compile_;
			$rootScope = _$rootScope_;
		}));

		it('should render first day of week correctly', function () {
			var result = setUp('2018-06-12');
			var picker = result.container[0].querySelector('.toggle-datepicker');
			picker.click();

			var currentWeekEl = result.container[0].querySelectorAll('.wfm-datepicker tbody tr')[1];
			var firstDayOfWeekEl = currentWeekEl.querySelectorAll("td span")[0];
			expect(firstDayOfWeekEl.innerHTML).toEqual("03");
		});
	});


	describe('<teamschedule-datepicker> in sv',
		function () {
			beforeEach(function () { moment.locale("zh-CN"); });
			afterEach(function () { moment.locale("en"); });

			beforeEach(function () {
				module('wfm.templates');
				module("wfm.teamSchedule");
				module(function ($provide) {
					$provide.service('CurrentUserInfo', function () {
						return {
							CurrentUserInfo: function () {
								return { "FirstDayOfWeek": 1, DefaultTimeZone: 'Europe/Berlin' };
							}
						};
					});
				});
			});

			beforeEach(inject(function (_$rootScope_, _$compile_) {
				$compile = _$compile_;
				$rootScope = _$rootScope_;
			}));
			it('should render first day of week correctly', function () {
				var result = setUp('2018-06-12');
				var picker = result.container[0].querySelector('.toggle-datepicker');
				picker.click();

				var currentWeekEl = result.container[0].querySelectorAll('.wfm-datepicker tbody tr')[1];
				var firstDayOfWeekEl = currentWeekEl.querySelectorAll("td span")[0];
				expect(firstDayOfWeekEl.innerHTML).toEqual("04");
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