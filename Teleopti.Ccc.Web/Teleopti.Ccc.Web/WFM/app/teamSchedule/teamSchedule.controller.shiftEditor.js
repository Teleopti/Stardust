
(function () {
	'use strict';

	angular.module('wfm.teamSchedule').controller('ShiftEditorViewController', [
		'$stateParams',
		function ($stateParams) {
			var vm = this;
			vm.personId = $stateParams.personId;
			vm.timezone = decodeURIComponent($stateParams.timezone);
			vm.date = $stateParams.date;
		}
	]);

	angular.module('wfm.teamSchedule').component('shiftEditor', {
		controller: ShiftEditorController,
		controllerAs: 'vm',
		templateUrl: 'app/teamSchedule/html/shiftEditor.html',
		bindings: {
			personId: '<',
			date: '<',
			timezone: '<'
		}
	});

	ShiftEditorController.$inject = [
		'$scope',
		'$element',
		'$timeout',
		'$window',
		'$interval',
		'$filter',
		'$state',
		'$translate',
		'TeamSchedule',
		'serviceDateFormatHelper',
		'ShiftEditorViewModelFactory',
		'TimezoneListFactory',
		'ActivityService',
		'ShiftEditorService',
		'CurrentUserInfo',
		'guidgenerator',
		'signalRSVC',
		'NoticeService'
	];

	function ShiftEditorController(
		$scope,
		$element,
		$timeout,
		$window,
		$interval,
		$filter,
		$state,
		$translate,
		TeamSchedule,
		serviceDateFormatHelper,
		ShiftEditorViewModelFactory,
		TimezoneListFactory,
		ActivityService,
		ShiftEditorService,
		CurrentUserInfo,
		guidgenerator,
		signalRSVC,
		NoticeService
	) {
		var doNotToggleSelectionAfterResizeEnd;

		var vm = this;
		var timeLineTimeRange = {
			Start: moment
				.tz(vm.date, vm.timezone)
				.add(-1, 'days')
				.hours(0),
			End: moment
				.tz(vm.date, vm.timezone)
				.add(3, 'days')
				.hours(0)
		};

		vm.showScrollLeftButton = false;
		vm.showScrollRightButton = false;
		vm.isInDifferentTimezone = false;
		vm.displayDate = moment(vm.date).format('L');
		vm.availableActivities = [];
		vm.trackId = guidgenerator.newGuid();
		vm.isSaving = false;

		vm.$onInit = function () {
			getSchedule();

			ActivityService.fetchAvailableActivities().then(function (data) {
				vm.availableActivities = data;
			});

			vm.timelineVm = ShiftEditorViewModelFactory.CreateTimeline(vm.date, vm.timezone, timeLineTimeRange);

			TimezoneListFactory.Create().then(function (timezoneList) {
				vm.timezoneName = timezoneList.GetShortName(vm.timezone);
			});

			subscribeToScheduleChange();
		};

		vm.gotoDayView = function () {
			$state.go('teams.dayView');
		};

		vm.isSameDate = function (interval) {
			return moment.tz(vm.date, vm.timezone).isSame(interval.Time, 'days');
		};

		vm.scroll = function (step) {
			var viewportEl = $element[0].querySelector('.viewport');
			viewportEl.scrollLeft += step;
			displayScrollButton();
		};

		vm.isNotAllowedToChange = function (shiftLayer) {
			return !shiftLayer.ShiftLayerIds || !shiftLayer.ShiftLayerIds.length;
		};

		vm.toggleSelection = function (shiftLayer, $event) {
			if (vm.isNotAllowedToChange(shiftLayer)) return;
			if (doNotToggleSelectionAfterResizeEnd) {
				doNotToggleSelectionAfterResizeEnd = false;
				return;
			}
			vm.selectedShiftLayer = shiftLayer !== vm.selectedShiftLayer ? shiftLayer : null;
			vm.selectedActivitiyId = vm.getMergedShiftLayer(shiftLayer).ActivityId;

			bindResizeEvent(shiftLayer, $event.target);
		};

		vm.useLighterBorder = function (shiftLayer) {
			var sl = vm.getMergedShiftLayer(shiftLayer);
			return shiftLayer.UseLighterBorder(sl.Color);
		}

		vm.changeActivityType = function () {
			var selectActivity = vm.availableActivities.filter(function (activity) {
				return vm.selectedActivitiyId == activity.Id;
			})[0];
			vm.selectedShiftLayer.Current = vm.selectedShiftLayer.Current || {};
			vm.selectedShiftLayer.Current.Color = selectActivity.Color;
			vm.selectedShiftLayer.Current.Description = selectActivity.Name;
			vm.selectedShiftLayer.Current.ActivityId = selectActivity.Id;
		};

		vm.saveChanges = function () {
			if (vm.scheduleChanged) {
				vm.showError = true;
				return;
			}
			vm.isSaving = true;
			ShiftEditorService.changeActivityType(vm.date, vm.personId, getChangedLayers(), {
				TrackId: vm.trackId
			}).then(
				function (response) {
					vm.isSaving = false;
					var errorMessages = getErrorMessagesFromActionResults(response.data);
					if (!!errorMessages.length) {
						showErrorNotice(errorMessages);
						return;
					}
					getSchedule();
					showSuccessNotice();
				},
				function () {
					vm.isSaving = false;
				}
				);
		};

		vm.isSaveButtonDisabled = function () {
			return !hasChanges() || vm.isSaving || vm.showError;
		};

		vm.refreshData = function () {
			if (vm.scheduleChanged) {
				getSchedule();
			}
		};

		vm.getTimeSpan = function () {
			return vm.getMergedShiftLayer(vm.selectedShiftLayer).TimeSpan;
		};

		vm.getMergedShiftLayer = function (layer) {
			return angular.extend({}, layer, layer.Current);
		}

		function bindResizeEvent(shiftLayer, shiftLayerEl) {
			!shiftLayer.interact &&
				(shiftLayer.interact = interact(shiftLayerEl))
					.resizable({
						allowFrom: '.selected',
						edges: { left: true, right: true },
						restrictSize: {
							min: { width: 5 }
						}
					})
					.on('resizemove', function (event) {
						var left = event.deltaRect.left;
						var width = event.rect.width;

						shiftLayer.isChangingStart = left != 0;

						resizeLayer(shiftLayer, (vm.getMergedShiftLayer(shiftLayer).TranslateX || 0) + left, width);

						$scope.$apply();
					})
					.on('resizeend', function (event) {
						var mergedShiftLayer = vm.getMergedShiftLayer(shiftLayer);
						resizeLayer(
							shiftLayer,
							round5(mergedShiftLayer.TranslateX),
							round5(mergedShiftLayer.Width),
							function (x, w, left) {
								var startMinutes = left + x;
								var endMinutes = startMinutes + w;

								var startTime = timeLineTimeRange.Start.clone().add(startMinutes, 'minutes');
								var endTime = timeLineTimeRange.Start.clone().add(endMinutes, 'minutes');

								removeCoveredLayers(shiftLayer, startTime, endTime);

								var curIndex = vm.scheduleVm.ShiftLayers.indexOf(shiftLayer);

								updateLayer(
									shiftLayer,
									serviceDateFormatHelper.getDateTime(startTime),
									serviceDateFormatHelper.getDateTime(endTime)
								);

								if (shiftLayer.isChangingStart) {
									var previousIndex = curIndex - 1;

									var previousShiftLayer = vm.scheduleVm.ShiftLayers[previousIndex];
									if (previousShiftLayer) {
										updateLayer(previousShiftLayer, vm.getMergedShiftLayer(previousShiftLayer).Start, shiftLayer.Current.Start);

										resizeLayer(previousShiftLayer, previousShiftLayer.Current.TranslateX || 0, getShiftLayerWidth(previousShiftLayer));
										
									}
								} else {
									var nextIndex = curIndex + 1;
									var nextShiftLayer = vm.scheduleVm.ShiftLayers[nextIndex];

									if (nextShiftLayer) {
										updateLayer(nextShiftLayer, shiftLayer.Current.End, vm.getMergedShiftLayer(nextShiftLayer).End);

										var translateX = getDiffMinutes(
											nextShiftLayer.Current.Start,
											moment.tz(nextShiftLayer.Start, vm.timezone)
										);
										var nextLayerWidth = getShiftLayerWidth(nextShiftLayer);
										resizeLayer(nextShiftLayer, translateX, nextLayerWidth)
									}
								}
							}
						);

						doNotToggleSelectionAfterResizeEnd = true;
					});
		}

		function updateLayer(shiftLayer, startTime, endTime) {
			shiftLayer.Current = shiftLayer.Current || {};
			shiftLayer.Current.Start = startTime;
			shiftLayer.Current.End = endTime;
			shiftLayer.Current.TimeSpan =
				moment.tz(startTime, vm.timezone).format('L LT') +
				' - ' +
				moment.tz(endTime, vm.timezone).format('L LT');
		}

		function redrawPreviousLayers(shiftLayer, startTime, endTime) {
			var index = vm.scheduleVm.ShiftLayers.indexOf(shiftLayer);
			if (index === 0) return false;

			for (var i = index - 1; i >= 0; i--) {
				var layer = vm.scheduleVm.ShiftLayers[i];
				if (layer.FloatOnTop && moment.tz(layer.Start, vm.timezone).isSameOrBefore(startTime)) {
					return true;
				}
				else if (layer.ActivityId === shiftLayer.ActivityId && moment.tz(layer.Start, vm.timezone).isSameOrBefore(startTime)) {
					return true;
				}
			}
			return false;
		}


		function resizeLayer(shiftLayer, x, width, afterResize) {
			shiftLayer.Current = shiftLayer.Current || {};

			shiftLayer.Current.Width = width;
			shiftLayer.Current.TranslateX = x || 0;

			afterResize && afterResize(x, width, shiftLayer.Left);
		}

		function removeCoveredLayers(shiftLayer, startTime, endTime) {
			var indexs = [];
			vm.scheduleVm.ShiftLayers.forEach(function (layer, i) {
				if (
					shiftLayer !== layer &&
					!layer.FloatOnTop &&
					endTime.isSameOrAfter(moment.tz(vm.getMergedShiftLayer(layer).End, vm.timezone)) &&
					startTime.isSameOrBefore(moment.tz(vm.getMergedShiftLayer(layer).Start, vm.timezone))
				) {
					indexs.push(i);
				}
			});

			if (indexs.length) vm.scheduleVm.ShiftLayers.splice(indexs[0], indexs.length);
		}

		function round5(number) {
			var a = number % 5;
			var b = number - a;
			var isMinus = number < 0;
			return a === 0 ? number : Math.abs(a) >= 3 ? (isMinus ? b - 5 : b + 5) : b;
		}

		function showErrorNotice(errorMessages) {
			angular.forEach(errorMessages, function (message) {
				NoticeService.error(message, null, true);
			});
		}

		function showSuccessNotice() {
			var successMessage = $translate.instant('SuccessfulMessageForSavingScheduleChanges');
			NoticeService.success(successMessage, 5000, true);
		}

		function getErrorMessagesFromActionResults(actionResults) {
			var errorMessages = [];
			actionResults.forEach(function (x) {
				if (x.ErrorMessages && x.ErrorMessages.length > 0) {
					x.ErrorMessages.forEach(function (message) {
						errorMessages.push(message);
					});
				}
			});
			return errorMessages;
		}

		function getSchedule() {
			initScheduleState();

			vm.isLoading = true;
			TeamSchedule.getSchedules(vm.date, [vm.personId]).then(function (data) {
				vm.scheduleVm = ShiftEditorViewModelFactory.CreateSchedule(vm.date, vm.timezone, data.Schedules[0]);
				vm.scheduleVm.ShiftLayers &&
					vm.scheduleVm.ShiftLayers.forEach(function (layer) {
						layer.Width = getShiftLayerWidth(layer);
						layer.Left = getShiftLayerLeft(layer);
					});
				vm.isInDifferentTimezone = vm.scheduleVm.Timezone !== vm.timezone;
				initAndBindScrollEvent();
				vm.isLoading = false;
			});
		}

		function getShiftLayerWidth(layer) {
			var mergedLayer = vm.getMergedShiftLayer(layer);
			var startInTimezone = moment.tz(mergedLayer.Start, vm.timezone);
			return getDiffMinutes(mergedLayer.End, startInTimezone);
		}

		function getShiftLayerLeft(layer) {
			return getDiffMinutes(vm.getMergedShiftLayer(layer).Start, timeLineTimeRange.Start);
		}

		function initScheduleState() {
			vm.scheduleChanged = false;
			vm.selectedShiftLayer = null;
			vm.selectedActivitiyId = null;
			vm.showError = false;
		}

		function subscribeToScheduleChange() {
			signalRSVC.subscribeBatchMessage(
				{ DomainType: 'IScheduleChangedInDefaultScenario' },
				function (messages) {
					for (var i = 0; i < messages.length; i++) {
						var message = messages[i];
						if (
							message.DomainReferenceId === vm.personId &&
							moment(vm.date).isBetween(
								getMomentDate(message.StartDate),
								getMomentDate(message.EndDate),
								'day',
								'[]'
							)
						) {
							if (vm.trackId !== message.TrackId) {
								vm.scheduleChanged = true;
							}
							return;
						}
					}
				},
				300
			);
		}

		function getMomentDate(date) {
			return moment(serviceDateFormatHelper.getDateOnly(date.substring(1, date.length)));
		}

		function getChangedLayers() {
			var currentUserTimezone = CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;
			var changedShiftLayers = vm.scheduleVm.ShiftLayers.filter(function (sl) {
				return !!sl.Current && !!sl.Current.ActivityId && sl.ActivityId !== sl.Current.ActivityId;
			});

			var sameShiftLayers = changedShiftLayers.filter(function (sl) {
				return !!sl.ShiftLayerIds.filter(function (id) {
					return (
						vm.scheduleVm.ShiftLayers.filter(function (isl) {
							return isl.ShiftLayerIds && isl.ShiftLayerIds.indexOf(id) >= 0;
						}).length > 1
					);
				}).length;
			});

			return changedShiftLayers.map(function (sl) {
				if (!sl.TopShiftLayerId && sameShiftLayers.indexOf(sl) >= 0) {
					var startTime = moment
						.tz(sl.Start, vm.timezone)
						.clone()
						.tz(currentUserTimezone);
					var endTime = moment
						.tz(sl.End, vm.timezone)
						.clone()
						.tz(currentUserTimezone);
					return {
						ActivityId: sl.Current.ActivityId,
						ShiftLayerIds: [sl.ShiftLayerIds[0]],
						StartTime: serviceDateFormatHelper.getDateTime(startTime),
						EndTime: serviceDateFormatHelper.getDateTime(endTime),
						IsNew: true
					};
				} else {
					return {
						ActivityId: sl.Current.ActivityId,
						ShiftLayerIds: !!sl.TopShiftLayerId ? [sl.TopShiftLayerId] : sl.ShiftLayerIds
					};
				}
			});
		}

		function hasChanges() {
			return (
				hasShift() &&
				!!vm.scheduleVm.ShiftLayers.filter(function (layer) {
					return !!layer.Current && !!layer.Current.ActivityId && layer.Current.ActivityId !== layer.ActivityId;
				}).length
			);
		}

		function initAndBindScrollEvent() {
			var viewportEl = $element[0].querySelector('.viewport');

			if (!hasShift()) return;

			initScrollState();
			angular.element($window).bind('resize', initScrollState);

			bindScrollMouseEvent(viewportEl.querySelector('.left-scroll'), -20);
			bindScrollMouseEvent(viewportEl.querySelector('.right-scroll'), 20);
		}

		var scrollIntervalPromise = null;
		function cancelScrollIntervalPromise() {
			if (scrollIntervalPromise) {
				$interval.cancel(scrollIntervalPromise);
				scrollIntervalPromise = null;
			}
		}

		function bindScrollMouseEvent(el, step) {
			el.addEventListener(
				'mousedown',
				function () {
					cancelScrollIntervalPromise();
					scrollIntervalPromise = $interval(function () {
						vm.scroll(step);
					}, 150);
				},
				false
			);
			el.addEventListener(
				'mouseup',
				function () {
					cancelScrollIntervalPromise();
				},
				false
			);
			el.addEventListener(
				'mouseleave',
				function () {
					cancelScrollIntervalPromise();
				},
				false
			);
		}
		function initScrollState() {
			$timeout(function () {
				var viewportEl = $element[0].querySelector('.viewport');
				var shiftProjectionTimeRange = vm.scheduleVm.ProjectionTimeRange;
				var shiftStart = getDiffMinutes(shiftProjectionTimeRange.Start, timeLineTimeRange.Start);
				var shiftLength = getDiffMinutes(shiftProjectionTimeRange.End, timeLineTimeRange.Start) - shiftStart;
				var scrollTo =
					viewportEl.clientWidth <= shiftLength
						? shiftStart - 120
						: shiftStart - (viewportEl.clientWidth - shiftLength) / 2;
				if (scrollTo <= 0) scrollTo = 0;
				viewportEl.scrollLeft = scrollTo;
				displayScrollButton();
			});
		}

		function displayScrollButton() {
			var viewportEl = $element[0].querySelector('.viewport');
			vm.showScrollLeftButton = viewportEl.scrollLeft > 0;
			vm.showScrollRightButton = viewportEl.scrollWidth > viewportEl.scrollLeft + viewportEl.clientWidth;
		}

		function hasShift() {
			return !!vm.scheduleVm && !!vm.scheduleVm.ShiftLayers && !!vm.scheduleVm.ShiftLayers.length;
		}

		function getDiffMinutes(dateTime, fromTime) {
			var hours = 0;
			var startTime = fromTime.clone();
			var dateTimeMoment = moment.tz(dateTime, vm.timezone);

			while (dateTimeMoment.diff(startTime, 'hours') > 0) {
				hours += 1;
				startTime = startTime.add(1, 'hours');
			}
			return hours * 60 + dateTimeMoment.diff(startTime, 'minutes');
		}
	}
})();
