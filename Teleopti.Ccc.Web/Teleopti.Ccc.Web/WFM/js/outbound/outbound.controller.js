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

			reset();

			$scope.addCampaign = addCampaign;
			$scope.reset = reset;
			$scope.isInputValid = isInputValid;
			$scope.backToList = backToList;
		
			$scope.$on('formLocator.campaignGeneralForm', function (event) {
				$scope.campaignGeneralForm = event.targetScope.campaignGeneralForm;
				$scope.formScope = event.targetScope;
			});

			$scope.$on('formLocator.campaignWorkloadForm', function (event) {
				$scope.campaignWorkloadForm = event.targetScope.campaignWorkloadForm;
				$scope.formScope = event.targetScope;
			});

			$scope.$watch(function () {
				return $scope.newCampaign;				
			}, function() {
				$scope.estimatedWorkload = $scope.campaignWorkloadForm && $scope.campaignWorkloadForm.$valid ?
					outboundService.calculateCampaignPersonHour($scope.newCampaign) + ' person-hour' : '';
			}, true);

			function isInputValid() {			
				if (!$scope.campaignGeneralForm) return false;
				if (!$scope.campaignWorkloadForm) return false;

				var startDate = deepPropertyAccess($scope, ['newCampaign', 'StartDate', 'Date']);
				var endDate = deepPropertyAccess($scope, ['newCampaign', 'EndDate', 'Date']);

				if (!(startDate && endDate && startDate <= endDate)) return false;			
				return $scope.campaignGeneralForm.$valid && $scope.campaignWorkloadForm.$valid;
			}


			function addCampaign() {
				if (!isInputValid()) {					
					flashErrorIcons();
					return;
				}
				outboundService.addCampaign($scope.newCampaign, function (campaign) {				
					outboundNotificationService.notifyCampaignCreationSuccess(angular.copy(campaign));
					reset();
					show(campaign);
				}, function (error) {
					outboundNotificationService.notifyCampaignCreationFailure(error);
				});
			}

			function flashErrorIcons() {
				$scope.$broadcast('expandable.expand');
			}

			function reset() {				
				$scope.newCampaign = {
					activity: {},
					WorkingHours: [
						outboundService.createEmptyWorkingPeriod(new Date(2000, 1, 1, 7, 0), new Date(2000, 1, 1, 12, 0)),
						outboundService.createEmptyWorkingPeriod(new Date(2000, 1, 1, 13, 0), new Date(2000, 1, 1, 18, 0))
					]
				};

				expandAllSections($scope);

				if ($scope.campaignGeneralForm) {
					$scope.campaignGeneralForm.$setPristine();
				}

				if ($scope.campaignWorkloadForm) {
					$scope.campaignWorkloadForm.$setPristine();
				}
			}
		
			function show(campaign) {
				$state.go('outbound.edit', { Id: campaign.Id });
			}

			function backToList() {
				$state.go('outbound.edit');
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
		'$scope', '$stateParams', '$state', '$filter', 'growl', 'outboundService33699', 'outboundNotificationService',
		function($scope, $stateParams, $state, $filter, growl, outboundService, outboundNotificationService) {

			init();

			function init() {
				$scope.campagin = null;

				var currentCampaignId = (angular.isDefined($stateParams.Id) && $stateParams.Id != "") ? $stateParams.Id : null;
				if (currentCampaignId == null) return;

				outboundService.getCampaign(currentCampaignId, function(campaign) {
					$scope.campaign = campaign;
					expandAllSections($scope);
				}, function() {
					outboundNotificationService.notifyCampaignLoadingFailure({ Message: currentCampaignId });
				});
			}

			$scope.$watch(function () {
				return $scope.campaign;
			}, function () {
				$scope.estimatedWorkload = $scope.campaignWorkloadForm && $scope.campaignWorkloadForm.$valid ?
					outboundService.calculateCampaignPersonHour($scope.campaign) + ' person-hour' : '';
			}, true);

			$scope.isCampaignLoaded = function() { return angular.isDefined($scope.campaign); };

	
			$scope.$on('formLocator.campaignWorkloadForm', function (event) {
				$scope.campaignWorkloadForm = event.targetScope.campaignWorkloadForm;
				$scope.formScope = event.targetScope;
			});

			$scope.$on('formLocator.campaignGeneralForm', function (event) {
				$scope.campaignGeneralForm = event.targetScope.campaignGeneralForm;
				$scope.formScope = event.targetScope;
			});
		
			$scope.editCampaign = function() {

			};
				
		}
	]);

	function expandAllSections(scope) {
		scope.acToggle0 = true;
		scope.acToggle1 = true;
		scope.acToggle2 = true;
		scope.acToggle3 = true;
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

	
})();