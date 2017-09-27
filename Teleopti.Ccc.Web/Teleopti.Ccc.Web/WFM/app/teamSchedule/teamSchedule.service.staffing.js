(function () {
	'use strict';
	angular.module("wfm.teamSchedule").service("StaffingInfoService", StaffingInfoService);

	StaffingInfoService.$inject = ['$q', '$http'];

	function StaffingInfoService($q, $http) {
		var self = this;
		var urlMap = {
			'skill': '../api/staffing/monitorskillstaffing',
			'skillGroup': '../api/staffing/monitorskillareastaffing'
		}

		self.getStaffingByDate = getStaffingByDate;

		function getStaffingByDate(skill, area,  date, useShrinkage) {
			if (!skill && !area)
				throw 'invalid argument exception.';
			var urlKey = skill ? 'skill' : 'skillGroup';
			var input = {
				DateTime: date,
				UseShrinkage: useShrinkage
			};
			switch (urlKey) {
				case 'skill':
					input.SkillId = skill.Id;
					break;
				case 'skillGroup': 
					input.SkillAreaId = area.Id;
					break;
			}
			return $q(function (resolve, reject) {
				$http.get(urlMap[urlKey], {
					params: input
				}).then(function (response) {
					resolve(response.data);
				}, function (err) {
					reject(err);
				});
			});
		}
	}
})();