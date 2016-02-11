(function () {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayCtrl', [
		'$scope', '$state', 'intradayService', '$stateParams',
		function ($scope, $state, intradayService, $stateParams) {

			$scope.SkillAreaId = $stateParams.skillAreaId;
			intradayService.skillList.query().$promise.then(function (result) {
				$scope.skillList = result;
				$scope.selectedSkill = $scope.skillList[0];
				$scope.skillChange($scope.selectedSkill);
			});

			$scope.format = intradayService.formatDateTime;

			$scope.skillChange = function (skill) {
				$scope.selectedSkill = skill;
			}

			$scope.configMode = function () {
				$state.go('intraday.config', {});
			};

			$scope.modalShown = false;
			$scope.toggleModal = function() {
				$scope.modalShown = !$scope.modalShown;
			};

			$scope.demos = [
				{id: 'Skill name1'},
				{id: 'Skill name2'},
				{id: 'Skill name3'},
				{id: 'Skill name4'},
				{id: 'Skill name5'},
				{id: 'Skill name6'},
				{id: 'Skill name7'},
				{id: 'Skill name8'},
				{id: 'Skill name9'},
				{id: 'Skill name10'},
				{id: 'Skill name11'},
				{id: 'Skill name12'},
				{id: 'Skill name13'},
				{id: 'Skill name14'},
				{id: 'Skill name15'},
				{id: 'Skill name16'},
				{id: 'Skill name17'},
				{id: 'Skill name18'},
				{id: 'Skill name19'},
				{id: 'Skill name20'},
				{id: 'Skill name21'},
				{id: 'Skill name22'},
				{id: 'Skill name23'},
				{id: 'Skill name24'},
				{id: 'Skill name25'},
				{id: 'Skill name26'},
				{id: 'Skill name27'}
			];

			$scope.demos2 = [
				{id: 'This area'},
				{id: 'Another area'},
				{id: 'My area'},
				{id: 'Your area'}
			];

			$scope.reload = setInterval(function () {
				intradayService.skillList.query().$promise.then(function (result) {
					result.forEach(function (resultItem) {
						$scope.skillList.forEach(function (skillItem) {
							if (resultItem.SkillName == skillItem.SkillName) {
								skillItem.Severity = resultItem.Severity;
								skillItem.Measures = resultItem.Measures;
								skillItem.LatestDate = resultItem.LatestDate;
							}
						});
					});
				});
			}, 60000);
		}
	]);
})()
