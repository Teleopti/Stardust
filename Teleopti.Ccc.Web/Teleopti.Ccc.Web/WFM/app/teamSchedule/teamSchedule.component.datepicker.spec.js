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
		
		it('should render correctly', function () {
			var result = setUp(moment('2016-06-01').toDate());
			var vm = result.commandControl;

			var dateStringInput = result.container[0].querySelector("#teamschedule-datepicker-input");
			expect(dateStringInput.value).toBe('6/1/16');
		});

		it('should update ngModel value when selected day is changed', function () {
			var result = setUp('2016-06-01');

			var inputEl = result.container[0].querySelector(".teamschedule-datepicker #teamschedule-datepicker-input");
			expect(inputEl.value).toBe('6/1/16');

			inputEl.value = '6/2/16';
			angular.element(inputEl).triggerHandler('change');

			$timeout(function () {
				expect(result.scope.curDate).toBe('2016-06-02');
			}, 300);

			$timeout.flush();
		});

		it('should back to previous date when date is null or invalid', function () {
			var result = setUp(moment('2016-06-01').toDate());

			var inputEl = result.container[0].querySelector(".teamschedule-datepicker #teamschedule-datepicker-input");
			inputEl.value = "";
			angular.element(inputEl).triggerHandler('change');

			$timeout(function () {
				expect(inputEl.value).toBe('6/1/16');
			}, 300);
			$timeout.flush();
		});

		it('should display date as the ngModel when ngModel is changed from outside', function () {
			var result = setUp('2018-10-09');

			result.scope.curDate = '2018-10-10';
			result.scope.$apply();

			$timeout(function () {
			var inputEl = result.container[0].querySelector(".teamschedule-datepicker #teamschedule-datepicker-input");
				expect(inputEl.value).toBe('10/10/18');
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
		var html = '<team-schedule-datepicker ng-model="curDate" ng-change="onScheduleDateChanged()" ></team-schedule-datepicker>';
		var scope = $rootScope.$new();

		if (inputDate == null)
			date = moment('2016-06-15').toDate();
		else
			date = inputDate;

		scope.curDate = date;

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