(function () {
	'use strict';

	angular.module('wfm.teamSchedule').service('TeamScheduleSkillService', TeamScheduleSkillService);

	TeamScheduleSkillService.$inject = ['$http', '$q'];

	function TeamScheduleSkillService($http, $q) {
		var urlMap = {
			skill: "../api/TeamScheduleData/skills",
			skillGroups: "../api/skillgroup/skillgroups"
		};

		this.getAllSkills = getAllSkills;
		this.getAllSkillGroups = getAllSkillGroups;

		function getAllSkills() {
			return $q(function (resolve, reject) {
				$http.get(urlMap.skill).then(function(response) {
						resolve(response.data);
					},
					function(error) {
						reject(error);
					});
			});
		}

		function getAllSkillGroups() {
			return $q(function (resolve, reject) {
				$http.get(urlMap.skillGroups).then(function(response) {
						resolve(response.data);
					},
					function(error) {
						reject(error);
					});
			});
		}

	}
})()