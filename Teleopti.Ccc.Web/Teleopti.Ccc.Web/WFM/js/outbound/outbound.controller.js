﻿"use strict";

var outbound = angular.module('wfm.outbound', ['outboundService']);

outbound.controller('OutboundListCtrl', [
	'$scope', '$state', 'OutboundService',
	function($scope, $state, OutboundService) {

		$scope.newName = "";
		$scope.selectedTarget = null;
		$scope.hideDetail = false;

		$scope.reset = function() {
			$scope.newName = "";
			$scope.form.$setPristine();
		};

		$scope.campaigns = OutboundService.listCampaign();

		$scope.create = function() {
			OutboundService.addCampaign({ name: $scope.newName }, function(_newCampaign_) {
				$scope.show(_newCampaign_);
			});
		};

		$scope.copyNew = function(campaign) {
			OutboundService.addCampaign({ name: campaign.name + "_Copy" }, function(_newCampaign_) {
				$scope.show(_newCampaign_);		 
			});
		};
		$scope.update = function(campaign) {
			OutboundService.updateCampaign(campaign);
		};

		$scope.show = function (campaign) {
			if (angular.isDefined(campaign)) $scope.selectedTarget = campaign;
			$state.go('outbound.edit', { Id: $scope.selectedTarget.Id });
			$scope.hideDetail = false;
		};
		
		$scope.delete = function(campaign) {
			if (confirm('Are you sure you want to delete this record?')) {
				OutboundService.deleteCampaign(campaign);
				$state.go('outbound.edit', { Id: null });
			};
		}
	}		
]);

outbound.controller('OutboundEditCtrl', [
	'$scope', '$stateParams', '$state', 'OutboundService', 
	function ($scope, $stateParams, $state, OutboundService) {			
		$scope.campaign = (angular.isDefined($stateParams.Id) && $stateParams.Id != "")?OutboundService.getCampaignById($stateParams.Id): null;

		angular.forEach($scope.campaign.Skills, function (skill) {			
			if (skill.IsSelected) {
				$scope.campaign.SelectedSkill = skill;
			}
		});
		
		$scope.showDetail = angular.isDefined($scope.campaign) && ($scope.campaign != null);

		$scope.update = function () {		
			OutboundService.updateCampaign($scope.campaign);
		};

		$scope.navigateToForecasting = function() {
			$state.go('outbound.forecasting', { Id: $scope.campaign.Id });
		};

		$scope.navigateToConfiguration = function() {
			$state.go('outbound.edit', { Id: $scope.campaign.Id });
		};		
	}
]);