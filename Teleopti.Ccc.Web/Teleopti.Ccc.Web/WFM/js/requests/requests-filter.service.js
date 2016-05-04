(function() {
	"use strict";

	angular.module('wfm.requests').service('RequestsFilter', [
			'requestsDataService',
			function(requestsDataSvc) {
				var vm = this;

				vm.RequestStatuses = requestsDataSvc.getAllRequestStatuses();
				angular.forEach(vm.RequestStatuses, function(status) {
					status.Selected = false;
				});

				requestsDataSvc.getRequestableAbsences().then(function(result) {
					vm.RequestableAbsences = result.data;
					angular.forEach(vm.RequestableAbsences, function(absence) {
						absence.Selected = false;
					});
				});

				vm.SubjectFilter = "";
				vm.MessageFilter = "";

				function getTextDescription(items) {
					var description = "";
					if (items == undefined) {
						return description;
					}

					for (var i = 0; i < items.length; i++) {
						var item = items[i];
						if (item.Selected) {
							description = description + item.Name + ", ";
						}
					};

					return description.length > 2 ? description.substring(0, description.length - 2) : "";
				}

				vm.GetSelectedAbsenceDescription = function() {
					return getTextDescription(vm.RequestableAbsences);
				}

				vm.GetSelectedStatusDescription = function() {
					return getTextDescription(vm.RequestStatuses);
				}

				vm.Filters = [];

				function removeFilter(filterName) {
					for (var i = 0; i < vm.Filters.length; i++) {
						var filter = vm.Filters[i];
						if (filter.hasOwnProperty(filterName)) {
							vm.Filters.splice(i, 1);
						}
					}
				}

				vm.RemoveFilter = function(filterName) {
					removeFilter(filterName);
				}

				vm.SetFilter = function(name, filter) {
					var nameInLowerCase = name.trim().toLowerCase();
					var verifiedFilter = "";
					if (nameInLowerCase === "status") {
						removeFilter("Status");
						if (filter == undefined || filter.trim().length === 0) return;

						var selectedStatuses = filter.trim().split(" ");
						angular.forEach(vm.RequestStatuses, function(status) {
							status.Selected = false;
							for (var i = 0; i < selectedStatuses.length; i++) {
								var selectedStatusId = parseInt(selectedStatuses[i].trim());
								if (status.Id === selectedStatusId) {
									status.Selected = true;
									verifiedFilter = verifiedFilter + selectedStatusId + " ";
								}
							}
						});
						vm.Filters.push({ "Status": verifiedFilter.trim() });
					} else if (nameInLowerCase === "absence") {
						removeFilter("Absence");
						if (filter == undefined || filter.trim().length === 0) return;

						var selectedAbsences = filter.trim().split(" ");
						angular.forEach(vm.RequestableAbsences, function(absence) {
							absence.Selected = false;
							for (var i = 0; i < selectedAbsences.length; i++) {
								var selectedAbsenceId = selectedAbsences[i].trim();
								if (absence.Id === selectedAbsenceId) {
									absence.Selected = true;
									verifiedFilter = verifiedFilter + selectedAbsenceId + " ";
								}
							}
						});
						vm.Filters.push({ "Absence": verifiedFilter.trim() });
					} else if (nameInLowerCase === "subject") {
						removeFilter("Subject");
						if (filter == undefined || filter.trim().length === 0) return;
						vm.Filters.push({ Subject: filter.trim() });
					} else if (nameInLowerCase === "message") {
						removeFilter("Message");
						if (filter == undefined || filter.trim().length === 0) return;
						vm.Filters.push({ Message: filter.trim() });
					};

					vm.ResetFilter = function() {
						vm.Filters = [];
					}
				}
			}
		]
	);
})();