(function() {
    'use strict';
    angular.module('wfm.intraday').controller('IntradayCtrl', ic);
	
	ic.$inject = ['$scope'];
	
    function ic($scope) {
        //remember to add dashboard state without url in app.js
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
    }
})();
