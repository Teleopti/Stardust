'use strict';

angular.module('wfm.seatPlan', [])
	.controller('SeatPlanCtrl', ['$scope', '$state',
	function ($scope, $state) {

		var startDate = moment().add(1, 'months').startOf('month').toDate();
		var endDate = moment().add(2, 'months').startOf('month').toDate();
		$scope.period = { startDate: startDate, endDate: endDate };
		$scope.Locations = [
		{
			"id": 1,
			"name": "Beijing",
			"children": []
		},
		{
			"id": 2,
			"name": "Shenzhen",
			"children": [
			{
				"id": 21,
				"name": "Futian",
				"selected": true,
				"children": [
					{
						"id": 211,
						"name": "Robs Area",
						"children": []
					},
					{
						"id": 212,
						"name": "Zhipings Area",
						"children": []
					}
				]
			},
					{
						"id": 22,
						"name": "Longhua",
						"children": []
					}
			]
		},
			{
				"id": 3,
				"name": "Chongqing",
				"children": []
			},
			{
				"id": 4,
				"name": "Chengdu",
				"children": []
			}
		];

		$scope.selectLocationNode = function (node) {
			node.selected = !node.selected;
		}

	}]
);