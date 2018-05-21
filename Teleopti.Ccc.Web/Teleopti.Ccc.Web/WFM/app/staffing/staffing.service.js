(function () {
  'use strict';

  angular
  .module('wfm.staffing')
  .factory('staffingService', staffingService);

  staffingService.$inject = ['$resource'];
  function staffingService($resource) {
	var monitorskillareastaffingByDate = $resource('../api/staffing/monitorskillareastaffing');
	var monitorskillstaffingByDate = $resource('../api/staffing/monitorskillstaffing');
	  var skills = $resource("../api/staffing/skills");
	  var areas = $resource("../api/skillgroup/skillgroups");
	var overtime = $resource('../api/staffing/overtime');
	var suggestion = $resource('../api/staffing/overtime/suggestion');
	var compensations = $resource('../api/staffing/GetCompensations');
	var savebpo = $resource('../api/staffing/importBpo');
	var staffingSettings = $resource('../api/staffing/staffingSettings');
	var fileExport = $resource('../api/staffing/exportStaffingDemand');
	var exportStaffingPeriodMessage = $resource('../api/staffing/exportStaffingPeriodMessage');
	var exportGapPeriodMessage = $resource('../api/staffing/exportGapPeriodMessage');
	var staffingDataExport = $resource('../api/staffing/exportforecastandstaffing');

	var staffingGanttDataForBpoTimeline = $resource('../api/staffing/GetAllGanttDataForBpoTimeline');
	var staffingGanttDataForOneSkill = $resource('../api/staffing/GetGanttDataForBpoTimelineOnSkill');
	var staffingGanttDataForOneSkillGroup = $resource('../api/staffing/GetGanttDataForBpoTimelineOnSkillGroup');

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
	  postFileExport: fileExport,
	  getExportStaffingPeriodMessage: exportStaffingPeriodMessage,
	  getExportGapPeriodMessage: exportGapPeriodMessage,
	  exportStaffingData: staffingDataExport,
	  getGanttData: staffingGanttDataForBpoTimeline,
	  getGanttDataForOneSkill: staffingGanttDataForOneSkill,
	  getGanttDataForOneSkillGroup: staffingGanttDataForOneSkillGroup,


	};

	return service;

  }
})();
