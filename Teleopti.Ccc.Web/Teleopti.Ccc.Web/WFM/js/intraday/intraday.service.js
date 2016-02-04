(function() {
	'use strict';
	angular.module('wfm.intraday')
		.service('intradayService', [
			'$resource', function ($resource) {
				this.skillList = $resource('../api/intraday/skillstatus', {}, {
					query: { method: 'GET', params: {}, isArray: true }
				});

				this.getSkills = function (data) {
					return $resource('../api/intraday/skills', {}, {
						query: {
							method: 'GET',
							params: {},
							isArray: true
						}
					}).query().$promise;
				};

				this.formatDateTime = function (time) {
					if (time === null || time === undefined || time === '') return '--:--:--';
					var momentTime = moment(time);
					if (momentTime.format("YYYY") > moment("1970").format("YYYY")) {
						return momentTime.format('HH:mm:ss');
					} else {
						return '--:--:--';
					}
				};
			}
	]);
})();