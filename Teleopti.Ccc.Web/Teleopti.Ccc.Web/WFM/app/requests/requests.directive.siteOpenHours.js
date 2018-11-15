'use strict';
(function() {
	angular
		.module('wfm.requests')
		.controller('requestsSiteOpenHoursController', [
			'requestsDataService',
			'requestsNotificationService',
			'requestsPermissions',
			requestsSiteOpenHoursController
		])
		.directive('requestsSiteOpenHours', requestsSiteOpenHoursDirective);

	function requestsSiteOpenHoursController(requestsDataService, requestsNotificationService, requestsPermissions) {
		var vm = this;
		var sitesBefore, sitesAfter;
		vm.openHours = [];
		vm.sites = [];
		vm.save = save;
		vm.cancel = cancel;
		vm.selectedSite = null;
		vm.hasEditPermission = requestsPermissions.all().HasEditSiteOpenHoursPermission;
		init();
		function init() {
			var getSiteProgress = requestsDataService.getSitesPromise();
			getSiteProgress.success(function(sites) {
				sitesBefore = sites;
				vm.sites = [];
				angular.forEach(sites, function(site) {
					vm.sites.push(denormalizeSite(site));
				});
				if (vm.sites.length > 0) vm.selectedSite = vm.sites[0].Id;
			});
		}

		function save() {
			var sites = [];
			angular.forEach(vm.sites, function(site) {
				sites.push(normalizeSite(site));
			});
			sitesAfter = sites;
			if (isOpenHoursSame(sitesBefore, sitesAfter)) {
				requestsNotificationService.notifyNothingChanged();
			} else {
				var maintainOpenHoursProgress = requestsDataService.maintainOpenHoursPromise(sites);
				maintainOpenHoursProgress.success(function(persistedSites) {
					requestsNotificationService.notifySaveSiteOpenHoursSuccess(persistedSites);
				});
			}
			cleanUp();
		}

		function cancel() {
			cleanUp();
		}

		function cleanUp() {
			vm.sites = [];
			vm.showSite = false;
		}

		// these utilities have nothing to do with requests, so I keep everything contained in this directive
		function isOpenHoursSame(sitesBefore, sitesAfter) {
			if (!sitesBefore || !sitesAfter) return;
			var isSame = true;
			for (var i = 0; i < sitesBefore.length && isSame; i++) {
				var beforeOpenHours = sitesBefore[i].OpenHours;
				var afterOpenHours = sitesAfter[i].OpenHours;
				if (beforeOpenHours.length === 0 && afterOpenHours.length > 0) {
					isSame = false;
					continue;
				}
				for (var j = 0; j < beforeOpenHours.length && isSame; j++) {
					var openHourBefore = beforeOpenHours[j];
					var openHourAfter = afterOpenHours.filter(function(compareOpenHour) {
						return openHourBefore.WeekDay === compareOpenHour.WeekDay;
					})[0];

					if (!openHourAfter) {
						isSame = false;
						continue;
					}

					if (openHourAfter.IsClosed && openHourBefore.IsClosed) {
						continue;
					}

					if (openHourAfter.IsClosed !== openHourBefore.IsClosed) {
						isSame = false;
						continue;
					}

					if (openHourAfter.StartTime + ':00' !== openHourBefore.StartTime) {
						isSame = false;
						continue;
					}

					if (openHourAfter.EndTime + ':00' !== openHourBefore.EndTime) {
						isSame = false;
						continue;
					}
				}
			}
			return isSame;
		}

		function normalizeSite(site) {
			var copySite = angular.copy(site);
			var weekDayOpenHours = [];
			angular.forEach(copySite.OpenHours, function(openHour) {
				var timespan = formatTimespanObj({
					StartTime: openHour.StartTime,
					EndTime: openHour.EndTime
				});
				for (var j = 0; j < 7; j++) {
					var weekDayOpenHour = weekDayOpenHours[j];
					if (!weekDayOpenHour) {
						weekDayOpenHour = createEmptyWeekDayOpenHour(j);
						weekDayOpenHours.push(weekDayOpenHour);
					}
					if (!weekDayOpenHour.IsClosed) {
						continue;
					}
					var openHourSelection = getOpenHourSelection(openHour.WeekDaySelections, j);
					weekDayOpenHour.EndTime = timespan.EndTime;
					weekDayOpenHour.StartTime = timespan.StartTime;
					weekDayOpenHour.IsClosed = openHourSelection && !openHourSelection.Checked;
				}
			});
			site.OpenHours = weekDayOpenHours;

			return site;
		}

		function getOpenHourSelection(weekDaySelections, weekDay) {
			var openHourSelections = weekDaySelections.filter(function(weekDaySelection) {
				return weekDay === weekDaySelection.WeekDay;
			});

			var openHourSelection = openHourSelections[0];
			if (openHourSelections.length > 1) {
				angular.forEach(openHourSelections, function(selection) {
					if (!selection.IsClosed) {
						openHourSelection = selection;
					}
				});
			}

			return openHourSelection;
		}

		function createEmptyWeekDayOpenHour(weekDay) {
			return {
				EndTime: '00:00',
				IsClosed: true,
				StartTime: '00:00',
				WeekDay: weekDay
			};
		}

		function denormalizeSite(site) {
			var siteCopy = angular.copy(site);
			var reformattedWorkingHours = [];
			siteCopy.OpenHours.forEach(function(a) {
				var startTime = parseTimespanString(a.StartTime),
					endTime = parseTimespanString(a.EndTime),
					timespan = {
						StartTime: startTime,
						EndTime: endTime
					};
				var workingHourRows = reformattedWorkingHours.filter(function(wh) {
					return sameTimespan(timespan, wh);
				});
				var workingHourRow;
				if (workingHourRows.length === 0) {
					workingHourRow = createEmptyWorkingPeriod(startTime, endTime);
					if (a.StartTime !== '00:00:00' || a.EndTime !== '00:00:00')
						reformattedWorkingHours.push(workingHourRow);
				} else {
					workingHourRow = workingHourRows[0];
				}
				angular.forEach(workingHourRow.WeekDaySelections, function(e) {
					if (e.WeekDay === a.WeekDay) {
						e.Checked = !a.IsClosed;
						e.IsClosed = a.IsClosed;
					}
				});
			});
			siteCopy.OpenHours = reformattedWorkingHours;
			return siteCopy;
		}
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
	function createEmptyWorkingPeriod(startTime, endTime) {
		var weekdaySelections = [];
		var startDow = moment.localeData()._week ? moment.localeData()._week.dow : 0;
		for (var i = 0; i < 7; i++) {
			var curDow = (startDow + i) % 7;
			weekdaySelections.push({ WeekDay: curDow, Checked: false });
		}
		return { StartTime: startTime, EndTime: endTime, WeekDaySelections: weekdaySelections };
	}
	function sameTimespan(timespan1, timespan2) {
		var formattedTimespan1 = formatTimespanObj(timespan1),
			formattedTimespan2 = formatTimespanObj(timespan2);
		return (
			formattedTimespan1.StartTime == formattedTimespan2.StartTime &&
			formattedTimespan1.EndTime == formattedTimespan2.EndTime
		);
	}
	function parseTimespanString(t) {
		if (!angular.isString(t)) return t;
		var parts = t.match(/^(\d[.])?(\d{1,2}):(\d{1,2})(:|$)/);
		if (parts) {
			var d = new Date();
			d.setHours(parts[2]);
			d.setMinutes(parts[3]);
			if (parts[1])
				return moment(d)
					.add(1, 'days')
					.toDate();
			else return d;
		}
	}
	function requestsSiteOpenHoursDirective() {
		return {
			scope: {
				showSite: '='
			},
			restrict: 'E',
			controller: 'requestsSiteOpenHoursController',
			controllerAs: 'requestsSiteOpenHours',
			bindToController: true,
			templateUrl: 'app/requests/html/requests-open-hours.html'
		};
	}
})();
