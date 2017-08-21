(function () {
	'use strict';

	describe('teamschedule scenario util tests', function () {
		var scenario;

		beforeEach(function () {
			module("wfm.teamSchedule");
		});

		beforeEach(inject(function (ScenarioTestUtil) {
			scenario = ScenarioTestUtil;
		}));


		it('should be able to set to scenarios test state', function () {
			scenario.inScenarioTest();

			expect(scenario.isScenarioTest()).toBeTruthy();
		});
		
	});
})();
