(function() {
    'use strict';
    angular.module('wfm.intraday').controller('IntradayConfigController', [
        '$scope',
        '$state',
        'intradayService',
        '$filter',
        'NoticeService',
        '$translate',
		'Toggle',
        'skillIconService',
        function($scope, $state, intradayService, $filter, NoticeService, $translate, toggleSvc, skillIconService) {
            $scope.skills = [];
            $scope.skillAreaName = '';
            $scope.getSkillIcon = skillIconService.get;
			$scope.toggles = {
				unifiedSkillGroupManagement: []
			};
			toggleSvc.togglesLoaded.then(function () {
				$scope.toggles.unifiedSkillGroupManagement = toggleSvc.WFM_Unified_Skill_Group_Management_45417;
			});
            $scope.exitConfigMode = function() {
                $state.go('intraday', {isNewSkillArea: false});
            };

            intradayService.getSkills.query().$promise.then(function(skills) {
                $scope.skills = skills;
            });

            $scope.skillSelected = function() {
                var selectedSkills = $filter('filter')($scope.skills, {isSelected: true});
                var selectedSkillIds = selectedSkills.map(function(skill) {
                    return skill.Id;
                });
                return selectedSkillIds.length > 0;
            };

            $scope.saveSkillArea = function(form) {
                if (form.$invalid) {
                    return;
                }
                var selectedSkills = $filter('filter')($scope.skills, {isSelected: true});

                var selectedSkillIds = selectedSkills.map(function(skill) {
                    return skill.Id;
                });

                if (selectedSkillIds.length <= 0) {
                    NoticeService.error($translate.instant('SkillAreaNoSkillSelected'), 5000, false);
                    return;
                }

                intradayService.createSkillArea
                    .query({
                        Name: $scope.skillAreaName,
                        Skills: selectedSkillIds
                    })
                    .$promise.then(function(result) {
                        notifySkillAreaCreation();
                        $state.go('intraday', {isNewSkillArea: true});
                    });
            };

            var notifySkillAreaCreation = function() {
                NoticeService.success($translate.instant('Created') + ' ' + $scope.skillAreaName, 5000, false);
            };
        }
    ]);
})();
