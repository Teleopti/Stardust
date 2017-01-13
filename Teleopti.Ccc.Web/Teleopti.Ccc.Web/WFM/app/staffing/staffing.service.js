(function () {
    'use strict';

    angular
        .module('wfm.staffing')
        .factory('staffingService', staffingService);

    staffingService.$inject = ['$resource'];
    function staffingService($resource) {
        var monitorskillareastaffing = $resource('../api/intraday/monitorskillareastaffing/:id', { id: '@id' });
        var monitorskillstaffing = $resource('../api/intraday/monitorskillstaffing/:id', { id: '@id' });
        var skills = $resource("../api/intraday/skills");
        ////////////////

        var service = {
            getSkillAreaStaffing: monitorskillareastaffing, //skillAreas
            getSkillStaffing: monitorskillstaffing, //skillsStaffing
            getSkills : skills
        };

        return service;

    }
})();