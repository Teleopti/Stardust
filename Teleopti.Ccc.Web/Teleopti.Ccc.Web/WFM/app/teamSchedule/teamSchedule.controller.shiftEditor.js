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
			return !!shiftLayer.IsNew? false : !shiftLayer.ShiftLayerIds || !shiftLayer.ShiftLayerIds.length;
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
						var translateX = round5(mergedShiftLayer.TranslateX);
						var startMinutes = mergedShiftLayer.Left + translateX;

						if (shiftLayer.isChangingStart) {
							var startTime = timeLineTimeRange.Start.clone().add(startMinutes, 'minutes');
							redrawPreviousLayers(shiftLayer, startTime);
						} else {
							var width = round5(mergedShiftLayer.Width);
							var endTime = timeLineTimeRange.Start.clone().add(startMinutes + width, 'minutes');
							redrawNextLayers(shiftLayer, endTime);
						}

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

		function redrawPreviousLayers(selectedShiftLayer, startTimeInTimezone) {
			var mergedSelectedShiftLayer = vm.getMergedShiftLayer(selectedShiftLayer);
			var startTime = serviceDateFormatHelper.getDateTime(startTimeInTimezone);
			var actualStartTime = startTime;

			if (startTime === mergedSelectedShiftLayer.Start) return;

			var index = vm.scheduleVm.ShiftLayers.indexOf(selectedShiftLayer);
			var layersCopy = vm.scheduleVm.ShiftLayers.slice(0);

			if (index !== 0) {
				var isShortenLayer = startTimeInTimezone.isAfter(moment.tz(mergedSelectedShiftLayer.Start, vm.timezone));
				if (isShortenLayer) {
					var isFilled = fillWithPreviousLayer(index, mergedSelectedShiftLayer.ActivityId, startTime);
					if (!isFilled) {
						actualStartTime = mergedSelectedShiftLayer.Start;
					}
				} else {
					var firstTimeGoPassTop = true;
					var hasGonePassTopActivity = false;
					var newLayerEndTime;

					for (var i = index - 1; i >= 0; i--) {
						var layer = layersCopy[i];
						var mergedLayer = vm.getMergedShiftLayer(layer);

						if (moment.tz(mergedLayer.End, vm.timezone).isSameOrBefore(startTimeInTimezone))
							break;

						var layerStartInTimezone = moment.tz(mergedLayer.Start, vm.timezone);
						var isCoveredCompletely = startTimeInTimezone.isSameOrBefore(layerStartInTimezone);
						var isSameType = mergedLayer.ActivityId === mergedSelectedShiftLayer.ActivityId;

						if (layer.FloatOnTop) {
							hasGonePassTopActivity = startTimeInTimezone.isBefore(layerStartInTimezone);
							if (firstTimeGoPassTop) {
								firstTimeGoPassTop = false;
								actualStartTime = mergedLayer.End;
							}
							newLayerEndTime = mergedLayer.Start;
							continue;
						}

						if (hasGonePassTopActivity) {
							var nextLayer = vm.scheduleVm.ShiftLayers[i + 1];
							var mergedNextLayer = vm.getMergedShiftLayer(nextLayer);
							var needMergeNextLayer = mergedNextLayer.ActivityId === mergedSelectedShiftLayer.ActivityId;
							var newLayerStart = isCoveredCompletely && i !== 0 ? mergedLayer.Start : startTime;

							if (needMergeNextLayer) {
								updateLayer(nextLayer, newLayerStart, mergedNextLayer.End);
								resizeLayer(nextLayer, getDiffMinutes(nextLayer.Current.Start, nextLayer.Start),
									getDiffMinutes(nextLayer.Current.End, nextLayer.Current.Start));
							}
							else if (isSameType && i === 0 && startTimeInTimezone.isSameOrBefore(layerStartInTimezone)) {
								updateLayer(layer, startTime, mergedLayer.End);
								resizeLayer(layer, getDiffMinutes(layer.Current.Start, layer.Start),
									getDiffMinutes(layer.Current.End, layer.Current.Start));
								break;
							}
							else if (!isSameType) {
								createLayer(mergedSelectedShiftLayer, newLayerStart, newLayerEndTime, i + 1);
							}
						}

						if (isCoveredCompletely) {
							vm.scheduleVm.ShiftLayers.splice(i, 1);
						} else if (!isSameType) {
							updateLayer(layer, mergedLayer.Start, startTime);
							resizeLayer(layer, getDiffMinutes(layer.Current.Start, layer.Start),
								getDiffMinutes(layer.Current.End, layer.Current.Start));
							break;
						}
					}
				}
			}

			updateLayer(selectedShiftLayer, actualStartTime, mergedSelectedShiftLayer.End);
			resizeLayer(selectedShiftLayer,
				getDiffMinutes(selectedShiftLayer.Current.Start, selectedShiftLayer.Start),
				getDiffMinutes(selectedShiftLayer.Current.End, selectedShiftLayer.Current.Start));
		}

		function fillWithPreviousLayer(selectedIndex, selectedActivityId, startTime) {
			var previousIsTop = false;
			var layerStart = null;
			var isFilled = false;
			for (var i = selectedIndex - 1; i >= 0; i--) {
				var layer = vm.scheduleVm.ShiftLayers[i];
				var mergedLayer = vm.getMergedShiftLayer(layer);
				if (layer.FloatOnTop) {
					previousIsTop = true;
					layerStart = mergedLayer.End;
					continue;
				}
				if (mergedLayer.ActivityId === selectedActivityId) break;
				isFilled = true;
				if (previousIsTop) {
					createLayer(mergedLayer, layerStart, startTime, selectedIndex);
				} else {
					updateLayer(layer, mergedLayer.Start, startTime);
					resizeLayer(layer, getDiffMinutes(layer.Start, layer.Current.Start), getDiffMinutes(layer.Current.End, layer.Current.Start));
				}
				break;
			}
			return isFilled;
		}


		function redrawNextLayers(selectedShiftLayer, endTimeInTimezone) {
			var mergedSelectedShiftLayer = vm.getMergedShiftLayer(selectedShiftLayer);

			var endTime = serviceDateFormatHelper.getDateTime(endTimeInTimezone);
			if (endTime === mergedSelectedShiftLayer.End) return;

			var actualEndTime = endTime;
			var isLayerShorten = moment.tz(mergedSelectedShiftLayer.End, vm.timezone).isAfter(endTimeInTimezone);

			var index = vm.scheduleVm.ShiftLayers.indexOf(selectedShiftLayer);
			var lastIndex = vm.scheduleVm.ShiftLayers.length - 1;
			var layersCopy = vm.scheduleVm.ShiftLayers.slice(0);

			if (index !== lastIndex) {
				if (isLayerShorten) {
					if (!fillWithNextLayer(index, mergedSelectedShiftLayer.ActivityId, endTime)) {
						actualEndTime = mergedSelectedShiftLayer.End;
					}
				} else {
					var hasGonePassTopActivity = false;
					var firstTimeGoPassTop = true;

					for (var i = index + 1; i <= lastIndex; i++) {
						var orginalLayer = layersCopy[i];
						var actualIndex = vm.scheduleVm.ShiftLayers.indexOf(orginalLayer);
						var layer = vm.scheduleVm.ShiftLayers[actualIndex];
						var mergedLayer = vm.getMergedShiftLayer(layer);

						if (moment.tz(mergedLayer.Start, vm.timezone).isSameOrAfter(endTimeInTimezone))
							break;

						var layerEndInTimezone = moment.tz(mergedLayer.End, vm.timezone);
						var isCoveredCompletely = endTimeInTimezone.isSameOrAfter(layerEndInTimezone);
						var isSameType = mergedLayer.ActivityId === mergedSelectedShiftLayer.ActivityId;

						if (layer.FloatOnTop) {
							hasGonePassTopActivity = endTimeInTimezone.isAfter(layerEndInTimezone);
							if (firstTimeGoPassTop) {
								firstTimeGoPassTop = false;
								actualEndTime = mergedLayer.Start;
							}
							continue;
						}
						var deleteIndex = actualIndex;
						if (hasGonePassTopActivity) {
							var previousLayer = vm.scheduleVm.ShiftLayers[actualIndex - 1];
							var mergedPreviousLayer = vm.getMergedShiftLayer(previousLayer);

							var needMerge = mergedPreviousLayer.ActivityId === mergedSelectedShiftLayer.ActivityId;
							if (needMerge) {
								var end = (i !== lastIndex && isCoveredCompletely) ? mergedLayer.End : endTime;
								updateLayer(previousLayer, mergedPreviousLayer.Start, end);
								resizeLayer(previousLayer, mergedPreviousLayer.TranslateX, getShiftLayerWidth(previousLayer.Current));
							}
							else if (isSameType && i === lastIndex && endTimeInTimezone.isSameOrAfter(layerEndInTimezone)) {
								updateLayer(layer, mergedLayer.Start, endTime);
								resizeLayer(layer, getDiffMinutes(layer.Current.Start, layer.Start), getDiffMinutes(layer.Current.End, layer.Current.Start));
								break;
							}
							else if (!isSameType) {
								deleteIndex = actualIndex + 1;
								createLayer(mergedSelectedShiftLayer, mergedLayer.Start, endTime, actualIndex);
							}
						}

						if (isCoveredCompletely) {
							if (!isSameType || (isSameType && i === lastIndex)) {
								vm.scheduleVm.ShiftLayers.splice(deleteIndex, 1);
							}
						} else if (!isSameType) {
							updateLayer(layer, endTime, mergedLayer.End);
							resizeLayer(layer, getDiffMinutes(layer.Current.Start, layer.Start), getDiffMinutes(layer.Current.End, layer.Current.Start));
							break;
						}
					}
				}
			}

			updateLayer(selectedShiftLayer, mergedSelectedShiftLayer.Start, actualEndTime);
			resizeLayer(selectedShiftLayer,
				getDiffMinutes(selectedShiftLayer.Current.Start, selectedShiftLayer.Start),
				getDiffMinutes(selectedShiftLayer.Current.End, selectedShiftLayer.Current.Start));
		}

		function fillWithNextLayer(selectedIndex, selectedActivityId, endTime) {
			var nextIsTop = false;
			var layerEnd = null;
			var isFilled = false;
			var total = vm.scheduleVm.ShiftLayers.length;
			for (var i = selectedIndex + 1; i <= total; i++) {
				var layer = vm.scheduleVm.ShiftLayers[i];
				var mergedLayer = vm.getMergedShiftLayer(layer);
				if (layer.FloatOnTop) {
					nextIsTop = true;
					layerEnd = mergedLayer.Start;
					continue;
				}
				if (mergedLayer.ActivityId === selectedActivityId) break;
				isFilled = true;
				if (nextIsTop) {
					createLayer(mergedLayer, endTime, layerEnd, selectedIndex + 1);
				} else {
					updateLayer(layer, endTime, mergedLayer.End);
					resizeLayer(layer, getDiffMinutes(layer.Current.Start, layer.Start), getDiffMinutes(layer.Current.End, layer.Current.Start));
				}
				break;
			}
			return isFilled;
		}

		function resizeLayer(shiftLayer, x, width, afterResize) {
			shiftLayer.Current = shiftLayer.Current || {};

			shiftLayer.Current.Width = width;
			shiftLayer.Current.TranslateX = x || 0;

			afterResize && afterResize(x, width, shiftLayer.Left);
		}

		function createLayer(layer, startTime, endTime, insertIndex) {
			var newLayer = vm.scheduleVm.SpliceLayer({
				ActivityId: layer.ActivityId,
				Description: layer.Description,
				Color: layer.Color
			}, startTime, endTime, insertIndex, 0);

			resizeLayer(newLayer, 0, getDiffMinutes(newLayer.End, newLayer.Start));
			newLayer.Left = getShiftLayerLeft(newLayer);
			newLayer.IsNew = true;
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
			return getDiffMinutes(mergedLayer.End, mergedLayer.Start);
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
			var startTime = moment.isMoment(fromTime) ? fromTime.clone() : moment.tz(fromTime, vm.timezone);
			var dateTimeMoment = moment.isMoment(dateTime) ? dateTime.clone() : moment.tz(dateTime, vm.timezone);

			while (dateTimeMoment.diff(startTime, 'hours') > 0) {
				hours += 1;
				startTime = startTime.add(1, 'hours');
			}
			return hours * 60 + dateTimeMoment.diff(startTime, 'minutes');
		}
	}
})();
