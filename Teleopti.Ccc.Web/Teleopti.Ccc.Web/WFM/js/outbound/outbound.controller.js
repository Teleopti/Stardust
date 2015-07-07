(function () {
	"use strict";

	angular.module('wfm.outbound')
		.controller('OutboundCreateCtrl', [
			'$scope', '$state', 'outboundService33699', 'outboundNotificationService',
			createCtrl])
		.controller('OutboundEditCtrl', [
			'$scope', '$state', '$stateParams', '$timeout', 'outboundService33699', 'outboundNotificationService',
			editCtrl]);


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

	function editCtrl($scope, $state, $stateParams, $timeout, outboundService, outboundNotificationService) {

		init();
		var originalCampaign;
		var muteDirtyWorkingHoursWatcher;

		$scope.init = init;
		$scope.editCampaign = editCampaign;
		$scope.reset = reset;
		$scope.dirtyWorkingHours = false;
			
		function editCampaign() {
			if (!$scope.isInputValid()) {
				$scope.flashErrorIcons();
				return;
			}
			outboundService.editCampaign($scope.campaign, function (campaign) {
				outboundNotificationService.notifyCampaignUpdateSuccess(angular.copy(campaign));
				init();			
			}, function (error) {
				outboundNotificationService.notifyCampaignUpdateFailure(error);
			});
		}

		function reset() {
			$scope.campaign = angular.copy(originalCampaign);
			
			$scope.campaignGeneralForm.$setPristine();
			$scope.campaignWorkloadForm.$setPristine();
			$scope.campaignSpanningPeriodForm.$setPristine();

			muteDirtyWorkingHoursWatcher = true;
			$scope.dirtyWorkingHours = false;
			$timeout(function () {
				muteDirtyWorkingHoursWatcher = false;
			});
						
		}

		function init() {
			$scope.campagin = null;
			$scope.isCampaignLoaded = function () { return angular.isDefined($scope.campaign); };

			var currentCampaignId = (angular.isDefined($stateParams.Id) && $stateParams.Id != "") ? $stateParams.Id : null;
			if (currentCampaignId == null) return;

			outboundService.getCampaign(currentCampaignId, function (campaign) {
				originalCampaign = campaign;
				$scope.campaign = angular.copy(campaign);

				expandAllSections($scope);
				registerCampaignForms($scope);
				registerPersonHourFeedback($scope, outboundService);
				setupValidators($scope);
				
				$scope.$watch(function () {
					return $scope.campaign.WorkingHours;
				}, function (newValue, oldValue) {
					if (newValue !== oldValue && !muteDirtyWorkingHoursWatcher) {
						$scope.dirtyWorkingHours = true;
					}					
				}, true);

			}, function () {
				outboundNotificationService.notifyCampaignLoadingFailure({ Message: currentCampaignId });
			});
		}
	}

	function createCtrl($scope, $state, outboundService, outboundNotificationService) {
		
		init();

		$scope.addCampaign = addCampaign;
		$scope.init = init;		
		$scope.backToList = backToList;
		
		function addCampaign() {
			if (!$scope.isInputValid()) {
				$scope.flashErrorIcons();
				return;
			}
			outboundService.addCampaign($scope.campaign, function (campaign) {
				outboundNotificationService.notifyCampaignCreationSuccess(angular.copy(campaign));
				init();
				show(campaign);
			}, function (error) {
				outboundNotificationService.notifyCampaignCreationFailure(error);
			});
		}	

		function init() {
			$scope.campaign = {
				Activity: {},
				StartDate: { Date: new Date() },
				EndDate: { Date: new Date() },
				WorkingHours: []
			};

			expandAllSections($scope);
			registerCampaignForms($scope);
			registerPersonHourFeedback($scope, outboundService);
			setupValidators($scope);			
		}

		function show(campaign) {
			$state.go('outbound.edit', { Id: campaign.Id });
		}

		function backToList() {
			$state.go('outbound.edit');
		}

	}

	function registerCampaignForms(scope) {
		scope.$on('formLocator.campaignGeneralForm', function (event) {
			scope.campaignGeneralForm = event.targetScope.campaignGeneralForm;
			scope.formScope = event.targetScope;
			scope.campaignGeneralForm.$setPristine();
		});

		scope.$on('formLocator.campaignWorkloadForm', function (event) {
			scope.campaignWorkloadForm = event.targetScope.campaignWorkloadForm;
			scope.formScope = event.targetScope;
			scope.campaignWorkloadForm.$setPristine();
		});

		scope.$on('formLocator.campaignSpanningPeriodForm', function (event) {
			scope.campaignSpanningPeriodForm = event.targetScope.campaignSpanningPeriodForm;
			scope.formScope = event.targetScope;
			scope.campaignSpanningPeriodForm.$setPristine();
		});
	}

	function registerPersonHourFeedback(scope, outboundService) {
		scope.$watch(function () {
			return scope.campaignWorkloadForm && scope.campaignWorkloadForm.$valid ?
				outboundService.calculateCampaignPersonHour(scope.campaign) : null;
		}, function (newValue) {
			scope.estimatedWorkload = newValue;
		});
	}

	function setupValidators(scope) {
		scope.isInputValid = isInputValid;
		scope.validWorkingHours = validWorkingHours;
		scope.flashErrorIcons = flashErrorIcons;
		scope.setRangeClass = setDateRangeClass;

		scope.$watch(function () {
			var startDate = deepPropertyAccess(scope, ['campaign', 'StartDate', 'Date']);
			var endDate = deepPropertyAccess(scope, ['campaign', 'EndDate', 'Date']);
			return { startDate: startDate, endDate: endDate };
		}, function (value) {
			if (scope.campaignSpanningPeriodForm) {
				if (!isDateValid(value.startDate, value.endDate)) {									
					scope.campaignSpanningPeriodForm.$setValidity('time-range', false);
				} else {
					scope.campaignSpanningPeriodForm.$setValidity('time-range', true);
				}
			}		
			scope.campaign.StartDate = { Date: angular.copy(value.startDate) };
			scope.campaign.EndDate = { Date: angular.copy(value.endDate) };
		}, true);

		function isInputValid() {
			if (!scope.campaignGeneralForm) return false;
			if (!scope.campaignWorkloadForm) return false;
			if (!scope.campaignSpanningPeriodForm) return false;
			
			return scope.campaignGeneralForm.$valid
				&& scope.campaignWorkloadForm.$valid
				&& scope.campaignSpanningPeriodForm.$valid
				&& validWorkingHours();
		}

		function isDateValid(startDate, endDate) {
			return endDate && startDate && startDate <= endDate;
		}

		function validWorkingHours() {
			var i, j;
			for (i = 0; i < scope.campaign.WorkingHours.length; i++) {
				for (j = 0; j < scope.campaign.WorkingHours[i].WeekDaySelections.length; j++) {
					if (scope.campaign.WorkingHours[i].WeekDaySelections[j].Checked) return true;
				}
			}
			return false;
		}

		function setDateRangeClass(date, mode) {
			if (mode === 'day') {
				var startDate = deepPropertyAccess(scope, ['campaign', 'StartDate', 'Date']);
				var endDate = deepPropertyAccess(scope, ['campaign', 'EndDate', 'Date']);

				if (startDate && endDate && startDate <= endDate) {
					var dayToCheck = new Date(date).setHours(12, 0, 0, 0);
					var start = new Date(startDate).setHours(12, 0, 0, 0);
					var end = new Date(endDate).setHours(12, 0, 0, 0);

					if (dayToCheck >= start && dayToCheck <= end) {
						return 'in-date-range';
					}
				}
			}
			return '';
		}

		function flashErrorIcons() {
			scope.$broadcast('expandable.expand');
		}
	} 


	var outbound = angular.module('wfm.outbound');

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