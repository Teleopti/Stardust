(function() {
	describe('exportSchedule controller tests',
		function() {

			var $componentController;
			beforeEach(function () {
				module("wfm.teamSchedule");
				module(function ($provide) {
					$provide.service('exportScheduleService', function() {
						return new fakeExportScheduleService();
					});
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


		});

})();