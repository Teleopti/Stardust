"use strict";

(function () {


	var outbound = angular.module('wfm.outbound', ['outboundServiceModule', 'ngAnimate']);


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

	outbound.controller('OutboundCreateCtrl', [
		'$scope', '$state',  'outboundService33699', 'outboundNotificationService',
		function ($scope, $state,  outboundService, outboundNotificationService) {


			$scope.$on('formLocator.createCampaignForm', function (event) {
				$scope.createCampaignForm = event.targetScope.createCampaignForm;
				$scope.formScope = event.targetScope;			
			});

			reset();

			$scope.addCampaign = function () {
				if (!isInputValid()) {
					console.log("input invalid");
					flashErrorIcons();
					return;
				}
				outboundService.addCampaign($scope.newCampaign, function (campaign) {
					reset();
					show(campaign);
					outboundNotificationService.notifyCampaignCreationSuccess(campaign);
				}, function (error) {
					outboundNotificationService.notifyCampaignCreationFailure(error);
				});
			}

			$scope.reset = reset;
			$scope.isInputValid = isInputValid;


			function isInputValid() {			
				if (!$scope.createCampaignForm) return false;

				var startDate = deepPropertyAccess($scope, ['newCampaign', 'StartDate', 'Date']);
				var endDate = deepPropertyAccess($scope, ['newCampaign', 'EndDate', 'Date']);

				if (!(startDate && endDate && startDate <= endDate)) return false;			

				return $scope.createCampaignForm.$valid;
			}

			function flashErrorIcons() {
				$scope.$broadcast('expandable.expand');
			}

			function reset() {
				$scope.newCampaign = {};
				$scope.acToggle1 = $scope.acToggle2 = $scope.acToggle3 = true;			

				if ($scope.createCampaignForm) {
					$scope.createCampaignForm.$setPristine();
				}						
			}

			function show(campaign) {
				$state.go('outbound.edit', { Id: campaign.Id });
			}

			function deepPropertyAccess(obj, propertiesChain) {
				var curObj = obj;
				for (var i = 0; i < propertiesChain.length; i += 1) {
					var curProperty = propertiesChain[i];
					if (angular.isDefined(curObj[curProperty])) curObj = curObj[curProperty];
					else return;
				}
				return curObj;
			}

		}
	]);

	outbound.controller('OutboundListCtrl', [
		'$scope', '$state', 'growl', 'outboundService',
		function ($scope, $state, growl, outboundService) {

			$scope.newName = "";
			$scope.selectedTarget = null;
			$scope.showMoreHeaderFields = false;
			
			$scope.reset = function() {
				$scope.newName = "";
				$scope.form.$setPristine();
			};

			$scope.campaigns = outboundService.listCampaign({}, function () {
				$scope.selectedTarget = outboundService.getCurrentCampaign();
				$scope.$broadcast('outbound.campaigns.loaded');
			});

			$scope.gotoCreateCampaign = function() {
				$state.go('outbound-create');
			};

			$scope.create = function() {
				outboundService.addCampaign({ name: $scope.newName }, function (_newCampaign_) {
					$scope.show(_newCampaign_);
					notifySuccess(growl, "New campaign <strong>" + $scope.newName + "</strong> created");
				}, function(error) {
					notifyFailure(growl, "Failed to create campaign " + error);
				});
			};

			$scope.copyNew = function(campaign) {
				outboundService.addCampaign({ name: campaign.name + "_Copy" }, function (_newCampaign_) {
					$scope.show(_newCampaign_);
					notifySuccess(growl, "New campaign <strong>" + campaign.name + "_Copy"  + "</strong> created");
				}, function (error) {
					notifyFailure(growl, "Failed to clone campaign " + error);
				});
			};
			$scope.update = function(campaign) {
				outboundService.updateCampaign(campaign, function () {
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
				outboundService.deleteCampaign(campaign, function () {
					notifySuccess(growl, "Campaign deleted successfully");
				}, function(error) {
					notifyFailure(growl, "Failed to delete campaign " + error);
				});
				$state.go('outbound.edit', { Id: null });
			}
		}
	]);

	outbound.controller('OutboundEditCtrl', [
		'$scope', '$stateParams', '$state', '$filter', 'growl', 'outboundService',
		function($scope, $stateParams, $state, $filter, growl, OutboundService) {
			$scope.campaign = (angular.isDefined($stateParams.Id) && $stateParams.Id != "") ? OutboundService.getCampaignById($stateParams.Id) : null;
			$scope.newWorkingPeriod = { StartTime: null, EndTime: null };
			$scope.showCampaignDetail = angular.isDefined($scope.campaign) && ($scope.campaign != null);
			$scope.isPeriodInValid = false;

			$scope.acToggle1 = true;
			$scope.acToggle2 = true;
			$scope.acToggle3 = true;

			$scope.useMeridian = true;

			$scope.$watch('campaign.StartDate.Date', function() {
				$scope.update(true);
			});
			$scope.$watch('campaign.EndDate.Date', function () {
				$scope.update(true);
			});

			$scope.$on('outbound.campaigns.loaded', function() {
				$scope.campaign = (angular.isDefined($stateParams.Id) && $stateParams.Id != "") ? OutboundService.getCampaignById($stateParams.Id) : null;
				$scope.showCampaignDetail = angular.isDefined($scope.campaign) && ($scope.campaign != null);
			});
		
			$scope.$on('formLocator.formCampaignParams', function (event) {
				$scope.formCampaignParams = event.targetScope.formCampaignParams;
				$scope.formScope = event.targetScope;
			});

			$scope.update = function (isAlwaysUpdate) {
				if ($scope.campaign == null) return;
				if ($scope.formCampaignParams == null) return;

				if (isAlwaysUpdate || ($scope.formCampaignParams.$dirty && $scope.formCampaignParams.$valid ) ) {
					OutboundService.updateCampaign($scope.campaign,
					function () {
						$scope.formScope.$broadcast("campaign.view.refresh");
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

			$scope.addWorkingPeriod = function () {
				if ($scope.newWorkingPeriod.EndTime != null && $scope.newWorkingPeriod.StartTime != null)
				if ($scope.newWorkingPeriod.EndTime >= $scope.newWorkingPeriod.StartTime) {
					$scope.isPeriodInValid = false;
					OutboundService.addWorkingPeriod($scope.campaign, angular.copy($scope.newWorkingPeriod));
				} else {
					$scope.isPeriodInValid = true;
				}
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