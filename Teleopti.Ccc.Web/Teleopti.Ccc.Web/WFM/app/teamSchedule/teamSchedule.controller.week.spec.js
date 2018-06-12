'use strict';
(function () {
	var $q,
		rootScope,
		$controller,
		weekViewCreator,
		viewStateKeeper;
	describe("teamschedule week controller tests", function () {
		beforeEach(function () {
			module("wfm.teamSchedule");
			module(function ($provide) {
				$provide.service('weekViewScheduleSvc', fakeWeekViewScheduleService);
				$provide.service('CurrentUserInfo', function () {
					return {
						CurrentUserInfo: function () {
							return { FirstDayOfWeek: 1 };
						}
					};
				});
			});
		});
		beforeEach(function () { moment.locale('sv'); });
		afterEach(function () { moment.locale('en'); });
		beforeEach(inject(function (_$q_, _$rootScope_, _$controller_, _PersonScheduleWeekViewCreator_, _ViewStateKeeper_) {
			$q = _$q_;
			rootScope = _$rootScope_.$new();
			weekViewCreator = _PersonScheduleWeekViewCreator_;
			viewStateKeeper = _ViewStateKeeper_;
			$controller = _$controller_;
		}));

		it("should clear group weeks and set person count when selected person larger than 500", function () {
			var controller = setUpController();
			controller.scheduleDate = new Date("2015-10-26");
			controller.searchOptions = {
				focusingSearch: false
			}
			controller.loadSchedules();
			expect(controller.groupWeeks.length).toEqual(0);
			expect(controller.total).toEqual(800);
		});

		it("should init all settings", function () {
			var state = {
				selectedDate: '2018-06-12',
				keyword: 'search',
				teamNameMap: '1 team selected',
				selectedSortOption: 'Name DESC',
				selectedTeamIds: [],
				selectedGroupPage: { pageId: '', groupIds: [] },
				staffingEnabled: true,
				timezone: 'Europe/Berlin'
			};
			viewStateKeeper.save(state);

			var controller = setUpController();

			expect(controller.searchOptions.keyword).toEqual('search');
			expect(controller.scheduleDate).toEqual('2018-06-12');
			expect(controller.teamNameMap).toEqual('1 team selected');
			expect(controller.sortOption).toEqual('Name DESC');

			expect(controller.selectedGroups.groupPageId).toEqual('');
			expect(controller.selectedGroups.groupIds).toEqual([]);
			expect(controller.staffingEnabled).toEqual(true);
			expect(controller.timezone).toEqual("Europe/Berlin");

			expect(controller.startOfWeek).toEqual("2018-06-11");
		});

		it('should get correct week days when start of week is changed', function () {
			var controller = setUpController();
			controller.startOfWeek = '2018-05-09';
			controller.onStartOfWeekChanged();

			expect(controller.startOfWeek).toEqual('2018-05-07');
			expect(moment(controller.weekDays[0].date).format('YYYY-MM-DD')).toEqual('2018-05-07');
			expect(moment(controller.weekDays[6].date).format('YYYY-MM-DD')).toEqual('2018-05-13');
		});

	});
	describe('#teamschedule week controller tests in ar-OM #', function () {
		beforeEach(function () {
			module("wfm.teamSchedule");
			module(function ($provide) {
				$provide.service('weekViewScheduleSvc', fakeWeekViewScheduleService);
				$provide.service('CurrentUserInfo', function () {
					return {
						CurrentUserInfo: function () {
							return { FirstDayOfWeek: 0 };
						}
					};
				});
			});
		});
		beforeEach(function () { moment.locale('ar-OM'); });
		afterEach(function () { moment.locale('en'); });
		beforeEach(inject(function (_$q_, _$rootScope_, _$controller_, _PersonScheduleWeekViewCreator_, _ViewStateKeeper_) {
			$q = _$q_;
			rootScope = _$rootScope_.$new();
			weekViewCreator = _PersonScheduleWeekViewCreator_;
			viewStateKeeper = _ViewStateKeeper_;
			$controller = _$controller_;
		}));

		it('should init startOfWeek correctly', function () {
			var state = {
				selectedDate: '2018-06-12'
			};
			viewStateKeeper.save(state);

			var controller = setUpController();

			expect(controller.startOfWeek).toEqual("2018-06-10");
		});
	});

	function setUpController() {
		return $controller("TeamScheduleWeeklyController", {
			$scope: rootScope,
			WeekViewCreator: weekViewCreator
		});
	};

	function fakeWeekViewScheduleService() {
		this.getSchedules = function () {
			var scheduleData = { "Schedules": [], "Total": 800, "Keyword": "" };
			var response = { data: scheduleData };
			return {
				then: function (callback) {
					callback(response.data);
				}
			}
		}
	}
})();