'use stirct';

(function() {
	function seatPlanReportPrint() {

		function link(scope, element, attrs) {

			function addContainerOverflowClass(element, styleClass) {
				element[0].parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.classList.add(styleClass);
			}
			function removeContainerOverflowClass(element, styleClass) {
				element[0].parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.classList.remove(styleClass);
			}

			element.on('click', function () {

				scope.vm.getSeatBookings({
					isLoadingReportToDisplay: false,
					callback: function (data) {
						scope.vm.seatBookingsAll = data.SeatBookingsByDate;
						angular.element(document).ready(function () {
							addContainerOverflowClass(element, 'container-to-print-report');
							window.print();
							removeContainerOverflowClass(element, 'container-to-print-report');
						});
					}
				});
			});
		}

		return {
			link: link,
			restrict: 'A'
		};
	}

	angular.module('wfm.seatPlan').directive('seatPlanReportPrint', seatPlanReportPrint);

})();