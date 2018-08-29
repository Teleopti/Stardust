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
		vm.selectedShiftLayers = [];

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
			return !!shiftLayer.IsNew ? false : !shiftLayer.ShiftLayerIds || !shiftLayer.ShiftLayerIds.length;
		};

		vm.toggleSelection = function (shiftLayer, $event) {
			if (vm.isNotAllowedToChange(shiftLayer)) return;
			if (doNotToggleSelectionAfterResizeEnd) {
				doNotToggleSelectionAfterResizeEnd = false;
				return;
			}
			vm.selectedShiftLayers = vm.selectedShiftLayers.indexOf(shiftLayer) === -1 ? [shiftLayer] : [];
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

			vm.selectedShiftLayers.forEach(function (layer) {
				layer.Current = layer.Current || {};
				layer.Current.Color = selectActivity.Color;
				layer.Current.Description = selectActivity.Name;
				layer.Current.ActivityId = selectActivity.Id;
			});
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

		vm.isSelected = function (shiftLayer) {
			return vm.selectedShiftLayers.indexOf(shiftLayer) !== -1;
		}

		vm.refreshData = function () {
			if (vm.scheduleChanged) {
				getSchedule();
			}
		};

		vm.getSelectionTimeSpan = function () {
			var lastIndex = vm.selectedShiftLayers.length -1;
			var startTime = vm.getMergedShiftLayer(vm.selectedShiftLayers[0]).Start;
			var endTime = vm.getMergedShiftLayer(vm.selectedShiftLayers[lastIndex]).End;

			return getTimeSpan(startTime, endTime);
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
							redrawLayers(shiftLayer, startTime, true);
						} else {
							var width = round5(mergedShiftLayer.Width);
							var endTime = timeLineTimeRange.Start.clone().add(startMinutes + width, 'minutes');
							redrawLayers(shiftLayer, endTime, false);
						}

						doNotToggleSelectionAfterResizeEnd = true;
						$scope.$apply();
					});
		}

		function redrawLayers(selectedShiftLayer, dateTimeInTimezone, isChangingStart) {
			var mergedSelectedShiftLayer = vm.getMergedShiftLayer(selectedShiftLayer);
			var dateTime = serviceDateFormatHelper.getDateTime(dateTimeInTimezone);

			var timeField = isChangingStart ? 'Start' : 'End';
			var timeReverseField = isChangingStart ? 'End' : 'Start';
			var coverCompletelyMethod = isChangingStart ? 'isBefore' : 'isAfter';
			var coverMethod = isChangingStart ? 'isSameOrBefore' : 'isSameOrAfter';
			var doUpdate = isChangingStart ? updateStart : updateEnd;
			var doUpdateSelf = isChangingStart ? updateEnd : updateStart;

			if (dateTime == mergedSelectedShiftLayer[timeField]) return;

			var index = vm.scheduleVm.ShiftLayers.indexOf(selectedShiftLayer);
			var endIndex = isChangingStart ? 0 : vm.scheduleVm.ShiftLayers.length - 1;
			var step = isChangingStart ? -1 : 1;
			var reverseStep = isChangingStart ? 1 : -1;

			var isLayerShorten = moment.tz(mergedSelectedShiftLayer[timeField], vm.timezone)[coverCompletelyMethod](dateTimeInTimezone);
			var layersCopy = vm.scheduleVm.ShiftLayers.slice(0);
			var actualDateTime = dateTime;

			if (index !== endIndex) {
				if (isLayerShorten) {
					if (!fillWithLayer(index, mergedSelectedShiftLayer.ActivityId, dateTime, isChangingStart)) {
						actualDateTime = mergedSelectedShiftLayer[timeField];
					}
				} else {
					var hasGonePassTopActivity = false;
					var firstTimeGoPassTop = true;
					var currentIndex = index + step;
					var newLayerTime = "";
					while (currentIndex !== endIndex + step) {
						var i = currentIndex;
						currentIndex += step;

						var orginalLayer = layersCopy[i];
						var orginalIndex = vm.scheduleVm.ShiftLayers.indexOf(orginalLayer);
						var layer = vm.scheduleVm.ShiftLayers[orginalIndex];
						var mergedLayer = vm.getMergedShiftLayer(layer);

						if (moment.tz(mergedLayer[timeReverseField], vm.timezone)[coverMethod](dateTimeInTimezone))
							break;

						var layerTimeInTimezone = moment.tz(mergedLayer[timeField], vm.timezone);
						var isCoveredCompletely = dateTimeInTimezone[coverMethod](layerTimeInTimezone);
						var isSameType = mergedLayer.ActivityId === mergedSelectedShiftLayer.ActivityId;
						var deleteIndex = orginalIndex;

						if (layer.FloatOnTop) {
							hasGonePassTopActivity = dateTimeInTimezone[coverCompletelyMethod](layerTimeInTimezone);
							if (firstTimeGoPassTop) {
								firstTimeGoPassTop = false;
								actualDateTime = mergedLayer[timeReverseField];
							}
							newLayerTime = mergedLayer[timeField];
							continue;
						}

						if (hasGonePassTopActivity) {
							var besideLayer = vm.scheduleVm.ShiftLayers[orginalIndex + reverseStep];
							var mergedBesideLayer = !!besideLayer && vm.getMergedShiftLayer(besideLayer);
							var needMerge = !!mergedBesideLayer && mergedBesideLayer.ActivityId === mergedSelectedShiftLayer.ActivityId;
							var layerActualTime = isCoveredCompletely && i !== endIndex ? mergedLayer[timeField] : dateTime;

							if (needMerge) {
								doUpdate(besideLayer, layerActualTime);
								vm.selectedShiftLayers.push(besideLayer);
							} else if (isSameType && i === endIndex && isCoveredCompletely) {
								doUpdate(layer, dateTime);
								vm.selectedShiftLayers.push(layer);
								break;
							} else if (!isSameType) {
								var start = isChangingStart ? layerActualTime : newLayerTime;
								var end = isChangingStart ? newLayerTime : layerActualTime;

								deleteIndex = isChangingStart ? orginalIndex : orginalIndex + 1;
								var insertIndex = isChangingStart ? orginalIndex + 1 : orginalIndex;
								createLayer(mergedSelectedShiftLayer, start, end, insertIndex);
							} else if (isSameType) {
								vm.selectedShiftLayers.push(layer);
							} 
						}

						if (isCoveredCompletely) {
							if (!isSameType || (isSameType && i === endIndex)) {
								vm.scheduleVm.ShiftLayers.splice(deleteIndex, 1);
							}
						} else if (!isSameType) {
							doUpdateSelf(layer, dateTime);
							break;
						}
					}
				}
			}

			doUpdate(selectedShiftLayer, actualDateTime);

			if (isChangingStart) {
				vm.selectedShiftLayers.reverse();
			}
		}

		function fillWithLayer(selectedIndex, selectedActivityId, dateTime, isChangingStart) {
			var step = isChangingStart ? -1 : 1;
			var besideLayer = vm.scheduleVm.ShiftLayers[selectedIndex + step];
			var secondLayer = vm.scheduleVm.ShiftLayers[selectedIndex + step * 2];

			var mergedBesideLayer = vm.getMergedShiftLayer(besideLayer);
			var doUpdateBeside = isChangingStart ? updateEnd : updateStart;

			if (besideLayer.FloatOnTop) {
				if (secondLayer && secondLayer.ActivityId !== selectedActivityId) {
					var startTime = isChangingStart ? mergedBesideLayer.End : dateTime;
					var endTime = isChangingStart ? dateTime : mergedBesideLayer.Start;
					var insertIndex = isChangingStart ? selectedIndex : selectedIndex + 1;
					createLayer(secondLayer, startTime, endTime, insertIndex);
					return true;
				}
			}
			else if (mergedBesideLayer.ActivityId !== selectedActivityId) {
				doUpdateBeside(besideLayer, dateTime);
				return true;
			}
			return false;
		}

		function updateLayer(shiftLayer, startTime, endTime) {
			shiftLayer.Current = shiftLayer.Current || {};
			shiftLayer.Current.Start = startTime;
			shiftLayer.Current.End = endTime;
			shiftLayer.Current.TimeSpan = getTimeSpan(startTime, endTime);
		}

		function getTimeSpan(startTime, endTime) {
			return moment.tz(startTime, vm.timezone).format('L LT') +
				' - ' +
				moment.tz(endTime, vm.timezone).format('L LT');
		}

		function resizeLayer(shiftLayer, x, width, afterResize) {
			shiftLayer.Current = shiftLayer.Current || {};

			shiftLayer.Current.Width = width;
			shiftLayer.Current.TranslateX = x || 0;

			afterResize && afterResize(x, width, shiftLayer.Left);
		}

		function updateStart(layer, dateTime) {
			updateLayer(layer, dateTime, vm.getMergedShiftLayer(layer).End);
			resizeLayer(layer, getDiffMinutes(layer.Current.Start, layer.Start),
				getDiffMinutes(layer.Current.End, layer.Current.Start));
		}

		function updateEnd(layer, dateTime) {
			updateLayer(layer, vm.getMergedShiftLayer(layer).Start, dateTime);
			resizeLayer(layer, getDiffMinutes(layer.Current.Start, layer.Start),
				getDiffMinutes(layer.Current.End, layer.Current.Start));
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

			vm.selectedShiftLayers.push(newLayer);
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
			vm.selectedShiftLayers = [];
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
