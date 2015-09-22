﻿(function() {
	'use strict';

	angular.module('outboundServiceModule')
		.service('outboundService', ['$filter', '$http', 'miscService', outboundService]);

	function outboundService($filter, $http, miscService) {

		var createCampaignCommandUrl = '../api/Outbound/Campaign';
		var getCampaignCommandUrl = '../api/Outbound/Campaign/';
		var getCampaignLoadUrl = '../api/Outbound/Campaign/Load';
		var postCampaignLoadUrl = '../api/Outbound/Campaign/Period/Load';
		var editCampaignCommandUrl = '../api/Outbound/Campaign/';
		var getCampaignStatisticsUrl = '../api/Outbound/Campaign/Statistics';
		var getCampaignPeriodStatisticsUrl = '../api/Outbound/Campaign/Period/Statistics';
		var getFilteredCampaignsUrl = '../api/Outbound/Campaigns';
		var getPeriodCampaignsUrl = '../api/Outbound/Period/Campaigns';
		var getGanttVisualizationUrl = '../api/Outbound/Gantt/Campaigns';

		var self = this;

		self.getVisualizationPeriod = function() {
			return {
				StartDate: { Date: moment().subtract(1, "months").date(1).format() },
				EndDate: { Date: moment().add(2, "months").date(1).subtract(1, 'days').format() }
			}
		}

		self.getDefaultPeriod = function() {
			return [moment().subtract(1, "months").date(1), moment().add(2, "months").date(1).subtract(1, 'days')];
		}

		this.load = function(successCb) {
			$http.post(getCampaignLoadUrl).success(function(data) {
				if (successCb != null) successCb(data);
			});
		}

		this.loadWithinPeriod = function(successCb) {
			var period = self.getVisualizationPeriod();
			$http.post(postCampaignLoadUrl, period).success(function(data) {
				if (successCb != null) successCb(data);
			});
		};

		this.getGanttVisualization = function(filter, successCb, errorCb) {
			$http.post(getGanttVisualizationUrl, filter).
				success(function(data) {
					if (successCb != null)
						successCb(data);
				}).
				error(function(data) {
					if (errorCb != null) errorCb(data);
				});
		}

		this.getCampaignStatistics = function(filter, successCb, errorCb) {
			$http.post(getCampaignStatisticsUrl).success(function(data) {
					if (successCb != null) successCb(data);
				}).
				error(function(data) {
					if (errorCb != null) errorCb(data);
				});
		};

		this.getCampaignStatisticsWithinPeriod = function(filter, successCb, errorCb) {
			var period = self.getVisualizationPeriod();
			$http.post(getCampaignPeriodStatisticsUrl, period).
				success(function(data) {
					if (successCb != null)
						successCb(data);
				}).
				error(function(data) {
					if (errorCb != null) errorCb(data);
				});
		}

		this.getCampaignSummary = function(id, successCb, errorCb) {
			$http.get(getFilteredCampaignsUrl + '/' + id).
				success(function(data) {
					if (successCb != null)
						successCb(data);
				}).
				error(function(data) {
					if (errorCb != null) errorCb(data);
				});
		};

		this.listFilteredCampaigns = function(filter, successCb, errorCb) {
			$http.post(getFilteredCampaignsUrl, filter).
				success(function(data) {
					if (successCb != null)
						successCb(data);
				}).
				error(function(data) {
					if (errorCb != null) errorCb(data);
				});
		};

		this.listCampaignsWithinPeriod = function(successCb, errorCb) {
			var period = self.getVisualizationPeriod();
			$http.post(getPeriodCampaignsUrl, period).success(function (data) {
					if (successCb != null)
						successCb(data);
				}).
				error(function(data) {
					if (errorCb != null) errorCb(data);
				});
		};

		this.getCampaign = function(campaignId, successCb, errorCb) {
			$http.get(getCampaignCommandUrl + campaignId).
				success(function(data) {
					if (successCb != null) successCb(denormalizeCampaign(data));
				}).
				error(function(data) {
					if (errorCb != null) errorCb(data);
				});
		};

		this.addCampaign = function(campaign, successCb, errorCb) {
			$http.post(createCampaignCommandUrl, normalizeCampaign(campaign)).
				success(function(data) {
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

		this.removeCampaign = function(campaign, successCb, errorCb) {
			$http.delete(getCampaignCommandUrl + campaign.Id)
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
			if (campaign.StartDate) {
				campaign.StartDate.Date = miscService.sendDateToServer(campaign.StartDate.Date);
			}
			if (campaign.EndDate) {
				campaign.EndDate.Date = miscService.sendDateToServer(campaign.EndDate.Date);
			}

			var campaign = angular.copy(campaign);

			var formattedWorkingHours = [];

			campaign.WorkingHours.forEach(function(d) {
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
			if (!angular.isString(t)) return t;
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

			if (campaign.StartDate) {
				campaign.StartDate.Date = miscService.getDateFromServer(campaign.StartDate.Date);
			}
			if (campaign.EndDate) {
				campaign.EndDate.Date = miscService.getDateFromServer(campaign.EndDate.Date);
			}

			var reformattedWorkingHours = [];

			campaign.WorkingHours.forEach(function(a) {

				var startTime = parseTimespanString(a.StartTime);
				var endTime = parseTimespanString(a.EndTime);

				var workingHourRows = reformattedWorkingHours.filter(function(wh) {
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
					angular.forEach(workingHourRow.WeekDaySelections, function(e) {
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
})();