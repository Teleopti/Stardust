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
			var newCampaign = OutboundService.addCampaign({ name: $scope.newName })
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
	'$scope', '$stateParams', 'OutboundService', 
	function ($scope, $stateParams, OutboundService) {
	
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
	}
]);


