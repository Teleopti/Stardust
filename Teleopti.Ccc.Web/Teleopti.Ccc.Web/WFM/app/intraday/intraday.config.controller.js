(function() {
    'use strict';
    angular.module('wfm.intraday').controller('IntradayConfigCtrl', [
        '$scope',
        '$state',
        'intradayService',
        '$filter',
        'NoticeService',
        '$translate',
        function($scope, $state, intradayService, $filter, NoticeService, $translate) {
            $scope.skills = [];
            $scope.skillAreaName = '';

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

            //TODO: #44026 Add more skilltype icons here
            $scope.setDynamicIcon = function(skill) {
                if (!skill.DoDisplayData) {
                    return 'mdi mdi-alert';
                }

                if (skill.IsMultisiteSkill) {
                    return 'mdi mdi-hexagon-multiple';
                }
                if (skill.SkillType === 'SkillTypeChat') {
                    return 'mdi mdi-message-text-outline';
                } else if (skill.SkillType === 'SkillTypeEmail') {
                    return 'mdi mdi-email-outline';
                } else if (skill.SkillType === 'SkillTypeEmail') {
                    return 'mdi mdi-email-outline';
                } else if (skill.SkillType === 'SkillTypeInboundTelephony') {
                    return 'mdi mdi-phone';
                } else if (skill.SkillType === 'SkillTypeRetail') {
                    return 'mdi mdi-credit-card';
                } else if (skill.SkillType === 'SkillTypeBackoffice') {
                    return 'mdi mdi-archive';
                } else if (skill.SkillType === 'SkillTypeProject') {
                    return 'mdi mdi-clock-fast';
                } else if (skill.SkillType === 'SkillTypeFax') {
                    return 'mdi mdi-fax';
                } else if (skill.SkillType === 'SkillTypeTime') {
                    return 'mdi mdi-clock';
                }
            };

            var notifySkillAreaCreation = function() {
                NoticeService.success($translate.instant('Created') + ' ' + $scope.skillAreaName, 5000, false);
            };
        }
    ]);
})();
