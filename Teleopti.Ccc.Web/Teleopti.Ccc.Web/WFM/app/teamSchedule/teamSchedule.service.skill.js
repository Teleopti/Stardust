(function () {
	'use strict';

	angular.module('wfm.teamSchedule').service('TeamScheduleSkillService', TeamScheduleSkillService);

	TeamScheduleSkillService.$inject = ['$http', '$q'];

	function TeamScheduleSkillService($http, $q) {
		var urlMap = {
			skill: "../api/GroupPage/AvailableStructuredGroupPages",
			skillGroups: "../api/GroupPage/AvailableStructuredGroupPagesForRequests"
		};

		this.getAllSkills = getAllSkills;
		this.getAllSkillGroups = getAllSkillGroups;

		 var mockSkills = [
			{
				Id: 'XYZ',
				Name: 'skill1'
			},
			{
				Id: 'ABC',
				Name: 'skill2'
			}
		];
		var mockedSkillGroups = [
			{
				Name: 'SkillArea1',
				Id: '123',
				Skills: [
					{
						Id: 'XYZ',
						Name: 'skill1'
					}
				]
			},
			{
				Name: 'SkillArea2',
				Id: '321',
				Skills: [
					{
						Id: 'ABC',
						Name: 'skill2'
					}
				]
			}
		];

		function getAllSkills() {
			return mockSkills;
			//return $q(function (resolve, reject) {
			//	$http.get(urlMap.skill, {}).then(function () {
			//		resolve(response.data);
			//	})
			//});
		}

		function getAllSkillGroups() {
			return mockedSkillGroups;
			//return $q(function (resolve, reject) {
			//	$http.get(urlMap.skillGroups, {}).then(function () {
			//		resolve(response.data);
			//	})
			//});
		}

	}
})()