(function() {
	
	'use strict';

    angular.module('outboundServiceModule')
		.service('outboundNotificationService', ['growl', '$translate', outboundNotificationService])
		.service('outboundService', ['$filter', '$http', 'outboundActivityService', outboundService])
		.service('outboundActivityService', ['$http', '$q', outboundActivityService]);
	

	function outboundActivityService($http, $q) {

		var listActivityCommandUrl = '../api/Outbound/Campaign/Activities';
		
		this.listActivity = function () {
		    var deferred = $q.defer();		
			$http.get(listActivityCommandUrl).success(function(data) {
			    deferred.resolve(data);
			});
		    return deferred.promise;
		};

		this.nullActivity = function() {
			return {
				Id: null,
				Name: ''
			};
		};
	}

	function outboundService($filter, $http, outboundActivityService) {
	
		var createCampaignCommandUrl = '../api/Outbound/Campaign';
		var getCampaignCommandUrl = '../api/Outbound/Campaign/';
		var listCampaignCommandUrl = '../api/Outbound/Campaign';
		var editCampaignCommandUrl = '../api/Outbound/Campaign/';
		
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
					if (successCb != null) successCb(data);
				}).
				error(function(data) {
					if (errorCb != null) errorCb(data);
				});
		};

		this.editCampaign = function(campaign, successCb, errorCb) {
			$http.put(editCampaignCommandUrl + campaign.Id, normalizeCampaign(campaign))
				.success(function(data) {
					if (successCb != null) successCb(data);
				})
				.error(function(data) {
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

		function parseTimespanString(t) {
			if (!angular.isString(t) ) return t;
			var parts = t.match(/^(\d{1,2}):(\d{1,2})(:|$)/);
			if (parts) {
				var d = new Date();
				d.setHours(parts[1]);
				d.setMinutes(parts[2]);
				return d;
			}

		}
		
		function denormalizeCampaign(campaign) {
			var campaign = angular.copy(campaign);

			if (campaign.StartDate) campaign.StartDate.Date = new Date(campaign.StartDate.Date);
			if (campaign.EndDate) campaign.EndDate.Date = new Date(campaign.EndDate.Date);

			var reformattedWorkingHours = [];
		

			campaign.WorkingHours.forEach(function (a) {

				var startTime = parseTimespanString(a.StartTime);
				var endTime = parseTimespanString(a.EndTime);

				var workingHourRows = reformattedWorkingHours.filter(function (wh) {					
					return formatTimespanInput(wh.StartTime) == formatTimespanInput(startTime)
						&& formatTimespanInput(wh.EndTime) == formatTimespanInput(endTime);
				});
				var workingHourRow; 
				if (workingHourRows.length == 0) {
					workingHourRow = createEmptyWorkingPeriod(startTime, endTime);

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

	function outboundNotificationService($growl, $translate) {
		
		this.notifySuccess = notifySuccess;
		this.notifyFailure = notifyFailure;

		this.notifyCampaignCreationSuccess = function (campaign) {
		    notifySuccess('CampaignCreated', [
		        '<strong>' + campaign.Name + '</strong>'
		    ]);               
		}

		this.notifyCampaignUpdateSuccess = function (campaign) {
		    notifySuccess('CampaignUpdated', [
               '<strong>' + campaign.Name + '</strong>'
		    ]);
		}

		this.notifyCampaignCreationFailure = function (error) {
		    notifyFailure('CampaignFailedCreation', [
		        (error && error.Message ? error.Message : error)
		    ]);
		}

		this.notifyCampaignUpdateFailure = function (error) {
		    notifyFailure('CampaignFailedUpdate', [
                (error && error.Message ? error.Message : error)
		    ]);
		}

		this.notifyCampaignLoadingFailure = function(error) {
		    notifyFailure('CampaignFailedLoading', [
		        (error && error.Message ? error.Message : error)
		    ]);
		}

		function notifySuccess(message, params) {
		    $translate(message).then(function(text) {
		        $growl.success("<i class='mdi mdi-thumb-up'></i> "
		            + text.replace('{0}', params[0]), {
		            ttl: 5000,
		            disableCountDown: true
		        });
		    });
		}

		function notifyFailure(message, params) {
		    $translate(message).then(function (text) {
		        $growl.error("<i class='mdi  mdi-alert-octagon'></i> "
                    + text.replace('{0}', params[0]), {
		            ttl: 5000,
		            disableCountDown: true
		        });
		    });
		}

	}


})();


