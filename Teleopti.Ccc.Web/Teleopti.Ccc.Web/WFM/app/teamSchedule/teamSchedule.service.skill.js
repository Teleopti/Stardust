(function () {
	'use strict';

	angular.module('wfm.teamSchedule').service('TeamScheduleSkillService', TeamScheduleSkillService);

	TeamScheduleSkillService.$inject = ['$http', '$q'];

	function TeamScheduleSkillService($http, $q) {
		var urlMap = {
			skill: "../api/TeamScheduleData/skills",
			skillGroups: "../api/TeamScheduleData/skillgroups"
		};

		this.getAllSkills = getAllSkills;
		this.getAllSkillGroups = getAllSkillGroups;

		var skills = [];
		var loadedSkills = false;
		function getAllSkills() {
			return $q(function (resolve, reject) {
				if (loadedSkills) {
					resolve(skills);
					return;
				}
				$http.get(urlMap.skill).then(function (response) {
					skills = response.data || [];
					loadedSkills = true;
					resolve(skills);
				},
				function (error) {
					reject(error);
				});
			});
		}

		var skillGroups = [];
		var loadedSkillGroups = false;
		function getAllSkillGroups() {
			return $q(function (resolve, reject) {
				if (loadedSkillGroups) {
					resolve(skillGroups);
					return;
				}
				$http.get(urlMap.skillGroups).then(function (response) {
					skillGroups = response.data || [];
					loadedSkillGroups = true;
					resolve(response.data);
				},
				function (error) {
					reject(error);
				});
			});
		}

	}
})();