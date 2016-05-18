(function () {
	angular.module('wfm.teamSchedule').directive('addPersonalActivity', addPersonalActivity);

	addPersonalActivityCtrl.$inject = ['ActivityService'];

	function addPersonalActivityCtrl(activityService) {
		var vm = this;

		
		activityService.fetchAvailableActivities().then(function (activities) {
			vm.activities = activities;
		});
	}

	function addPersonalActivity() {
		return {
			restrict: 'E',
			scope: {},
			controller: addPersonalActivityCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'js/teamSchedule/html/addPersonalActivity.html',
			require: ['^teamscheduleCommandContainer', 'addPersonalActivity'],
			link: postlink
		}

		function postlink(scope, elem, attrs, ctrls) {
			var containerCtrl = ctrls[0],
				selfCtrl = ctrls[1];

			scope.getDate = containerCtrl.getDate;
		}
	}
})();