
(function () {
	'use strict';

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
		'$state',
		'$translate',
		'TeamSchedule',
		'serviceDateFormatHelper',
		'ShiftEditorViewModelFactory',
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
		$state,
		$translate,
		TeamSchedule,
		serviceDateFormatHelper,
		ShiftEditorViewModelFactory,
		ActivityService,
		ShiftEditorService,
		CurrentUserInfo,
		guidgenerator,
		signalRSVC,
		NoticeService
	) {
		var doNotToggleSelectionAfterResizeEnd = false;

		var vm = this;
		var timeLineTimeRange = {
			Start: moment.tz(vm.date, vm.timezone).add(-1, 'days').hours(0),
			End: moment.tz(vm.date, vm.timezone).add(3, 'days').hours(0)
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
			vm.timelineVmWidth = getDiffMinutes(timeLineTimeRange.End, timeLineTimeRange.Start);
			
			subscribeToScheduleChange();

			bindResizeLayerEvent();
		};

		vm.$onDestroy = function () {
			interact('.shift-layer').unset();
		};

		vm.gotoDayView = function () {
			$state.go('teams.dayView');
		};

		vm.isSameDate = function (interval) {
			return moment.tz(vm.date, vm.timezone).isSame(interval.Time, 'days');
		};

		vm.scroll = function (step) {
			var timelineViewportEl = $element[0].querySelector('.timeline').parentElement;
			scrollTo(timelineViewportEl.scrollLeft + step);
		};

		vm.isNotAllowedToChange = function (shiftLayer) {
			return !!shiftLayer.IsNew ? false : !shiftLayer.ShiftLayerIds || !shiftLayer.ShiftLayerIds.length;
		};

		vm.toggleSelection = function (shiftLayer, $event) {
			if (vm.isNotAllowedToChange(shiftLayer)) return;

			if (vm.isOnDesktop && doNotToggleSelectionAfterResizeEnd) {
				doNotToggleSelectionAfterResizeEnd = false;
				$event.preventDefault();
				return;
			}
			vm.selectedShiftLayers = vm.selectedShiftLayers.indexOf(shiftLayer) === -1 ? [shiftLayer] : [];
			vm.selectedActivitiyId = vm.getMergedShiftLayer(shiftLayer).ActivityId;
		};

		vm.useLighterBorder = function (shiftLayer) {
			var sl = vm.getMergedShiftLayer(shiftLayer);
			return shiftLayer.UseLighterBorder(sl.Color);
		};

		vm.changeActivityType = function () {
			var selectActivity = vm.availableActivities.filter(function (activity) {
				return vm.selectedActivitiyId == activity.Id;
			})[0];

			vm.selectedShiftLayers.forEach(function (layer) {
				layer.Current = layer.Current || {};
				layer.Current.Color = selectActivity.Color;
				layer.Current.Description = selectActivity.Name;
				layer.Current.ActivityId = selectActivity.Id;
				if (!isNotRegularActivity(layer)) {
					layer.FloatOnTop = selectActivity.FloatOnTop;
				}
				mergeBesideLayer(layer);
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
			var lastIndex = vm.selectedShiftLayers.length - 1;
			var startTime = vm.getMergedShiftLayer(vm.selectedShiftLayers[0]).Start;
			var endTime = vm.getMergedShiftLayer(vm.selectedShiftLayers[lastIndex]).End;

			return getTimeSpan(startTime, endTime);
		};

		vm.getMergedShiftLayer = function (layer) {
			return angular.extend({}, layer, layer.Current);
		}

		vm.isNotResizable = isNotRegularActivity;

		vm.isOnDesktop = isOnDesktop();

		vm.onResizeMove = function (event) {
			var left = event.deltaRect.left;
			var width = event.rect.width;
			var index = parseInt(event.target.attributes.index.value);

			var curShiftLayer = vm.scheduleVm.ShiftLayers[index];

			var mergedShiftLayer = vm.getMergedShiftLayer(curShiftLayer);
			var lastWidth = mergedShiftLayer.Width;
			var translateX = (mergedShiftLayer.TranslateX || 0) + left;
			resizeLayer(curShiftLayer, translateX, width);

			var isChangingStart = left != 0;
			scrollToProperPosition(curShiftLayer, translateX, width, lastWidth, isChangingStart);

			$scope.$apply();
		};

		vm.onResizeEnd = function (event) {
			var curShiftLayer = vm.scheduleVm.ShiftLayers[parseInt(event.target.attributes.index.value)];
			var mergedShiftLayer = vm.getMergedShiftLayer(curShiftLayer);

			var originStartMinutes = mergedShiftLayer.Left + mergedShiftLayer.TranslateX;
			var originStartTime = timeLineTimeRange.Start.clone().add(originStartMinutes, 'minutes');

			var startMinutes = mergedShiftLayer.Left + round5(mergedShiftLayer.TranslateX);
			var startTime = timeLineTimeRange.Start.clone().add(startMinutes, 'minutes');

			var isChangingStart = serviceDateFormatHelper.getDateTime(originStartTime) !== mergedShiftLayer.Start;

			if (isChangingStart) {
				redrawLayers(curShiftLayer, startTime, true);
			} else {
				var width = round5(mergedShiftLayer.Width);
				var endTime = timeLineTimeRange.Start.clone().add(startMinutes + width, 'minutes');
				redrawLayers(curShiftLayer, endTime, false);
			}

			doNotToggleSelectionAfterResizeEnd = true;
			$scope.$apply();
		}

		function isNotRegularActivity(shiftLayer) {
			return !!shiftLayer.IsPersonalActivity
				|| !!shiftLayer.IsMeeting
				|| !!shiftLayer.IsIntradayAbsence
				|| !!shiftLayer.IsOvertime;
		}

		function canNotFillWith(layer) {
			return layer.FloatOnTop || vm.isNotResizable(layer);
		}

		function isOnDesktop() {
			var userAgent = navigator.userAgent || navigator.vendor || window.opera;
			return !(!!userAgent.match(/ipad|iphone|ipod/i) || !!userAgent.match(/android/i));
		}

		function bindResizeLayerEvent() {
			interact('.shift-layer')
				.resizable({
					allowFrom: '.selected:not(.non-resizable)',
					edges: { left: true, right: true },
					restrictSize: {
						min: { width: 5 }
					}
				})
				.on('resizemove', vm.onResizeMove)
				.on('resizeend', vm.onResizeEnd);
		}

		function isExceedMaxLength(startTime, endTime) {
			return getDiffMinutes(endTime, startTime) > 36 * 60;
		}

		function redrawLayers(selectedShiftLayer, dateTimeInTimezone, isChangingStart) {
			var originalShiftLayers = vm.scheduleVm.ShiftLayers.slice(0);
			var originalSelectedShiftLayer = originalShiftLayers[originalShiftLayers.indexOf(selectedShiftLayer)];
			var selectedLayers = [originalSelectedShiftLayer];

			var mergedSelectedShiftLayer = vm.getMergedShiftLayer(originalSelectedShiftLayer);
			var dateTime = serviceDateFormatHelper.getDateTime(dateTimeInTimezone);

			var timeField = isChangingStart ? 'Start' : 'End';
			var timeReverseField = isChangingStart ? 'End' : 'Start';
			var coverCompletelyMethod = isChangingStart ? 'isBefore' : 'isAfter';
			var coverMethod = isChangingStart ? 'isSameOrBefore' : 'isSameOrAfter';
			var doUpdate = isChangingStart ? updateStart : updateEnd;
			var doUpdateSelf = isChangingStart ? updateEnd : updateStart;
			var step = isChangingStart ? -1 : 1;
			var reverseStep = isChangingStart ? 1 : -1;

			if (dateTime == mergedSelectedShiftLayer[timeField]) {
				actualDateTime = mergedSelectedShiftLayer[timeField];
			} else {


				var index = originalShiftLayers.indexOf(originalSelectedShiftLayer);
				var lastLayerIndex = originalShiftLayers.length - 1;
				var endIndex = isChangingStart ? 0 : lastLayerIndex;

				var isLayerShorten = moment.tz(mergedSelectedShiftLayer[timeField], vm.timezone)[coverCompletelyMethod](dateTimeInTimezone);
				var layersCopy = originalShiftLayers.slice(0);
				var actualDateTime = dateTime;

				var shiftStart = isChangingStart ? dateTime : vm.getMergedShiftLayer(originalShiftLayers[0]).Start;
				var shiftEnd = isChangingStart ? vm.getMergedShiftLayer(originalShiftLayers[lastLayerIndex]).End : dateTime;

				if (isChangingStart && !isSameDate(dateTime)
					|| (!isLayerShorten && isExceedMaxLength(shiftStart, shiftEnd))) {
					actualDateTime = mergedSelectedShiftLayer[timeField];
				}
				else if (index !== endIndex) {
					if (isLayerShorten) {
						if (!fillWithLayer(originalShiftLayers, index, mergedSelectedShiftLayer, dateTime, isChangingStart)) {
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
							var orginalIndex = originalShiftLayers.indexOf(orginalLayer);
							var layer = originalShiftLayers[orginalIndex];
							var mergedLayer = vm.getMergedShiftLayer(layer);

							if (moment.tz(mergedLayer[timeReverseField], vm.timezone)[coverMethod](dateTimeInTimezone))
								break;

							var layerTimeInTimezone = moment.tz(mergedLayer[timeField], vm.timezone);
							var isCoveredCompletely = dateTimeInTimezone[coverMethod](layerTimeInTimezone);
							var isSameTypeWithSelected = isSameType(mergedLayer, mergedSelectedShiftLayer);
							var deleteIndex = orginalIndex;
							if (layer.FloatOnTop && !isSameTypeWithSelected) {
								hasGonePassTopActivity = dateTimeInTimezone[coverCompletelyMethod](layerTimeInTimezone);
								if (firstTimeGoPassTop) {
									firstTimeGoPassTop = false;
									actualDateTime = mergedLayer[timeReverseField];
								}
								newLayerTime = mergedLayer[timeField];
								if (hasGonePassTopActivity && i == endIndex) {
									var start = isChangingStart ? dateTime : newLayerTime;
									var end = isChangingStart ? newLayerTime : dateTime;
									var insertIndex = isChangingStart ? orginalIndex : orginalIndex + 1;
									var newLayer = createLayer(originalShiftLayers, originalSelectedShiftLayer, start, end, insertIndex);
									selectedLayers.push(newLayer);
								}
								continue;
							}

							if (hasGonePassTopActivity) {
								var previousLayer = originalShiftLayers[orginalIndex + reverseStep];
								var nextLayer = originalShiftLayers[orginalIndex + step];
								var mergedBesideLayer = !!previousLayer && vm.getMergedShiftLayer(previousLayer);
								var needMerge = !!mergedBesideLayer && isSameType(mergedBesideLayer, mergedSelectedShiftLayer);
								var layerActualTime = isCoveredCompletely && i !== endIndex ? (nextLayer.IsOvertime ? dateTime : mergedLayer[timeField]) : dateTime;

								if (needMerge) {
									doUpdate(previousLayer, layerActualTime);
									selectedLayers.push(previousLayer);
								} else if (isSameTypeWithSelected && i === endIndex && isCoveredCompletely) {
									doUpdate(layer, dateTime);
									selectedLayers.push(layer);
									break;
								} else if (!isSameTypeWithSelected) {
									deleteIndex = isChangingStart ? orginalIndex : orginalIndex + 1;

									var start = isChangingStart ? layerActualTime : newLayerTime;
									var end = isChangingStart ? newLayerTime : layerActualTime;
									var insertIndex = isChangingStart ? orginalIndex + 1 : orginalIndex;
									var newLayer = createLayer(originalShiftLayers, mergedSelectedShiftLayer, start, end, insertIndex);
									selectedLayers.push(newLayer);
								} else if (isSameTypeWithSelected) {
									selectedLayers.push(layer);
								}
							}

							if (isCoveredCompletely) {
								if (!isSameTypeWithSelected
									|| (isSameTypeWithSelected && i === endIndex)
									|| (isSameTypeWithSelected && needMerge)) {
									originalShiftLayers.splice(deleteIndex, 1);
								}
							} else {
								if (!isSameTypeWithSelected) {
									doUpdateSelf(layer, dateTime);
								} else if (isSameTypeWithSelected && !hasGonePassTopActivity) {
									actualDateTime = mergedLayer.End;
									originalShiftLayers.splice(deleteIndex, 1);
								}
								break;
							}
						}

						if (isChangingStart) {
							selectedLayers.reverse();
						}
					}
				}
			}
			doUpdate(originalSelectedShiftLayer, actualDateTime);
			vm.selectedShiftLayers = selectedLayers;
			vm.scheduleVm.ShiftLayers = originalShiftLayers;
		}
		function isSameDate(dateTime) {
			return serviceDateFormatHelper.getDateOnly(moment.tz(dateTime, vm.timezone).tz(vm.scheduleVm.Timezone)) === vm.date;
		}

		function fillWithLayer(shiftLayers, selectedIndex, selectedLayer, dateTime, isChangingStart) {
			var step = isChangingStart ? -1 : 1;
			var besideLayer = shiftLayers[selectedIndex + step];
			var secondLayer = shiftLayers[selectedIndex + step * 2];

			var mergedBesideLayer = vm.getMergedShiftLayer(besideLayer);
			var doUpdateBeside = isChangingStart ? updateEnd : updateStart;

			if (besideLayer.FloatOnTop) {
				if (secondLayer
					&& !isSameType(secondLayer, selectedLayer)
					&& !canNotFillWith(secondLayer)) {
					var startTime = isChangingStart ? mergedBesideLayer.End : dateTime;
					var endTime = isChangingStart ? dateTime : mergedBesideLayer.Start;
					var insertIndex = isChangingStart ? selectedIndex : selectedIndex + 1;
					createLayer(shiftLayers, secondLayer, startTime, endTime, insertIndex);
					return true;
				}

				if (!secondLayer && besideLayer.IsOvertime) return true;
			}
			else if (!isSameType(mergedBesideLayer, selectedLayer)) {
				doUpdateBeside(besideLayer, dateTime);
				return true;
			}
			return false;
		}

		function mergeBesideLayer(layer) {
			var mergedLayer = vm.getMergedShiftLayer(layer);
			var index = vm.scheduleVm.ShiftLayers.indexOf(layer);
			var previousLayer = vm.scheduleVm.ShiftLayers[index - 1];
			var nextLayer = vm.scheduleVm.ShiftLayers[index + 1];
			var deleteIndex = index + 1;

			if (previousLayer && isSameType(mergedLayer, previousLayer)) {
				updateStart(layer, vm.getMergedShiftLayer(previousLayer).Start);
				vm.scheduleVm.ShiftLayers.splice(index - 1, 1);
				deleteIndex = index;
			}

			if (nextLayer && isSameType(mergedLayer, nextLayer)) {
				updateEnd(layer, vm.getMergedShiftLayer(nextLayer).End);
				vm.scheduleVm.ShiftLayers.splice(deleteIndex, 1);
			}
		}

		function isSameType(layer, besideLayer) {
			var mergedLayer = vm.getMergedShiftLayer(layer);
			var mergedBesideLayer = vm.getMergedShiftLayer(besideLayer);

			return mergedLayer.ActivityId == mergedBesideLayer.ActivityId
				&& mergedLayer.IsOvertime == mergedBesideLayer.IsOvertime
				&& mergedLayer.IsPersonalActivity == mergedBesideLayer.IsPersonalActivity
				&& mergedLayer.IsMeeting == mergedBesideLayer.IsMeeting;
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

		function resizeLayer(shiftLayer, x, width) {
			shiftLayer.Current = shiftLayer.Current || {};
			shiftLayer.Current.Width = width;
			shiftLayer.Current.TranslateX = x || 0;
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

		function createLayer(shiftLayers, layer, startTime, endTime, insertIndex) {
			var newLayer = ShiftEditorViewModelFactory.CopyToNewLayer(vm.getMergedShiftLayer(layer), startTime, endTime);

			resizeLayer(newLayer, 0, getDiffMinutes(newLayer.End, newLayer.Start));
			newLayer.Left = getShiftLayerLeft(newLayer);
			newLayer.IsNew = true;

			shiftLayers.splice(insertIndex, 0, newLayer);
			return newLayer;
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
				vm.isLoading = false;

				vm.scheduleVm = ShiftEditorViewModelFactory.CreateSchedule(vm.date, vm.timezone, data.Schedules[0]);
				vm.isInDifferentTimezone = vm.scheduleVm.Timezone !== vm.timezone;

				if (!!vm.scheduleVm.ShiftLayers) {
					vm.scheduleVm.ShiftLayers.forEach(function (layer) {
						layer.Width = getShiftLayerWidth(layer);
						layer.Left = getShiftLayerLeft(layer);
					});

					initAndBindScrollEvent();
				}
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
			var timelineEl = $element[0].querySelector('.timeline');

			if (!hasShift()) return;

			initScrollState();
			angular.element($window).bind('resize', initScrollState);

			bindScrollEvent(timelineEl.querySelector('.left-scroll'), -20);
			bindScrollEvent(timelineEl.querySelector('.right-scroll'), 20);

			timelineEl.parentElement.addEventListener('scroll', function () {
				var timelineViewportEl = $element[0].querySelector('.timeline').parentElement;
				var shiftViewportEl = $element[0].querySelector('.shift').parentElement;
				shiftViewportEl.scrollLeft = timelineViewportEl.scrollLeft;
			});

			timelineEl.addEventListener('keydown', function (e) {
				e.preventDefault();
			});
		}

		var scrollIntervalPromise = null;
		function cancelScrollIntervalPromise() {
			if (scrollIntervalPromise) {
				$interval.cancel(scrollIntervalPromise);
				scrollIntervalPromise = null;
			}
		}

		function bindScrollEvent(el, step, eventPrefix) {
			el.addEventListener(
				'mousedown',
				function (e) {
					cancelScrollIntervalPromise();
					scrollIntervalPromise = $interval(function () {
						vm.scroll(angular.isFunction(step) ? step(e) : step);
					}, 150);
				},
				false
			);
			el.addEventListener('mouseup', cancelScrollIntervalPromise, false);
			el.addEventListener('mouseleave', cancelScrollIntervalPromise, false);
		}

		function initScrollState() {
			$timeout(function () {
				var shiftViewportEl = $element[0].querySelector('.shift').parentElement;
				var shiftTimeRange = getShiftTimeRangeInMinutes();
				var shiftLength = shiftTimeRange.End - shiftTimeRange.Start;

				var scrollLeft =
					shiftViewportEl.clientWidth <= shiftLength
						? shiftTimeRange.Start - 120
						: shiftTimeRange.Start - (shiftViewportEl.clientWidth - shiftLength) / 2;
				if (scrollLeft <= 0) scrollLeft = 0;

				scrollTo(scrollLeft);
			});
		}

		function getShiftTimeRangeInMinutes() {
			var allLayers = $element[0].querySelectorAll('.shift-layer');
			var firstLayer = allLayers[0];
			var shiftStart = parseInt(firstLayer.style.left) + getTranslateX(firstLayer);
			var lastLayer = allLayers[allLayers.length - 1];
			var shiftEnd = parseInt(lastLayer.style.left) + getTranslateX(lastLayer) + parseInt(lastLayer.style.width);

			return {
				Start: shiftStart,
				End: shiftEnd
			};
		}

		function scrollToProperPosition(shiftLayer, translateX, width, lastWidth, isChangingStart) {
			var viewportEl = $element[0].querySelector('.shift').parentElement;
			var mergedShiftLayer = vm.getMergedShiftLayer(shiftLayer);
			var start = translateX + shiftLayer.Left;
			var end = start + width;
			var viewingEnd = viewportEl.scrollLeft + viewportEl.clientWidth;

			var isExtending = width > lastWidth;
			var changingWidth = width - lastWidth;

			var shiftTimeRange = getShiftTimeRangeInMinutes();

			if (isChangingStart) {
				if ((isExtending && viewportEl.scrollLeft > start)
					|| (!isExtending && shiftTimeRange.End > viewingEnd)) {
					var step = isExtending ? changingWidth + 50 : changingWidth;
					vm.scroll(-step);
				}
			} else {
				if ((isExtending && end > viewingEnd)
					|| (!isExtending && viewportEl.scrollLeft > shiftTimeRange.Start)) {
					var step = isExtending ? changingWidth + 50 : changingWidth;
					vm.scroll(step);
				}
			}
		}

		function getTranslateX(el) {
			var transformRegx = new RegExp(/translate\(\s*(-?[0-9\.\px,\spx)]+)\)/g);
			return parseInt(el.style.transform.replace(transformRegx, function (a, b, c) {
				return b;
			}).split(',')[0]);
		}

		function displayScrollButton() {
			var viewportEl = $element[0].querySelector('.shift').parentElement;
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

		function scrollTo(scrollLeft) {
			var timelineViewportEl = $element[0].querySelector('.timeline').parentElement;
			var shiftViewportEl = $element[0].querySelector('.shift').parentElement;
			timelineViewportEl.scrollLeft = scrollLeft;
			shiftViewportEl.scrollLeft = scrollLeft;
			displayScrollButton();
		}
	}
})();
