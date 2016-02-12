(function () {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayCtrl', [
		'$scope', '$state', 'intradayService', '$stateParams', '$filter',
		function ($scope, $state, intradayService, $stateParams, $filter) {

			$scope.selectedSkillArea = undefined;

			$scope.$on('$stateChangeSuccess', function (evt, to, params, from) {
				if (params.newSkillArea == true) {
					reloadSkillAreas();
				}
			});
			
			$scope.newSkillArea = $stateParams.newSkillArea;

			var reloadSkillAreas = function() {
				intradayService.getSkillAreas.query().$promise.then(function (result) {
					$scope.skillAreas = result;
				});
			};

			reloadSkillAreas();


			intradayService.getSkills.query().$promise.then(function (result) {
				$scope.skills = result;
			});

			$scope.changeSkillArea = function (skillAreaId) {
				//var result = $scope.skillAreas.filter(function( obj ) {
				//	return obj.Id == skillAreaId;
				//});

				//$scope.selectedSkillArea = result[0];
				$scope.selectedSkillArea = $filter('filter')($scope.skillAreas, { Id: skillAreaId })[0];
				console.log($scope.selectedSkillArea.Id);
				console.log($scope.selectedSkillArea.Name);
			};

			$scope.testableHub = '';
			$scope.test = function() {
				$scope.testableHub = $scope.selectedSkillAreaId;
			};

			$scope.format = intradayService.formatDateTime;

			$scope.configMode = function () {
				$state.go('intraday.config', { isNewSkillArea: false });
			};

			$scope.modalShown = false;
			$scope.toggleModal = function () {
				$scope.modalShown = !$scope.modalShown;
			};
		}
		]);
})()
