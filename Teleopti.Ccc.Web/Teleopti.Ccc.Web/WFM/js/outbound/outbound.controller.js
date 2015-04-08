var outbound = angular.module('wfm.outbound', ['outboundService']);

outbound.controller('OutboundListCtrl', [
	'$scope', '$state', 'OutboundService',
	function($scope, $state, OutboundService) {
		
		$scope.newName = "";
		$scope.selectedTarget = null;
		$scope.hideDetail = false;

		$scope.reset = function () {			
			$scope.newName = "";
			$scope.form.$setPristine();
		};
		

		$scope.campaigns = OutboundService.listCampaign();

		$scope.create = function () {
			var newCampaign = OutboundService.addCampaign({ name: $scope.newName });
			$scope.campaigns.unshift(newCampaign);
			$scope.selectedTarget = newCampaign;			
		};

		$scope.copyNew = function (campaign) {
			$scope.campaigns.unshift(OutboundService.addCampaign({ name: campaign.name + "_Copy" }));
		};

		$scope.update = function(campaign) {
			OutboundService.updateCampaign(campaign);
		};

		$scope.show = function (campaign) {			
			if (angular.isDefined(campaign)) $scope.selectedTarget = campaign;
			$state.go('outbound.edit', { id: $scope.selectedTarget.id });
			$scope.hideDetail = false;
		};

		$scope.delete = function (campaign, idx) {		
			if (confirm('Are you sure you want to delete this record?')) {
				if (OutboundService.deleteCampaign(campaign, idx)) {
					$scope.campaigns.splice(idx, 1);
				}
			}
		};
	}
]);

outbound.controller('OutboundEditCtrl', [
	'$scope', '$stateParams', '$state', 'OutboundService', 
	function ($scope, $stateParams, $state, OutboundService) {
	
		$scope.acToggle1 = true;

		$scope.skills = [
			{ label: "Phone", value: "Phone" },
			{ label: "Consultancy", value: "Consultancy" },
			{ label: "Writing", value: "Writing" }
		];

		$scope.workinghours = [];

		$scope.toggleWorkinghoursInWeekday = function (wd) {
			// TBD				
		};

		$scope.$on("outbound.campaigns.updated", function() {
			$scope.campaign = OutboundService.getCampaignById($stateParams.id);
			$scope.showDetail = angular.isDefined($scope.campaign);
		});

		$scope.campaign = OutboundService.getCampaignById($stateParams.id);

		$scope.showDetail = angular.isDefined($scope.campaign);

		$scope.period = {
			startDate: moment().add(1, 'months').startOf('month').toDate(),
			endDate: moment().add(2, 'months').startOf('month').toDate()
		};

		$scope.params = {
			skill: $scope.skills[0]
		};

		$scope.update = function () {		
			OutboundService.updateCampaign($scope.campaign);
		};

		$scope.navigateToForecasting = function() {
			$state.go('outbound.forecasting', { id: $scope.campaign.id });
		};

		$scope.navigateToConfiguration = function() {
			$state.go('outbound.edit', { id: $scope.campaign.id });
		};

		$scope.timeRanges = [
			{
				startTime: new Date(1970, 0, 1, 7, 0, 0),
				endTime: new Date(1970, 0, 1, 18, 0, 0),				
				id: 1
			},
			{
				startTime: new Date(1970, 0, 1, 7, 0, 0),
				endTime: new Date(1970, 0, 1, 11, 0, 0),
				id: 2
			},
			{
				startTime: new Date(1970, 0, 1, 13, 0, 0),
				endTime: new Date(1970, 0, 1, 18, 0, 0),
				id: 3
			}

		];

		$scope.newTimeRange = {
			startTime: new Date(1970, 0, 1, 7, 0, 0),
			endTime: new Date(1970, 0, 1, 18, 0, 0)
		};

		$scope.curTimeRangeIdMax = 3;
		$scope.addToTimeRanges = function () {
			var timeRange = angular.copy($scope.newTimeRange);
			$scope.curTimeRangeIdMax += 1;
			timeRange.id = $scope.curTimeRangeIdMax;
			$scope.timeRanges.push(timeRange);

		};

		$scope.deleteFromTimeRanges = function(timeRange, idx) {
			if (confirm('Are you sure you want to delete this time range?')) {				
				$scope.timeRanges.splice(idx, 1);				
			}
		};
	}
]);


