(function() {

	'use strict';

	angular.module('wfm.outbound')
		.controller('CampaignListGanttCtrl', ['$scope', 'OutboundToggles', campaignListGanttCtrl]);

	function campaignListGanttCtrl($scope, OutboundToggles) {

		$scope.initialized = false;
		$scope.$watch(function() {
			return OutboundToggles.ready;
		}, function(value) {
			if (value) {
				if (OutboundToggles.isGanttEnabled()) init();
			}
		});

		function init() {
			$scope.initialized = true;
		}

		$scope.ganttOptions = {
			headers: ['month', 'day'],
			fromDate: '2015-9-1',
			toDate: '2015-10-31'
		}

		$scope.ganttData = [
		 {
		 	name: "Create concept",
		 	from: '2015-9-9',
		 	to: '2015-10-15',
		 	tasks: [
			  {
			  	name: "Create concept",
			  	content: "<i class=\"fa fa-cog\" ng-click=\"scope.handleTaskIconClick(task.model)\"></i> {{task.model.name}}",
			  	color: "#09F",
			  	from: "2015-9-9",
			  	to: "2015-10-15",
			  	id: "58916e1c-1c2e-3d39-f5d4-8246b604fed41"
			  }
		 	],
		 	id: "81aea986-5ece-977d-942b-c6dc447baaed2"
		 }, {
		 	name: "Create concept234",
		 	from: '2015-9-10',
		 	to: '2015-10-16',
		 	tasks: [
			  {
			  	name: "Create concept2",
			  	content: "<i class=\"fa fa-cog\" ng-click=\"scope.handleTaskIconClick(task.model)\"></i> {{task.model.name}}",
			  	color: "#F1C232",
			  	from: "2015-9-10",
			  	to: "2015-10-1",
			  	id: "58916e1c-1c2e-3d39-f5d4-8246b604fed43"
			  },
			   {
			   	name: "Create concept2",
			   	content: "<i class=\"fa fa-cog\" ng-click=\"scope.handleTaskIconClick(task.model)\"></i> {{task.model.name}}",
			   	color: "#F1C232",
			   	from: "2015-10-10",
			   	to: "2015-10-16",
			   	id: "58916e1c-1c2e-3d39-f5d4-8246b604fed431"
			   }
		 	],
		 	id: "81aea986-5ece-977d-942b-c6dc4472baaed4"
		 }
		];
	}

})();