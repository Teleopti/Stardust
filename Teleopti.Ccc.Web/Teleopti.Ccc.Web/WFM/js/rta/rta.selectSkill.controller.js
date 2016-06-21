(function () {
	'use strict';
	angular.module('wfm.rta')
		.controller('RtaSelectSkillCtrl', [
			'$scope', '$state', 'RtaService', '$stateParams', '$interval', 'RtaOrganizationService', 'RtaFormatService', 'NoticeService', 'Toggle', '$translate',
			function ($scope, $state, RtaService) {
				$scope.skills = [];

				RtaService.getSkills()
					.then(function(skills) {
						$scope.skills = skills;
					});

				$scope.querySearch = function (query, myArray) {
					var results = query ? myArray.filter(createFilterFor(query)) : myArray;
					return results;
				};

				function createFilterFor(query) {
					var lowercaseQuery = angular.lowercase(query);
					return function filterFn(item) {
						var lowercaseName = angular.lowercase(item.Name);
						return (lowercaseName.indexOf(lowercaseQuery) === 0);
					};
				};

				$scope.selectedSkillChange = function(item) {
					if (item) {
						$state.go('rta.agents-skill', { skillIds: [item.Id] });
					};
				}
			}
		]);
})();
