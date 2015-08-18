'use stirct';

(function() {
	function seatPlanReportPrint() {

		var printSectionDiv = document.createElement('div'), printElementId;

		function getAndPrintElement() {

			var elemToPrint = document.getElementById(printElementId);
			if (elemToPrint) {

				printSectionDiv = elemToPrint.cloneNode(true);
				printSectionDiv.id = "seatPlanReportContentPrintDiv";
				document.body.appendChild(printSectionDiv);
				window.print();
				if (document.getElementById('seatPlanReportContentPrintDiv')) {
					document.body.removeChild(printSectionDiv);
				}
			}
		}

		function link(scope, element, attrs) {

			//element.on('click', function () {

				scope.vm.getSeatBookings({
					isLoadingReportToDisplay: false,
					callback: function (data) {
						scope.vm.seatBookingsAll = data.SeatBookingsByDate;
						printElementId = attrs.printElementId;
						angular.element(document).ready(function () {


							
							//element[0].parentElement.parentElement.parentElement.parentElement.parentElement.style.overflow = 'inherit';

							element[0].parentElement.nextElementSibling.nextElementSibling.nextElementSibling.style.height = 'auto';

							window.print();
						});
					}
				//});


				//element[0].parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.classList.add('container-noprint');

				//if (element[0].parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.classList.contains('container-noprint')) {
				//	scope.vm.getSeatBookings({
				//		isLoadingReportToDisplay: false,
				//		callback: function (data) {
				//			scope.vm.seatBookingsAll = data.SeatBookingsByDate;
				//			printElementId = attrs.printElementId;
				//			angular.element(document).ready(function () {
				//				getAndPrintElement();
				//				element[0].parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.classList.remove('container-noprint');
				//			});
				//		}
				//	});
				//}
			});
		}

		return {
			link: link,
			restrict: 'A'
		};
	}

	angular.module('wfm.seatPlan').directive('seatPlanReportPrint', seatPlanReportPrint);;

})();