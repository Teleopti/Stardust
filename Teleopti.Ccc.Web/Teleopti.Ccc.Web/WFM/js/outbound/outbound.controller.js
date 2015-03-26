var outbound = angular.module('wfm.outbound', ['outboundService']);

outbound.controller('OutboundListCtrl', [
	'$scope', '$state', 'OutboundService',
	function($scope, $state, OutboundService) {
		$scope.campaigns = OutboundService.listCampaign();
		$scope.newName = "";
		$scope.selectedTarget = null;
		$scope.hideDetail = false;

		$scope.reset = function () {
			$scope.selectedTarget = null;		
			$scope.form.$setPristine();
		};
		
		$scope.create = function () {
			OutboundService.addCampaign({ name: $scope.newName });			
		};

		$scope.copyNew = function (campaign) {
			OutboundService.addCampaign({ name: campaign.name + "_Copy" });
		};

		$scope.update = function(campaign) {
			OutboundService.updateCampaign(campaign);
		};

		$scope.show = function(campaign) {
			$scope.selectedTarget = campaign;
			$state.go('outbound.edit', { id: $scope.selectedTarget.id });
		};

		$scope.delete = function (campaign, idx) {		
			if (confirm('Are you sure you want to delete this record?')) {
				OutboundService.deleteCampaign(campaign, idx);
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
		$scope.availableWorkingHours = [
			{ label: "Closed", value: "Closed" }
		];

		$scope.$on("outbound.campaigns.updated", function() {
			$scope.campaign = OutboundService.getCampaignById($stateParams.id);
		});

		$scope.campaign = OutboundService.getCampaignById($stateParams.id);

		$scope.period = {
			startDate: moment().add(1, 'months').startOf('month').toDate(),
			endDate: moment().add(2, 'months').startOf('month').toDate()
		};

		$scope.params = {
			skill: $scope.skills[0]
		};

	}
]);