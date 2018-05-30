(function () {
	'use strict';

	angular.module('wfm.teamSchedule').service('ViewStateKeeper', ViewStateKeeper);

	ViewStateKeeper.$inject = [];

	function ViewStateKeeper() {
		var currentState = {};

		this.save = function (state) {
			currentState = state;
		}

		this.get = function () {
			return currentState;
		}
	}

})();