(function () {
    'use strict';

    angular
        .module('wfm.staffing')
        .factory('staffingService', staffingService);

    staffingService.$inject = ['$resource'];
    function staffingService($resource) {
        var monitorskillareastaffingByDate = $resource('../api/staffing/monitorskillareastaffing');
        var monitorskillstaffingByDate = $resource('../api/staffing/monitorskillstaffing');
        var skills = $resource("../api/intraday/skills");
        var areas = $resource("../api/skillgroup/skillgroups");
        var overtime = $resource('../api/staffing/overtime');
        var suggestion = $resource('../api/staffing/overtime/suggestion');
        var compensations = $resource('../api/staffing/GetCompensations');
        var savebpo = $resource('../api/staffing/importBpo');
        var staffingSettings = $resource('../api/staffing/staffingSettings');
        var fileExport = $resource('../api/staffing/exportStaffingDemand');
        ////////////////

        var service = {
            getSkillAreaStaffingByDate: monitorskillareastaffingByDate, //skillAreasByDate
            getSkillStaffingByDate: monitorskillstaffingByDate, //skillsStaffingByDate
            getSkills: skills,
            getSkillAreas: areas,
            addOvertime: overtime,
            getSuggestion: suggestion,
            getCompensations: compensations,
            importbpo: savebpo,
            staffingSettings: staffingSettings,
            postFileExport: fileExport
        };

        return service;

    }
})();
