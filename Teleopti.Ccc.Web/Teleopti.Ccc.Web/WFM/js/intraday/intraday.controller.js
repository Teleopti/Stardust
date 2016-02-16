(function () {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayCtrl', [
		'$scope', '$state', 'intradayService', '$stateParams', '$filter',
		function ($scope, $state, intradayService, $stateParams, $filter) {

			$scope.selectedSkillArea = undefined;
			$scope.selectedSkill = undefined;

			$scope.$on('$stateChangeSuccess', function (evt, to, params, from) {
				if (params.newSkillArea == true) {
					reloadSkillAreas();
				}
			});
			
			$scope.newSkillArea = $stateParams.newSkillArea;

			var reloadSkillAreas = function() {
				intradayService.getSkillAreas.query().$promise.then(function (result) {
					$scope.skillAreas = $filter('orderBy')(result, 'Name');
					if ($scope.skillAreas.length > 0)
						$scope.selectedSkillArea = $scope.skillAreas[0];
				});
			};

			reloadSkillAreas();


			intradayService.getSkills.query().$promise.then(function (result) {
				$scope.skills = result;
				$scope.selectedSkill = $scope.skills[0];
			});

			$scope.format = intradayService.formatDateTime;

			$scope.configMode = function () {
				$state.go('intraday.config', { isNewSkillArea: false });
			};

			$scope.modalShown = false;

			$scope.toggleModal = function () {
				$scope.modalShown = !$scope.modalShown;
			};

			$scope.deleteSkillArea = function () {
				intradayService.deleteSkillArea.remove(
				{
					 id: $scope.selectedSkillArea.Id
				}).$promise.then(function (result) {
					$scope.skillAreas.splice($scope.skillAreas.indexOf($scope.selectedSkillArea), 1);
					$scope.selectedSkillArea = undefined;
				}, function(error) {
					console.log('error ' + error);
				});

				$scope.toggleModal();
			};
			
			$scope.querySearch = function (query, myArray) {
				var results = query ? myArray.filter(createFilterFor(query)) : myArray, deferred;
				return results;
			}

			function createFilterFor(query) {
				var lowercaseQuery = angular.lowercase(query);
				return function filterFn(item) {
					console.log('"'+item.Name + '" ' + query);
					console.log(item.Name.indexOf(lowercaseQuery));
					var lowercaseName = angular.lowercase(item.Name);
					return (lowercaseName.indexOf(lowercaseQuery) === 0);
				};
			}
		}
		]);
})()
