"use strict";

(function() {
	var outbound = angular.module('wfm.outbound', ['outboundService']);

	var notifySuccess = function (growl, message) {
		growl.success("<i class='mdi mdi-thumb-up'></i> " + message + ".", {
			ttl: 5000,
			disableCountDown: true
		});
	}

	var notifyFailure = function (growl, message) {
		growl.error("<i class='mdi  mdi-alert-octagon'></i> " + message + ".", {
			ttl: 5000,
			disableCountDown: true
		});
	}

	outbound.controller('OutboundListCtrl', [
		'$scope', '$state', 'growl', 'OutboundService',
		function($scope, $state, growl, OutboundService) {

			$scope.newName = "";
			$scope.selectedTarget = null;
			$scope.showMoreHeaderFields = false;
			
			$scope.reset = function() {
				$scope.newName = "";
				$scope.form.$setPristine();
			};

			$scope.campaigns = OutboundService.listCampaign({}, function() {
				$scope.selectedTarget = OutboundService.getCurrentCampaign();
				$scope.$broadcast('outbound.campaigns.loaded');
			});

			$scope.create = function() {
				OutboundService.addCampaign({ name: $scope.newName }, function(_newCampaign_) {
					$scope.show(_newCampaign_);
					notifySuccess(growl, "New campaign <strong>" + $scope.newName + "</strong> created");
				}, function(error) {
					notifyFailure(growl, "Failed to create campaign " + error);
				});
			};

			$scope.copyNew = function(campaign) {
				OutboundService.addCampaign({ name: campaign.name + "_Copy" }, function(_newCampaign_) {
					$scope.show(_newCampaign_);
					notifySuccess(growl, "New campaign <strong>" + campaign.name + "_Copy"  + "</strong> created");
				}, function (error) {
					notifyFailure(growl, "Failed to clone campaign " + error);
				});
			};
			$scope.update = function(campaign) {
				OutboundService.updateCampaign(campaign, function() {
					notifySuccess(growl, "Campaign updated successfully");
				}, function(error) {
					notifyFailure(growl, "Failed to update campaign " + error);
				});
			};

			$scope.show = function(campaign) {
				if (angular.isDefined(campaign)) $scope.selectedTarget = campaign;
				$state.go('outbound.edit', { Id: $scope.selectedTarget.Id });
				$scope.showMoreHeaderFields = false;
			};

			$scope.delete = function(campaign) {
				OutboundService.deleteCampaign(campaign, function() {
					notifySuccess(growl, "Campaign deleted successfully");
				}, function(error) {
					notifyFailure(growl, "Failed to delete campaign " + error);
				});
				$state.go('outbound.edit', { Id: null });
			}
		}
	]);

	outbound.controller('OutboundEditCtrl', [
		'$scope', '$stateParams', '$state', '$filter', 'growl', 'OutboundService',
		function($scope, $stateParams, $state, $filter, growl, OutboundService) {
			$scope.campaign = (angular.isDefined($stateParams.Id) && $stateParams.Id != "") ? OutboundService.getCampaignById($stateParams.Id) : null;
			$scope.newWorkingPeriod = { StartTime: null, EndTime: null };
			$scope.showCampaignDetail = angular.isDefined($scope.campaign) && ($scope.campaign != null);

			$scope.$on('outbound.campaigns.loaded', function() {
				$scope.campaign = (angular.isDefined($stateParams.Id) && $stateParams.Id != "") ? OutboundService.getCampaignById($stateParams.Id) : null;
				$scope.showCampaignDetail = angular.isDefined($scope.campaign) && ($scope.campaign != null);
			});
		
			$scope.$on('formLocator.formCampaignParams', function (event) {
				$scope.formCampaignParams = event.targetScope.formCampaignParams;				
			});

			$scope.update = function (isAlwaysUpdate) {
				if (isAlwaysUpdate || $scope.formCampaignParams.$dirty) {
					OutboundService.updateCampaign($scope.campaign,
					function () {
						notifySuccess(growl, "Campaign updated successfully");
					},
					function (error) {
						notifyFailure(growl, "Failed to update campaign " + error);
					});
				}
				$scope.formCampaignParams.$setPristine();
			};

			$scope.navigateToForecasting = function() {
				$state.go('outbound.forecasting', { Id: $scope.campaign.Id });
			};

			$scope.navigateToConfiguration = function() {
				$state.go('outbound.edit', { Id: $scope.campaign.Id });
			};

			var clearConflictWorkingPeriodAssignments = function(WorkingPeriod, WeekDay) {
				angular.forEach($scope.campaign.CampaignWorkingPeriods, function(workingPeriod) {
					workingPeriod.ExpandedWorkingPeriodAssignments[WeekDay.WeekDay].Checked =
						workingPeriod == WorkingPeriod;
				});
			};


			$scope.toggleWorkingPeriodAssignment = function(WorkingPeriod, WeekDay) {
				if (WeekDay.Checked) {
					clearConflictWorkingPeriodAssignments(WorkingPeriod, WeekDay);
					OutboundService.addWorkingPeriodAssignment($scope.campaign, WorkingPeriod, WeekDay);
				} else {
					OutboundService.deleteWorkingPeriodAssignment($scope.campaign, WorkingPeriod, WeekDay);
				}
			};

			$scope.addWorkingPeriod = function() {
				OutboundService.addWorkingPeriod($scope.campaign, angular.copy($scope.newWorkingPeriod));
			};

			$scope.deleteWorkingPeriod = function(workingPeriod) {
				OutboundService.deleteWorkingPeriod($scope.campaign, workingPeriod);
			};

			$scope.resetWorkingPeriodForm = function() {
				$scope.newWorkingPeriod = { StartTime: null, EndTime: null };
			};

			$scope.toggleWorkingPeriodSelect = function(workingPeriod) {
				workingPeriod.selected = ! workingPeriod.selected;
			};


		}
	]);
})();