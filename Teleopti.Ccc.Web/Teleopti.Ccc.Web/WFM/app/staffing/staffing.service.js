(function () {
    'use strict';

    angular
        .module('wfm.staffing')
        .factory('staffingService', staffingService);

    staffingService.$inject = ['$resource'];
    function staffingService($resource) {
        var monitorskillareastaffingByDate = $resource('../api/intraday/monitorskillareastaffing');
        var monitorskillstaffingByDate = $resource('../api/intraday/monitorskillstaffing');
        var skills = $resource("../api/intraday/skills");
        var areas = $resource("../api/intraday/skillarea");
        var overtime = $resource('../api/staffing/overtime');
        var suggestion = $resource('../api/staffing/overtime/suggestion');
        var resCalc = $resource('../TriggerResourceCalculate');
        ////////////////

        var service = {
            getSkillAreaStaffingByDate: monitorskillareastaffingByDate, //skillAreasByDate
            getSkillStaffingByDate: monitorskillstaffingByDate, //skillsStaffingByDate
            getSkills: skills,
            getSkillAreas: areas,
            addOvertime: overtime,
            getSuggestion: suggestion,
            triggerResourceCalculate: resCalc
        };

        return service;

    }
})();