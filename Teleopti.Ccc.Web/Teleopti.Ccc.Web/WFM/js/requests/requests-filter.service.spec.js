"use strict";

describe("[Requests Filter Service Test]", function() {
	var target;

	var absences = [
		{ Id: "00", Name: "Absence0" },
		{ Id: "01", Name: "Absence1" },
		{ Id: "02", Name: "Absence2" },
		{ Id: "03", Name: "Absence3" }
	];

	var statuses = [
		{ Id: 0, Name: "Status0" },
		{ Id: 1, Name: "Status1" },
		{ Id: 2, Name: "Status2" },
		{ Id: 3, Name: "Status3" }
	];

	function mockRequestsDataService() {
		this.getRequestableAbsences = function() {
			return {
				then: function(mock) {
					mock({ data: absences });
				}
			}
		}

		this.getAllRequestStatuses = function() {
			return statuses;
		}
	}

	var $rootScope;

	beforeEach(function() {
		var requestsDataSvc = new mockRequestsDataService();
		module("wfm.requests");
		module(function($provide) {
			$provide.service("requestsDataService", function() { return requestsDataSvc; });
		});
	});

	beforeEach(inject(function(_$rootScope_, _RequestsFilter_) {
		$rootScope = _$rootScope_;
		target = _RequestsFilter_;
	}));

	it("Should get description for selected absences", function() {
		for (var i = 0; i < target.RequestableAbsences.length; i++) {
			var absence = target.RequestableAbsences[i];
			absence.Selected = absence.Id === "00" || absence.Id === "02";
		}
		var description = target.GetSelectedAbsenceDescription();
		expect(description).toEqual("Absence0, Absence2");
	});

	it("Should get description for selected status", function() {
		for (var i = 0; i < target.RequestStatuses.length; i++) {
			var absence = target.RequestStatuses[i];
			absence.Selected = absence.Id === 1 || absence.Id === 2;
		}
		var description = target.GetSelectedStatusDescription();
		expect(description).toEqual("Status1, Status2");
	});

	it("Should update filters on filter changed", function() {
		target.SetFilter("Subject", "CDEF");
		target.SetFilter("Subject", "Abc 123");

		target.SetFilter("Message", "000");
		target.SetFilter("Message", "Def 456");

		target.SetFilter("Status", "2");
		target.SetFilter("Status", "0 3 9 ABC");

		target.SetFilter("Absence", "00 01");
		target.SetFilter("Absence", "00 DEF 02 03");

		target.SetFilter("ShouldBeIgnored", "Something");

		var filters = target.Filters;
		expect(filters.length).toEqual(4);

		var subjectCriteriaCount = 0;
		var messageCriteriaCount = 0;
		var statusCriteriaCount = 0;
		var absenceCriteriaCount = 0;
		var shouldIgnoredCriteriaCount = 0;

		for (var i = 0; i < filters.length; i++) {
			var criteria = filters[i];

			if (criteria.hasOwnProperty("Subject")) {
				subjectCriteriaCount++;
				expect(criteria.Subject).toEqual("Abc 123");
			} else if (criteria.hasOwnProperty("Message")) {
				messageCriteriaCount++;
				expect(criteria.Message).toEqual("Def 456");
			} else if (criteria.hasOwnProperty("Status")) {
				statusCriteriaCount++;
				expect(criteria.Status).toEqual("0 3");
				expect(target.GetSelectedStatusDescription()).toEqual("Status0, Status3");
				for (var j = 0; j < target.RequestStatuses.length; j++) {
					var status = target.RequestStatuses[j];
					expect(status.Selected).toEqual(status.Id === 0 || status.Id === 3);
				}
			} else if (criteria.hasOwnProperty("Absence")) {
				absenceCriteriaCount++;
				expect(criteria.Absence).toEqual("00 02 03");
				expect(target.GetSelectedAbsenceDescription()).toEqual("Absence0, Absence2, Absence3");
				for (var j = 0; j < target.RequestableAbsences.length; j++) {
					var absence = target.RequestableAbsences[j];
					expect(absence.Selected).toEqual(absence.Id === "00"
						|| absence.Id === "02" || absence.Id === "03");
				}
			} else if (criteria.hasOwnProperty("ShouldBeIgnored")) {
				shouldIgnoredCriteriaCount++;
			}
		}

		expect(subjectCriteriaCount).toEqual(1);
		expect(messageCriteriaCount).toEqual(1);
		expect(statusCriteriaCount).toEqual(1);
		expect(absenceCriteriaCount).toEqual(1);
		expect(shouldIgnoredCriteriaCount).toEqual(0);
	});

	it("Should remove single filter when no keyword set", function() {
		target.SetFilter("Subject", "Abc 123");
		target.SetFilter("Message", "000");
		target.SetFilter("Status", "0 3 9");
		target.SetFilter("Absence", "00 01");

		expect(target.Filters.length).toEqual(4);

		target.SetFilter("Subject", undefined);
		target.SetFilter("Message", "");
		target.SetFilter("Status", "");
		target.SetFilter("Absence", undefined);

		expect(target.Filters.length).toEqual(0);
	});

	it("Should reset single filter", function() {
		target.SetFilter("Subject", "Abc 123");
		target.SetFilter("Message", "000");
		target.SetFilter("Status", "0 3 9");
		target.SetFilter("Absence", "00 01");

		expect(target.Filters.length).toEqual(4);

		target.RemoveFilter("Subject");
		target.RemoveFilter("Message");
		target.RemoveFilter("Status");
		target.RemoveFilter("Absence");

		expect(target.Filters.length).toEqual(0);
	});

	it("Should reset all filters", function() {
		target.SetFilter("Subject", "Abc 123");
		target.SetFilter("Message", "000");
		target.SetFilter("Status", "0 3 9");
		target.SetFilter("Absence", "00 01");

		expect(target.Filters.length).toEqual(4);

		target.ResetFilter();
		expect(target.Filters.length).toEqual(0);
	});
});