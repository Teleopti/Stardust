(function () {
	'use strict';

	angular.module('wfm.teamSchedule').service('TeamsStaffingConfigurationStorageService', staffingConfigStorageService);

	function staffingConfigStorageService() {
		var key = 'teams_staffing_config_data';

		this.getConfig = getData;
		this.setSkill = setSkill;
		this.setSize = setSize;
		this.clearConfig = clearConfig;


		function setSize(tableHeight, tableBodyHeight, chartHeight) {
			var data = getData() || {};
			data.tableHeight = tableHeight;
			data.tableBodyHeight = tableBodyHeight;
			data.chartHeight = chartHeight;
			localStorage.setItem(key, angular.toJson(data));
		}

		function setSkill(skillId, skillGroupId) {
			var data = getData() || {};
			data.skillId = skillId;
			data.skillGroupId = skillGroupId;
			localStorage.setItem(key, angular.toJson(data));
		}

		function clearConfig() {
			localStorage.removeItem(key);
		}

		function getData() {
			var data = localStorage.getItem(key);
			if (data) {
				return angular.fromJson(data);
			}
		}
	}
})();