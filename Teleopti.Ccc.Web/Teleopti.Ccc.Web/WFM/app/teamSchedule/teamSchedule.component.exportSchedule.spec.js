(function() {
	describe('exportSchedule controller tests',
		function() {
			var $componentController, groupPageService;
			beforeEach(function () {
				module("wfm.teamSchedule");
				module(function ($provide) {
					$provide.service('exportScheduleService', function() {
						return new fakeExportScheduleService();
					});
					$provide.service('groupPageService', function () {
						groupPageService = new fakeGroupPageService();
						return groupPageService;
					})
				});
			});
			
			beforeEach(inject(function (_$componentController_) {
				$componentController = _$componentController_;
			}));

			it('can select three optional columns for most',
				function() {
					var ctrl = $componentController('teamsExportSchedule', null, {});
					ctrl.configuration.optionalColumnIds = ['opt1', 'opt2', 'opt3'];

					expect(ctrl.isOptionalColDisabled('opt4')).toEqual(true);
					expect(ctrl.isOptionalColDisabled('opt1')).toEqual(false);
				});

			it('should not load available groups with incorrect dates',
				function () {
					var ctrl = $componentController('teamsExportSchedule', null, {});
					ctrl.configuration.period = null;
					ctrl.onPeriodChanged();
					var currentInputPeriod = groupPageService.currentPeriod();
					expect(currentInputPeriod).toEqual(undefined);
				});

			function commonTestsInDifferentLocale() {
				it('should get available groups with correct dates',
					function () {
						var ctrl = $componentController('teamsExportSchedule', null, {});
						ctrl.configuration.period = { startDate: '2017-01-01', endDate: '2017-01-10' };
						ctrl.onPeriodChanged();
						var currentInputPeriod = groupPageService.currentPeriod();
						expect(currentInputPeriod.startDate).toEqual('2017-01-01');
						expect(currentInputPeriod.endDate).toEqual('2017-01-10');
					});
			}

			commonTestsInDifferentLocale();

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
				beforeAll(function () {
					moment.locale('fa-IR');
				});

				afterAll(function () {
					moment.locale('en');
				});

				commonTestsInDifferentLocale();
			});

			function fakeExportScheduleService() {
				this.getOptionalColumnsData = function() {
					var data = [
						{
							Id: "opt1",
							Name: "opt1"
						},
						{
							Id: "opt2",
							Name: "opt2"
						},
						{
							Id: "opt3",
							Name: "opt3"
						},
						{
							Id: "opt4",
							Name: "opt4"
						}
					];
					return {
						then: function(callback) {
							callback(data);
						}
					}
				};
			}
			function fakeGroupPageService() {
				var currentPeriod;
				this.currentPeriod = function () {
					return currentPeriod;
				}
				this.fetchAvailableGroupPages = function (startDate, endDate) {
					currentPeriod = {startDate:startDate, endDate:endDate};
					return {
						then:function (cb) {
					}
					}
				}
			}
		});
})();