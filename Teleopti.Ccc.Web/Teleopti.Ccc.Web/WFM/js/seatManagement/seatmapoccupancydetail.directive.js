'use strict';

(function () {

	angular.module('wfm.seatPlan').controller('SeatMapOccupancyCtrl', seatMapOccupancyDirectiveController);
	seatMapOccupancyDirectiveController.$inject = ['seatMapCanvasUtilsService', 'seatMapService', 'growl', 'seatMapTranslatorFactory'];

	function seatMapOccupancyDirectiveController(utils, seatMapService, growl, seatmapTranslator) {
		var vm = this;
		vm.previousSelectedSeats = [];

		vm.getDisplayTime = function (booking) {
			return utils.getSeatBookingTimeDisplay(booking);
		};

		vm.deleteSeatBooking = function (booking) {
			seatMapService.occupancy.remove({ Id: booking.BookingId }).$promise.then(function () {
				vm.refreshSeatMap();
				var deleteSuccessMessage = seatmapTranslator.TranslatedStrings['SeatBookingDeletedSuccessfully'].replace('{0}', booking.SeatName).replace('{1}', booking.FirstName +' '+ booking.LastName);

				onSuccessDeleteSeatBooking(deleteSuccessMessage);
			});
		};

		vm.getSeatBookingDetailClass = function (booking) {
			var belongsToDateMoment = moment(booking.BelongsToDate.Date);
			var scheduleDateMoment = moment(vm.scheduleDate);

			if (!belongsToDateMoment.isSame(scheduleDateMoment, 'day')) {
				return 'seatmap-seatbooking-previousday';
			}

			return '';
		};

		function setupObjectSelectionHandlers() {
			var onObjectSelection = function (e, handleGroupOnlyAction) {
				var object = e.target;

				var objIsGroup = (object.get('type') == 'group');
				if (objIsGroup) {
					if (!handleGroupOnlyAction) {
						return;
					}
				};

				object.hasRotatingPoint = false;
				object.hasControls = false;
				if (objIsGroup){
					
					object.hasBorders = false;
					var seats = utils.getObjectsOfTypeFromGroup(object, 'seat');
					if (seats.length > 0) {
						loadOccupancyForSeats(seats);
					}

				} else {
					if (object.get('type') == 'seat') {
						loadOccupancyForSeats([object]);
					}
				}
			};

			canvas().on('selection:created', function (e) {
				onObjectSelection(e, true);
			});

			canvas().on('object:selected', function (e) {
				onObjectSelection(e, false);
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
					vm.previousSelectedSeats = [];
				}
			});

		};

		function loadOccupancyForSeats(seats) {
			vm.previousSelectedSeats = seats;
			var seatIds = [];
			seats.forEach(function (seat) {
				seatIds.push(seat.id);
			});

			utils.loadOccupancyDetailsForSeats(seatIds, vm.scheduleDate).then(function (result) {
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

		function selectSeat() {
			if (vm.previousSelectedSeats.length > 0) {
				if (vm.previousSelectedSeats.length == 1) {
					var seat = utils.getSeatObjectById(canvas(), vm.previousSelectedSeats[0].id);
					if (seat != null) {
						canvas().setActiveObject(seat);
					} else {
						vm.occupancyDetails = undefined;
					}
				} else {
					var seatObjects = [];
					vm.previousSelectedSeats.forEach(function (seat) {
						var seatObj = utils.getSeatObjectById(canvas(), seat.id);
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


		function onSuccessDeleteSeatBooking(message) {
			growl.success("<i class='mdi mdi-thumb-up'></i> " + message + ".", {
				ttl: 5000,
				disableCountDown: true
			});
		};


		vm.init = function () {
			setupObjectSelectionHandlers();

			canvas().on('seatmaplocation:loaded', function (e) {
				utils.ungroupObjectsSoTheyCanBeIndividuallySelected(canvas());
				utils.showSeatBooking(canvas(), e.data.Seats);
				selectSeat();
			});
		}

	};

})();


(function () {
	angular.module('wfm.seatMap').directive('seatmapOccupancyDetail', seatmapOccupancyDetailDirective);


	function linkFunction(scope, element, attributes, controllers) {
		var vm = controllers[0];
		var parentVm = controllers[1];
		vm.parentVm = parentVm;
		vm.init();
	};

	function seatmapOccupancyDetailDirective() {
		return {
			controller: 'SeatMapOccupancyCtrl',
			controllerAs: 'vm',
			bindToController: true,
			require: ['seatmapOccupancyDetail', '^seatmapCanvas'],
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