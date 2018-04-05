(function() {
	'use strict';
	angular.module('wfm.intraday').service('intradaySkillService', ['$resource', timeService]);

	function timeService($resource) {
		var skillItem;

		var setSkill = function(item) {
			skillItem = item;
		};

		var getSkill = function() {
			return skillItem;
		};

		return {
			setSkill: setSkill,
			getSkill: getSkill
		};
	}
})();
