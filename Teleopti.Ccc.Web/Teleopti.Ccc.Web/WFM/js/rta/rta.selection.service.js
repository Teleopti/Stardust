(function () {
	'use strict';

	angular.module('wfm.rta').service('RtaSelectionService', ['$state',
		function ($state) {

			this.toggleSelection = function (id, selectedItemIds) {
				var index = selectedItemIds.indexOf(id);
				if (index > -1) {
					selectedItemIds.splice(index, 1);
				} else {
					selectedItemIds.push(id);
				}
				return selectedItemIds;
			};

			this.openSelection = function(selectedItemIds, state) {
				if (selectedItemIds.length === 0) return;
				$state.go(state,
				{
					ids: selectedItemIds
				});
			}

			//$scope.openSelectedTeams = function () {
			//	if ($scope.selectedTeamIds.length === 0) return;
			//	$state.go('rta.agents-teams', {
			//		teamIds: $scope.selectedTeamIds
			//	});
			//};
		}
	]);
})();
