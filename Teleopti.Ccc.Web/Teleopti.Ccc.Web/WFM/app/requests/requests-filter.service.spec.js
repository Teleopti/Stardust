(function() {
	"use strict";

	describe("[Requests Filter Service Test]", function() {
		var target;

		var $rootScope;

		beforeEach(function() {
			module("wfm.requests");
		});

		beforeEach(inject(function(_$rootScope_, _RequestsFilter_) {
			$rootScope = _$rootScope_;
			target = _RequestsFilter_;
		}));

		it("Should update filters on filter changed", function() {
			target.SetFilter("Subject", "CDEF");
			target.SetFilter("Subject", "Abc 123");

			target.SetFilter("Message", "000");
			target.SetFilter("Message", "Def 456");

			target.SetFilter("Status", "2");
			target.SetFilter("Status", "0 3");

			target.SetFilter("Absence", "00 01");
			target.SetFilter("Absence", "00 02 03");

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
				} else if (criteria.hasOwnProperty("Absence")) {
					absenceCriteriaCount++;
					expect(criteria.Absence).toEqual("00 02 03");
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
})();