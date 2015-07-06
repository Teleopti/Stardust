(function() {
	
	'use strict';

	angular.module('outboundServiceModule', ['ngResource'])
		.service('outboundService', ['$resource', '$filter', outboundService])
		.service('outboundNotificationService', ['growl', outboundNotificationService])
		.service('outboundService33699', ['$filter', '$http', 'outboundActivityService', outboundService_33699])
		.service('outboundActivityService', ['$http', outboundActivityService]);
	

	function outboundActivityService($http) {

		var listActivityCommandUrl = '../api/Outbound/Campaign/Activities';
		var activities = [];
		var sync = false;

		this.listActivity = function () {			
			if (!sync) {
				activities.splice(0, activities.length);
				$http.get(listActivityCommandUrl).success(function(data) {
					if (angular.isArray(data)) {
						angular.forEach(data, function(e) {
							activities.push(e);
						});
					}
					sync = true;
				});
			}
			return activities;
		};

		this.nullActivity = function() {
			return {
				Id: null,
				Name: ''
			};
		};
		
		this.refresh = function() { sync = false; };
	}

	function outboundService_33699($filter, $http, outboundActivityService) {
	
		var createCampaignCommandUrl = '../api/Outbound/Campaign';
		var getCampaignCommandUrl = '../api/Outbound/Campaign/';
		var listCampaignCommandUrl = '../api/Outbound/Campaign';
		
		this.listCampaign = function (filter, successCb, errorCb) {
			$http.get(listCampaignCommandUrl).success(function(data) {

					if (successCb != null) successCb(data);
				}).
				error(function(data) {
					if (errorCb != null) errorCb(data);
				});
		};

		this.getCampaign = function (campaignId, successCb, errorCb) {
			$http.get(getCampaignCommandUrl + campaignId).
				success(function (data) {					
					if (successCb != null) successCb(denormalizeCampaign(data));
				}).
				error(function (data) {
					if (errorCb != null) errorCb(data);
				});
		};

		this.addCampaign = function(campaign, successCb, errorCb) {
			$http.post(createCampaignCommandUrl, normalizeCampaign(campaign)).
				success(function (data) {
					outboundActivityService.refresh();
					if (successCb != null) successCb(data);
				}).
				error(function(data) {
					if (errorCb != null) errorCb(data);
				});
		};

		this.createEmptyWorkingPeriod = createEmptyWorkingPeriod;
		this.calculateCampaignPersonHour = calculateCampaignPersonHour;

		function calculateCampaignPersonHour(campaign) {
			var Target = campaign.CallListLen * campaign.TargetRate / 100,
				RPCR = campaign.RightPartyConnectRate / 100,
				CR = campaign.ConnectRate / 100,
				Unproductive = campaign.UnproductiveTime,
				ConnectAHT = campaign.ConnectAverageHandlingTime,
				RPCAHT = campaign.RightPartyAverageHandlingTime;

			if (RPCAHT == 0 || CR == 0) return null;

			var hours = (Target * (RPCAHT + Unproductive)
				+ (Target / RPCR - Target) * (ConnectAHT + Unproductive)
				+ (Target / (CR * RPCR) - Target / RPCR) * Unproductive) / 60 / 60;

			return hours;
		}

		function normalizeCampaign(campaign) {
			var campaign = angular.copy(campaign);

			var formattedWorkingHours = [];

			campaign.WorkingHours.forEach(function (d) {
				d.WeekDaySelections.forEach(function(e) {
					if (e.Checked) {
						formattedWorkingHours.push({
							WeekDay: e.WeekDay,							
							StartTime: formatTimespanInput(d.StartTime),
							EndTime: formatTimespanInput(d.EndTime)							
						});
					}
				});						
			});

			campaign.WorkingHours = formattedWorkingHours;
			return campaign;
		}

		function formatTimespanInput(dtObj) {
			return $filter('date')(dtObj, 'HH:mm');
		}

		
		function denormalizeCampaign(campaign) {
			var campaign = angular.copy(campaign);
			var reformattedWorkingHours = [];
		
			campaign.WorkingHours.forEach(function (a) {
				var workingHourRows = reformattedWorkingHours.filter(function(wh) { return wh.StartTime == a.StartTime && wh.EndTime == a.EndTime;});
				var workingHourRow; 
				if (workingHourRows.length == 0) {
					workingHourRow = createEmptyWorkingPeriod(a.StartTime, a.EndTime);

					angular.forEach(workingHourRow.WeekDaySelections, function(e) {
						if (e.WeekDay == a.WeekDay) e.Checked = true;
					});				
					reformattedWorkingHours.push(workingHourRow);
				} else {
					workingHourRow = workingHourRows[0];
					angular.forEach(workingHourRow.WeekDaySelections, function (e) {
						if (e.WeekDay == a.WeekDay) e.Checked = true;
					});					
				}
									
			});
			campaign.WorkingHours = reformattedWorkingHours;	
			return campaign;
		};

		function createEmptyWorkingPeriod(startTime, endTime) {
			var weekdaySelections = [];
			var startDow = (moment.localeData()._week) ? moment.localeData()._week.dow : 0;
	
			for (var i = 0; i < 7; i++) {
				var curDow = (startDow + i) % 7;
				weekdaySelections.push({ WeekDay: curDow, Checked: false });
			}

			return { StartTime: startTime, EndTime: endTime, WeekDaySelections: weekdaySelections };
		}
	}


	function outboundService($resource, $filter) {
		var self = this;
		var Campaign = $resource('../api/Outbound/Campaign/:Id', { Id: '@Id' }, {
			update: { method: 'PUT' }
		});

		var CampaignWorkingPeriod = $resource('../api/Outbound/Campaign/:CampaignId/WorkingPeriod/:WorkingPeriodId', {
			CampaignId: '@CampaignId',
			WorkingPeriodId: '@WorkingPeriodId'
		}, {
			update: { method: 'PUT' }
		});

		var expandWorkingPeriod = function (workingPeriod) {
			var assignments = angular.isDefined(workingPeriod.WorkingPeroidAssignments) ? workingPeriod.WorkingPeroidAssignments : [];
			var expandedAssignments = [];
			for (var i = 0; i < 7; i++) {
				var assigned = assignments.filter(function (x) { return x.WeekDay == i; });
				expandedAssignments.push({ WeekDay: i, Checked: assigned.length > 0 });
			}
			workingPeriod.ExpandedWorkingPeriodAssignments = expandedAssignments;
		};

		self.campaigns = [];
		self.currentCampaignId = null;

		self.getCurrentCampaign = function () {
			if (self.currentCampaignId == null) return null;
			var matched = self.campaigns.filter(function (campaign) { return campaign.Id == self.currentCampaignId; });
			if (matched.length == 0) return null;
			else return matched[0];
		};

		self.addCampaign = function (campaign, successCb, errorCb) {
			var newCampaign = new Campaign(campaign);
			newCampaign.$save(
				function () {
					if (angular.isDefined(successCb))
						successCb(newCampaign);
				},
				function (data) {
					if (angular.isDefined(errorCb))
						errorCb(data);
				});

			self.campaigns.push(newCampaign);
			return newCampaign;
		};

		self.listCampaign = function (campaignFilter, successCb, errorCb) {
			self.campaigns = Campaign.query({},
				function () {
					if (angular.isDefined(successCb))
						successCb();
				},
				function (data) {
					if (angular.isDefined(errorCb))
						errorCb(data);
				});
			return self.campaigns;
		};

		self.getCampaignById = function (Id) {
			self.currentCampaignId = Id;
			var matched = self.campaigns.filter(function (campaign) { return campaign.Id === Id; });
			if (matched.length === 0) return null;
			var campaign = matched[0];
			if (!(angular.isDefined(campaign.IsFull) && campaign.IsFull)) {
				var fetched = Campaign.get({ Id: Id }, function () {
					campaign = angular.extend(campaign, fetched, { IsFull: true });
					angular.forEach(campaign.CampaignWorkingPeriods, function (period) {
						expandWorkingPeriod(period);
					});
						
				});
			}
			return campaign;
		};
		

		self.updateCampaign = function (campaign, successCb, errorCb) {
			Campaign.update(campaign, function () {
				if (angular.isDefined(successCb))
					successCb();
			},
				function (data) {
					if (angular.isDefined(errorCb))
						errorCb(data);
				});
		};

		self.deleteCampaign = function (campaign, successCb, errorCb) {
			Campaign.remove({ Id: campaign.Id },
				function () {
					if (angular.isDefined(successCb))
						successCb();
				},
				function (data) {
					if (angular.isDefined(errorCb))
						errorCb(data);
				});
			self.campaigns.splice(self.campaigns.indexOf(campaign), 1);
		}

		self.addWorkingPeriod = function (campaign, workingPeriod) {
			var newWorkingPeriod = new CampaignWorkingPeriod();
			newWorkingPeriod.CampaignId = campaign.Id;
			newWorkingPeriod.StartTime = $filter('date')(workingPeriod.StartTime, 'HH:mm');
			newWorkingPeriod.EndTime = $filter('date')(workingPeriod.EndTime, 'HH:mm');
			newWorkingPeriod.WorkingPeriod = {
				period: {
					_minimum: $filter('date')(workingPeriod.StartTime, 'HH:mm:ss'),
					_maximum: $filter('date')(workingPeriod.EndTime, 'HH:mm:ss')
				}
			};
			newWorkingPeriod.$save(function () {
				expandWorkingPeriod(newWorkingPeriod);
				campaign.CampaignWorkingPeriods.push(newWorkingPeriod);
			});
		};

		self.deleteWorkingPeriod = function (campaign, workingPeriod) {
			CampaignWorkingPeriod.remove({ CampaignId: campaign.Id, WorkingPeriodId: workingPeriod.Id });
			campaign.CampaignWorkingPeriods.splice(campaign.CampaignWorkingPeriods.indexOf(workingPeriod), 1);
		}

		self.addWorkingPeriodAssignment = function (campaign, workingPeriod, weekDay) {
			CampaignWorkingPeriod.update({
				CampaignId: campaign.Id,
				WeekDay: weekDay.WeekDay,
				CampaignWorkingPeriods: [workingPeriod.Id]
			});
		};

		self.deleteWorkingPeriodAssignment = function (campaign, workingPeriod, weekDay) {
			CampaignWorkingPeriod.update({
				CampaignId: campaign.Id,
				WeekDay: weekDay.WeekDay,
				CampaignWorkingPeriods: []
			});
		};


	}

	function outboundNotificationService($growl) {
		

		this.notifySuccess = notifySuccess;
		this.notifyFailure = notifyFailure;

		this.notifyCampaignCreationSuccess = function (campaign) {			
			notifySuccess("New campaign <strong>" + campaign.Name + "</strong> created");
		}

		this.notifyCampaignCreationFailure = function (error) {
			notifyFailure("Failed to create campaign "  + (error && error.Message? error.Message: error));
		}

		this.notifyCampaignLoadingFailure = function(error) {
			notifyFailure("Failed to load campaign " + (error && error.Message ? error.Message : error));
		}

		function notifySuccess(message) {
			$growl.success("<i class='mdi mdi-thumb-up'></i> " + message + ".", {
				ttl: 5000,
				disableCountDown: true
			});
		}

		function notifyFailure(message) {
			$growl.error("<i class='mdi  mdi-alert-octagon'></i> " + message + ".", {
				ttl: 5000,
				disableCountDown: true
			});
		}

	}


})();


