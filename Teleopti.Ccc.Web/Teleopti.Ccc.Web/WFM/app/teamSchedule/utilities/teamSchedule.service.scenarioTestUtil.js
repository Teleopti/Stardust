(function() {
	"use strict";

	angular.module("wfm.teamSchedule").service("ScenarioTestUtil", function() {
		var scenario = this;

		var inTest = false;

		scenario.isScenarioTest = function() {
			return inTest;
		}

		scenario.inScenarioTest = function() {
			inTest = true;
		}
	});
})();