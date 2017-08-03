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
        var areas = $resource("../api/intraday/skillarea");
        var overtime = $resource('../api/staffing/overtime');
        var suggestion = $resource('../api/staffing/overtime/suggestion');
        var compensations = $resource('../api/staffing/GetCompensations');
        var savebpo = $resource('../api/staffing/importBpo');
        var fileImportLicense = $resource('../api/staffing/GetLicense');
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
            fileImportLicense: fileImportLicense
        };

        return service;

    }
})();