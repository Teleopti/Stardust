'use strict';
(function() {
	describe("teamschedule week controller tests", function () {
		var $q,
			rootScope,
			controller,
			weekViewCreator,
			fakeStateParams;
		
		beforeEach(function () {
			module("wfm.teamSchedule");
			module(function ($provide) {
				fakeStateParams = setUpFakeStateParams();
				$provide.service('weekViewScheduleSvc', fakeWeekViewScheduleService);
				$provide.service('$stateParams', function () { return fakeStateParams });
			});
		});
		beforeEach(
			inject(function (_$q_, _$rootScope_, _$controller_, _PersonScheduleWeekViewCreator_) {
				$q = _$q_;
				rootScope = _$rootScope_.$new();
				weekViewCreator = _PersonScheduleWeekViewCreator_;
				controller = setUpController(_$controller_);
			}));

		it("should clear group weeks and set person count when selected person larger than 500",function() {
			controller.scheduleDate = new Date("2015-10-26");
			controller.searchOptions = {
				focusingSearch: false
			}
			controller.loadSchedules();
			expect(controller.groupWeeks.length).toEqual(0);
			expect(controller.total).toEqual(800);
		});

		it("should init all settings state as same as the route state", function () {
			expect(controller.searchOptions.keyword).toEqual(fakeStateParams.keyword);
			expect(controller.scheduleDate).toEqual(fakeStateParams.selectedDate);
			expect(controller.teamNameMap).toEqual(fakeStateParams.teamNameMap);
			expect(controller.sortOption).toEqual(fakeStateParams.selectedSortOption);
			if (fakeStateParams.selectedTeamIds) {
				expect(controller.selectedGroups.groupIds).toEqual(fakeStateParams.selectedTeamIds);
			} else {
				expect(controller.selectedGroups.groupPageId).toEqual(fakeStateParams.selectedGroupPage.pageId);
				expect(controller.selectedGroups.groupIds).toEqual(fakeStateParams.selectedGroupPage.groupIds);
			}
			expect(controller.staffingEnabled).toEqual(fakeStateParams.staffingEnabled);
		});

		function setUpController($controller) {
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

		function setUpFakeStateParams() {
			return {
				selectedDate: new Date('2018-02-02'),
				keyword: 'search',
				teamNameMap: '1 team selected',
				selectedTeamIds: [],
				selectedGroupPage: { pageId: '', groupIds: [] },
				staffingEnabled: true
			};
		}

	});
})();