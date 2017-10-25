'use strict';
(function() {
	describe("teamschedule week controller tests", function () {
		var $q,
			rootScope,
			controller,
			weekViewCreator;
		
		beforeEach(function () {
			module("wfm.teamSchedule");
			module(function ($provide) {
				$provide.service('weekViewScheduleSvc', fakeWeekViewScheduleService);
			});
		});
		beforeEach(
			inject(function (_$q_, _$rootScope_, _$controller_, _PersonScheduleWeekViewCreator_) {
				$q = _$q_;
				rootScope = _$rootScope_.$new();
				weekViewCreator = _PersonScheduleWeekViewCreator_;
				controller = setUpController(_$controller_);
			}));

		function setUpController($controller) {
			return $controller("TeamScheduleWeeklyController", {
				$scope: rootScope,
				WeekViewCreator: weekViewCreator
			});
		};

		function fakeWeekViewScheduleService() {
			this.getSchedules = function() {
				var scheduleData = { "Schedules": [], "Total": 800, "Keyword": "" };
				var response = { data: scheduleData };
				return {
					then: function(callback) {
						callback(response.data);
					}
				}
			}
		}

		it("should clear group weeks and set person count when selected person larger than 500",function() {
			controller.scheduleDate = new Date("2015-10-26");
			controller.searchOptions = {
				focusingSearch: false
			}
			controller.loadSchedules();
			expect(controller.groupWeeks.length).toEqual(0);
			expect(controller.total).toEqual(800);
		});

	});
})();