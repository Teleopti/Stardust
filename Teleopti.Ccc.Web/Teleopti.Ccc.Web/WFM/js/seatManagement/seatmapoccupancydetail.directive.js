'use strict';

(function () {

	angular.module('wfm.seatPlan').controller('SeatMapOccupancyCtrl', seatMapOccupancyDirectiveController);
	seatMapOccupancyDirectiveController.$inject = ['seatMapCanvasUtilsService', 'seatPlanService', 'seatMapService', 'growl', 'seatMapTranslatorFactory'];

	function seatMapOccupancyDirectiveController(utils, seatPlanService, seatMapService, growl, seatmapTranslator) {
		var vm = this;

		vm.selectedPeople = [];
		vm.showPeopleSelection = false;
		vm.hasOpenedPersonSelectionPanel = false;
		vm.previousSelectedSeatIds = [];

		vm.asignAgentsToSeats = function () {
			var selectedDay = moment(vm.scheduleDate).format("YYYY-MM-DD");
			seatPlanService.seatPlan.add({ StartDate: selectedDay, EndDate: selectedDay, PersonIds: vm.selectedPeople, SeatIds: vm.previousSelectedSeatIds, locations: [vm.parentVm.seatMapId] })
									.$promise.then(function (seatPlanResultMessage) {
										onSeatPlanCompleted(seatPlanResultMessage);
									});
		};
		vm.getDisplayTime = function (booking) {
			return utils.getSeatBookingTimeDisplay(booking);
		};

		vm.deleteSeatBooking = function (booking) {
			seatMapService.occupancy.remove({ Id: booking.BookingId }).$promise.then(function () {
				vm.refreshSeatMap();
				var deleteSuccessMessage = seatmapTranslator.TranslatedStrings['SeatBookingDeletedSuccessfully'].replace('{0}', booking.SeatName).replace('{1}', booking.FirstName + ' ' + booking.LastName);

				onSuccessShowMessage(deleteSuccessMessage);
			});
		};

		vm.showPersonSelectionPanel = function () {

			if (vm.hasOpenedPersonSelectionPanel) {
				vm.resetPersonSearch();
			}

			vm.showPeopleSelection = true;
			vm.hasOpenedPersonSelectionPanel = true;
		};

		vm.getSeatBookingDetailClass = function (booking) {
			var belongsToDateMoment = moment(booking.BelongsToDate.Date);
			var scheduleDateMoment = moment(vm.scheduleDate);

			if (!belongsToDateMoment.isSame(scheduleDateMoment, 'day')) {
				return 'seatmap-seatbooking-previousday';
			}

			return '';
		};

		function onSeatPlanCompleted(seatPlanResultMessage) {

			var seatPlanResultDetailMessage = seatmapTranslator.TranslatedStrings['SeatPlanResultDetailMessage']
						.replace('{0}', seatPlanResultMessage.NumberOfBookingRequests)
						.replace('{1}', seatPlanResultMessage.RequestsGranted)
						.replace('{2}', seatPlanResultMessage.RequestsDenied)
						.replace('{3}', seatPlanResultMessage.NumberOfUnscheduledAgentDays);

			if (seatPlanResultMessage.RequestsDenied > 0) {
				onWarningShowMessage(seatPlanResultDetailMessage);
			}
			else {
				onSuccessShowMessage(seatPlanResultDetailMessage);
			}

			vm.refreshSeatMap();
		};

		function setupObjectSelectionHandlers() {
			var onObjectSelection = function (e) {

				var object = e.target;

				var objIsGroup = (object.get('type') == 'group');

				object.hasRotatingPoint = false;
				object.hasControls = false;
				if (objIsGroup) {

					object.hasBorders = false;
					var seats = utils.getObjectsOfTypeFromGroup(object, 'seat');
					if (seats.length > 0) {
						loadOccupancyForSeats(seats);
						vm.parentVm.rightPanelOption.panelState = true;
					}
				} else {
					if (object.get('type') == 'seat') {
						loadOccupancyForSeats([object]);
						vm.parentVm.rightPanelOption.panelState = true;
					}
				}

				vm.showPeopleSelection = false;
			};

			canvas().on('object:selected', function (e) {
				onObjectSelection(e);
			});

			canvas().on('selection:created', function (e) {
				if (e.e && e.e.shiftKey) {
					onObjectSelection(e);
				}
			});

			canvas().on('before:selection:cleared', function (e) {
				var unselectedCount = 0;
				if (e.target._objects) {
					unselectedCount = e.target._objects.length;
				} else {
					unselectedCount = 1;
				}

				var count = 0;
				canvas().getObjects().forEach(function (obj) {
					if (obj.active) {
						count++;
					}
				});

				if (count == unselectedCount) {
					vm.occupancyDetails = undefined;
					vm.previousSelectedSeatIds = [];
				}
				vm.parentVm.rightPanelOption.panelState = false;
			});

		};

		function loadOccupancyForSeats(seats) {
			vm.previousSelectedSeatIds = [];
			seats.forEach(function (seat) {
				vm.previousSelectedSeatIds.push(seat.id);
			});

			utils.loadOccupancyDetailsForSeats(vm.previousSelectedSeatIds, vm.scheduleDate).then(function (result) {
				onSeatOccupancyLoaded(result, seats);
			});
		};

		function onSeatOccupancyLoaded(occupancyDetails, seats) {
			var sortedSeats = seats.sort(seatCanvasPositionComparer);
			vm.occupancyDetails = [];
			sortedSeats.forEach(function (seat) {
				if (occupancyDetails && occupancyDetails.length) {
					var foundOccupancyDetail = false;
					for (var detailIdx = 0 ; detailIdx < occupancyDetails.length; detailIdx++) {
						if (occupancyDetails[detailIdx].SeatId == seat.id) {
							vm.occupancyDetails.push(occupancyDetails[detailIdx]);
							foundOccupancyDetail = true;
							break;
						}
					};

					if (!foundOccupancyDetail) {
						createEmptyOccupancyDetail(seat);
					}
				} else {
					createEmptyOccupancyDetail(seat);
				}
			});

		};

		function createEmptyOccupancyDetail(seat) {
			vm.occupancyDetails.push({
				SeatName: seat.name, SeatId: seat.id,
				Occupancies: [{ IsEmpty: true }]
			});
		};

		function seatCanvasPositionComparer(seatA, seatB) {
			return (seatA.left + seatA.top) - (seatB.left + seatB.top);
		};

		function canvas() {
			return vm.parentVm.getCanvas();
		};

		vm.selectSeat = function() {
			if (vm.previousSelectedSeatIds.length > 0) {
				if (vm.previousSelectedSeatIds.length == 1) {
					var seat = utils.getSeatObjectById(canvas(), vm.previousSelectedSeatIds[0]);
					if (seat != null) {
						canvas().setActiveObject(seat);
					} else {
						vm.occupancyDetails = undefined;
					}
				} else {
					var seatObjects = [];
					vm.previousSelectedSeatIds.forEach(function (seatId) {
						var seatObj = utils.getSeatObjectById(canvas(), seatId);
						if (seatObj != null)
							seatObjects.push(seatObj);
					});

					utils.selectGroupOfObjects(canvas(), seatObjects);
				}
			} else {

				var seat = utils.getSeatObject(canvas(), true);

				if (seat != null) {	
					canvas().setActiveObject(seat);
				} else {
					vm.occupancyDetails = undefined;
				}
			}
		}

		function onSuccessShowMessage(message) {
			growl.success("<i class='mdi mdi-thumb-up'></i> " + message + ".", {
				ttl: 5000,
				disableCountDown: true
			});
		};

		function onWarningShowMessage(message) {
			growl.warning("<i class='mdi mdi-alert'></i> " + message + ".", {
				ttl: 5000,
				disableCountDown: true
			});
		};

		vm.init = function () {
			setupObjectSelectionHandlers();

			canvas().on('seatmaplocation:loaded', function (e) {
				utils.ungroupObjectsSoTheyCanBeIndividuallySelected(canvas());
				utils.showSeatBooking(canvas(), e.data.Seats);
				vm.selectSeat();
			});
		}

	};

})();


(function () {
	angular.module('wfm.seatMap').directive('seatmapOccupancyDetail', seatmapOccupancyDetailDirective);

	function linkFunction(scope, element, attributes, controllers) {
		var vm = controllers[0];
		vm.parentVm = controllers[1];
		vm.rightPanelVm = controllers[2];
		
		vm.parentVm.rightPanelOption.panelTitle = "Seat Information";//need to translate

		vm.init();
	};

	function seatmapOccupancyDetailDirective() {
		return {
			controller: 'SeatMapOccupancyCtrl',
			controllerAs: 'vm',
			bindToController: true,
			require: ['seatmapOccupancyDetail', '^seatmapCanvas', '^seatmapRightPanel'],
			scope: {
				scheduleDate: '=',
				refreshSeatMap: '&'
			},
			restrict: "E",
			templateUrl: "js/seatManagement/html/seatmapoccupancylist.html",
			link: linkFunction
		};
	};

})();