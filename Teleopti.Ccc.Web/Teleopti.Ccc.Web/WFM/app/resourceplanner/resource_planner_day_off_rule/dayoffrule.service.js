(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.factory('dayOffRuleService', factory);

	factory.$inject = ['$resource'];

	function factory($resource) {
		var dayoffRules = $resource('../api/resourceplanner/dayoffrules/:id', {id:'@id'});

		var service = {
			dayoffRules: dayoffRules
		};

		return service;
	}
})();
