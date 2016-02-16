(function () {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayCtrl', [
		'$scope', '$state', 'intradayService', '$stateParams', '$filter',
		function ($scope, $state, intradayService, $stateParams, $filter) {

			$scope.$on('$stateChangeSuccess', function (evt, to, params, from) {
				if (params.newSkillArea == true) {
					reloadSkillAreas();
				}
			});
			
			$scope.newSkillArea = $stateParams.newSkillArea;

			var reloadSkillAreas = function() {
				intradayService.getSkillAreas.query().
					$promise.then(function (result) {
						$scope.skillAreas = $filter('orderBy')(result, 'Name');
				});
			};

			reloadSkillAreas();

			intradayService.getSkills.query().
				$promise.then(function (result) {
					$scope.skills = result;
			});

			$scope.format = intradayService.formatDateTime;

			$scope.configMode = function () {
				$state.go('intraday.config', { isNewSkillArea: false });
			};

			$scope.modalShown = false;

			$scope.toggleModal = function () {
				$scope.modalShown = !$scope.modalShown;
			};

			$scope.deleteSkillArea = function (skillArea) {
				intradayService.deleteSkillArea.remove(
				{
					 id: skillArea.Id
				})
				.$promise.then(function (result) {
					$scope.skillAreas.splice($scope.skillAreas.indexOf(skillArea), 1);
					$scope.selectedItem = null;
					$scope.isSkillAreaActive = false;
				}, function(error) {
					console.log('error ' + error);
				});

				$scope.toggleModal();
			};

			$scope.querySearch = function(query, myArray) {
				var results = query ? myArray.filter(createFilterFor(query)) : myArray, deferred;
				return results;
			};

			function createFilterFor(query) {
				var lowercaseQuery = angular.lowercase(query);
				return function filterFn(item) {
					var lowercaseName = angular.lowercase(item.Name);
					return (lowercaseName.indexOf(lowercaseQuery) === 0);
				};
			};

			$scope.selectedItemChange = function (item, type) {
				if (type == 'skill' && this.selectedSkill) {
					$scope.selectedItem = item;
					this.selectedSkillArea = null;
					this.searchSkillAreaText = '';
					$scope.isSkillAreaActive = false;
				}
				if (type == 'skillArea' && this.selectedSkillArea) {
					$scope.selectedItem = item;
					this.selectedSkill = null;
					this.searchSkillText = '';
					$scope.isSkillAreaActive = true;
				}
			};
		}
		]);
})()
