(function() {
    'use strict';
    angular.module('wfm.utilities').factory('skillIconService', [
        function() {
            var get = function(skill) {
                if (!skill.DoDisplayData) {
                    return 'mdi mdi-alert';
                }

                if (skill.IsMultisiteSkill) {
                    return 'mdi mdi-hexagon-multiple';
                }

                switch (skill.SkillType) {
                    case 'SkillTypeChat':
                        return 'mdi mdi-message-text-outline';
                    case 'SkillTypeEmail':
                        return 'mdi mdi-email-outline';
                    case 'SkillTypeInboundTelephony':
                        return 'mdi mdi-phone';
                    case 'SkillTypeRetail':
                        return 'mdi mdi-credit-card';
                    case 'SkillTypeBackoffice':
                        return 'mdi mdi-archive';
                    case 'SkillTypeProject':
                        return 'mdi mdi-clock-fast';
                    case 'SkillTypeFax':
                        return 'mdi mdi-fax';
                    case 'SkillTypeTime':
                        return 'mdi mdi-clock';
                    default:
                        return 'mdi mdi-alert';
                }
            };

            return {
                get: get
            };
        }
    ]);
})();
