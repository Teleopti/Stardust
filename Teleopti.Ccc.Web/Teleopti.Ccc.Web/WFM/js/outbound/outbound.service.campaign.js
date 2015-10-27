(function() {
	'use strict';

	angular.module('outboundServiceModule')
		.service('outboundService', ['$filter', '$http', 'miscService', outboundService]);

	function outboundService($filter, $http, miscService) {

		var createCampaignCommandUrl = '../api/Outbound/Campaign';
		var getCampaignCommandUrl = '../api/Outbound/Campaign/';
		var getCampaignLoadUrl = '../api/Outbound/Campaign/Load';
		var reloadCampaignSchedulesUrl = '../api/Outbound/Campaign/Period/Load';
		var editCampaignCommandUrl = '../api/Outbound/Campaign/';
		var getCampaignStatisticsUrl = '../api/Outbound/Campaign/Statistics';
		var getCampaignPeriodStatisticsUrl = '../api/Outbound/Campaign/Period/Statistics';
		var getFilteredCampaignsUrl = '../api/Outbound/Campaigns';
		var getPeriodCampaignsUrl = '../api/Outbound/Period/Campaigns';
		var getGanttVisualizationUrl = '../api/Outbound/Gantt/Campaigns';
		var getCampaignDetailUrl = "../api/Outbound/Campaign/Detail";
		var updateThresholdUrl = '../api/Outbound/Campaign/ThresholdsSetting';
		var loadCampaignScheduleUrl = '../api/Outbound/Campaign/Navigation';
	
		var self = this;

		this.getThreshold = function (successCb) {
			$http.post(updateThresholdUrl).success(function(data) {
				if (successCb != null) successCb(data);
			});
		}

		this.loadCampaignSchedule = function (period, successCb, errorCb) {
			$http.put(loadCampaignScheduleUrl, normalizePeriod(period))
				.success(function (data) {
					if (successCb != null) successCb(data);
				})
				.error(function (data) {
					if (errorCb != null) errorCb(data);
				});
		}

		this.updateThreshold = function (threshold, successCb, errorCb) {
			$http.put(updateThresholdUrl, threshold)
				.success(function(data) {
					if (successCb != null) successCb(data);
				})
				.error(function(data) {
					if (errorCb != null) errorCb(data);
				});
		}

		self.getGanttPeriod = function(periodStart) {
			periodStart = periodStart ? moment(periodStart).clone() : moment().subtract(1, "months");
			periodStart.date(1);

			var periodEnd = periodStart.clone().add(3, 'months').subtract(1, 'days');
			return {
				PeriodStart: periodStart.toDate(),
				PeriodEnd: periodEnd.toDate()
			}
		};

		this.load = function(successCb) {
			$http.post(getCampaignLoadUrl).success(function(data) {
				if (successCb != null) successCb(data);
			});
		}

		this.reloadCampaignSchedules = function(period, successCb) {			
			$http.post(reloadCampaignSchedulesUrl, normalizePeriod(period)).success(function (data) {
				if (successCb != null) successCb(data);
			});
		};

		this.getGanttVisualization = function (period, successCb, errorCb) {
			$http.post(getGanttVisualizationUrl, normalizePeriod(period)).
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

		this.getCampaignStatisticsWithinPeriod = function(period, successCb, errorCb) {			
			$http.post(getCampaignPeriodStatisticsUrl, normalizePeriod(period)).
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

		this.getCampaignDetail = function (id, successCb, errorCb) {
			$http.post(getCampaignDetailUrl, {CampaignId: id}).
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

		this.listCampaignsWithinPeriod = function(period, successCb, errorCb) {			
			$http.post(getPeriodCampaignsUrl, normalizePeriod(period)).success(function (data) {
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

		function normalizePeriod(period) {
			return {
				StartDate: { Date: miscService.sendDateToServer(period.PeriodStart) },
				EndDate: { Date: miscService.sendDateToServer(period.PeriodEnd) }
			};
		};



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
						var timespan = formatTimespanObj({
							StartTime: d.StartTime,
							EndTime: d.EndTime
						});

						
						formattedWorkingHours.push({
							WeekDay: e.WeekDay,
							StartTime: timespan.StartTime,
							EndTime: timespan.EndTime
						});
					}
				});
			});

			campaign.WorkingHours = formattedWorkingHours;
			return campaign;
		}

		function formatTimespanObj(timespan) {
			var startTimeMoment = moment(timespan.StartTime),
				endTimeMoment = moment(timespan.EndTime);

			if (startTimeMoment.isSame(endTimeMoment, 'day')) {
				return {
					StartTime: startTimeMoment.format('HH:mm'),
					EndTime: endTimeMoment.format('HH:mm')
				};
			} else {
				return {
					StartTime: startTimeMoment.format('HH:mm'),
					EndTime: '1.' + endTimeMoment.format('HH:mm')
				};
			}
		}

		function parseTimespanString(t) {
			if (!angular.isString(t)) return t;
			var parts = t.match(/^(\d[.])?(\d{1,2}):(\d{1,2})(:|$)/);
			if (parts) {
				var d = new Date();
				d.setHours(parts[2]);
				d.setMinutes(parts[3]);

				if (parts[1]) return moment(d).add(1, 'days').toDate();
				else return d;
			}
		}

		function sameTimespan(timespan1, timespan2) {
			var formattedTimespan1 = formatTimespanObj(timespan1),
				formattedTimespan2 = formatTimespanObj(timespan2);
			return formattedTimespan1.StartTime == formattedTimespan2.StartTime &&
				formattedTimespan1.EndTime == formattedTimespan2.EndTime;
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

				var startTime = parseTimespanString(a.StartTime),
					endTime = parseTimespanString(a.EndTime),
					timespan = {
						StartTime: startTime,
						EndTime: endTime
					};
				
				var workingHourRows = reformattedWorkingHours.filter(function (wh) {
					return sameTimespan(timespan, wh);
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