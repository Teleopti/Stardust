(function() {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayCtrl', [
			'$scope', '$state', 'intradayService', '$stateParams', '$filter',
			function($scope, $state, intradayService, $stateParams, $filter) {

				$scope.$on('$stateChangeSuccess', function(evt, to, params, from) {
					if (params.newSkillArea == true) {
						reloadSkillAreas();
					}
				});

				$scope.newSkillArea = $stateParams.newSkillArea;

				var reloadSkillAreas = function() {
					intradayService.getSkillAreas.query()
						.$promise.then(function (result) {
							$scope.skillAreas = $filter('orderBy')(result.SkillAreas, 'Name');
							$scope.HasPermissionToModifySkillArea = result.HasPermissionToModifySkillArea;
					});
				};

				reloadSkillAreas();

				intradayService.getSkills.query().
					$promise.then(function(result) {
						$scope.skills = result;
					});

				$scope.format = intradayService.formatDateTime;

				$scope.configMode = function() {
					$state.go('intraday.config', { isNewSkillArea: false });
				};

				$scope.modalShown = false;

				$scope.toggleModal = function() {
					$scope.modalShown = !$scope.modalShown;
				};

				$scope.deleteSkillArea = function(skillArea) {
					intradayService.deleteSkillArea.remove(
						{
							id: skillArea.Id
						})
						.$promise.then(function(result) {
							$scope.skillAreas.splice($scope.skillAreas.indexOf(skillArea), 1);
							$scope.selectedItem = null;
							clearSkillAreaSelection();
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

				var skillAreaCtrl, skillCtrl;

				$scope.selectedItemChange = function(item, type) {
					if (type == 'skill' && this.selectedSkill) {
						skillCtrl = this;
						$scope.selectedItem = item;
						clearSkillAreaSelection();
					}
					if (type == 'skillArea' && this.selectedSkillArea) {
						skillAreaCtrl = this;
						$scope.isSkillAreaActive = true;
						$scope.selectedItem = item;
						clearSkillSelection();
					}
				};

				function clearSkillSelection() {
					if (!skillCtrl) return;
					skillCtrl.selectedSkill = null;
					skillCtrl.searchSkillText = '';
				};

				function clearSkillAreaSelection() {
					if (!skillAreaCtrl) return;
					skillAreaCtrl.selectedSkillArea = null;
					skillAreaCtrl.searchSkillAreaText = '';
					$scope.isSkillAreaActive = false;
				};
			}
		]);
})();
