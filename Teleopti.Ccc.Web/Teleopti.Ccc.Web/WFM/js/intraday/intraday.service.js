(function() {
	'use strict';

	var intradayService = angular.module('wfm.intraday');
	intradayService.service('IntradayService', [
		'$resource', function ($resource) {

			this.skillList = $resource('../api/intraday/skillstatus', {}, {
				query: { method: 'GET', params: {}, isArray: true }
			});


			//intradayService.skillList = [
			//	{
			//		"SkillName": "McLaren Honda Support",
			//		"Severity": "5",
			//		"Measures": [
			//	{
			//		"Name": "Calls",
			//		"Value": "3506",
			//		"StringValue": "It is not looking good."
			//	}
			//		]
			//	},
			//	{
			//		"SkillName": "Williams Martini Racing Support",
			//		"Severity": "1",
			//		"Measures": [
			//	{
			//		"Name": "Calls",
			//		"Value": "15",
			//		"StringValue": "It is looking okay."
			//	}
			//		]
			//	}
			//];
		}
	]);
})();