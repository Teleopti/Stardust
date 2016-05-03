(function() {
	"use strict";

	angular.module('wfm.requests').service('RequestsFilter', [
			'requestsDataService',
			function (requestsDataSvc) {
				var vm = this;

				vm.AllRequestStatus = requestsDataSvc.getAllRequestStatuses();

				requestsDataSvc.getRequestableAbsences().then(function (result) {
					vm.AllRequestableAbsences = result.data;
				});

				function getTextDescription(properties, selectedItemIds) {
					var description = "";
					if (properties == undefined) {
						return description;
					}

					angular.forEach(selectedItemIds, function(itemId) {
						for (var i = 0; i < properties.length; i++) {
							var property = properties[i];
							if (property.Id === itemId) {
								description = description + property.Name + ", ";
								break;
							}
						};
					});

					return description.length > 2 ? description.substring(0, description.length - 2) : "";
				}

				vm.GetSelectedAbsenceDescription = function (selectedAbsences) {
					console.log();
					return getTextDescription(vm.AllRequestableAbsences, selectedAbsences);
				}

				vm.GetSelectedStatusDescription = function (selectedStatus) {
					return getTextDescription(vm.AllRequestStatus, selectedStatus);
				}
			}
		]
	);
})();