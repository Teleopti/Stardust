"use strict";

describe("[Requests Filter Service Test]", function () {
	var target;

	var absences = {
		data: [
			{ Id: "00", Name: "Absence0" },
			{ Id: "01", Name: "Absence1" },
			{ Id: "02", Name: "Absence2" },
			{ Id: "03", Name: "Absence3" }
		]
	};

	var statuses = [
		{ Id: 0, Name: "Status0" },
		{ Id: 1, Name: "Status1" },
		{ Id: 2, Name: "Status2" },
		{ Id: 3, Name: "Status3" }
	];

	function mockRequestsDataService() {
		this.getRequestableAbsences = function () {
			return {
				then: function (mock) {
					mock(absences);
				}
			}
		}

		this.getAllRequestStatuses = function () {
			return statuses;
		}
	}

	var $rootScope;

	beforeEach(function () {
		var requestsDataSvc = new mockRequestsDataService();
		module('wfm.requests');
		module(function ($provide) {
			$provide.service('requestsDataService', function () { return requestsDataSvc; });
		});
	});

	beforeEach(inject(function (_$rootScope_, _RequestsFilter_) {
		$rootScope = _$rootScope_;
		target = _RequestsFilter_;
	}));

	it("Should get description for selected absences", function () {
		var description = target.GetSelectedAbsenceDescription(["00", "02"]);
		expect(description).toEqual("Absence0, Absence2");
	});

	it("Should get description for selected status", function () {
		var description = target.GetSelectedStatusDescription([1, 2]);
		expect(description).toEqual("Status1, Status2");
	});
});