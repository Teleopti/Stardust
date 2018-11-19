(function() {
	'use strict';

	describe('[Requests Filter Service Test]', function() {
		var $rootScope, target, requestsTabNames;

		beforeEach(function() {
			module('wfm.requests');
		});

		beforeEach(inject(function(_$rootScope_, _RequestsFilter_, REQUESTS_TAB_NAMES) {
			$rootScope = _$rootScope_;
			target = _RequestsFilter_;
			requestsTabNames = REQUESTS_TAB_NAMES;
		}));

		it('Should update filters on filter changed', function() {
			target.setFilter('Subject', 'CDEF', requestsTabNames.absenceAndText);
			target.setFilter('Subject', 'Abc 123', requestsTabNames.absenceAndText);

			target.setFilter('Message', '000', requestsTabNames.absenceAndText);
			target.setFilter('Message', 'Def 456', requestsTabNames.absenceAndText);

			target.setFilter('Status', '2', requestsTabNames.absenceAndText);
			target.setFilter('Status', '0 3', requestsTabNames.absenceAndText);

			target.setFilter('Type', '00 01', requestsTabNames.absenceAndText);
			target.setFilter('Type', '00 02 03', requestsTabNames.absenceAndText);

			target.setFilter('ShouldBeIgnored', 'Something', requestsTabNames.absenceAndText);

			var filters = target.filters[requestsTabNames.absenceAndText];
			expect(filters.length).toEqual(4);

			var subjectCriteriaCount = 0;
			var messageCriteriaCount = 0;
			var statusCriteriaCount = 0;
			var absenceCriteriaCount = 0;
			var shouldIgnoredCriteriaCount = 0;

			for (var i = 0; i < filters.length; i++) {
				var criteria = filters[i];

				if (criteria.hasOwnProperty('Subject')) {
					subjectCriteriaCount++;
					expect(criteria.Subject).toEqual('Abc 123');
				} else if (criteria.hasOwnProperty('Message')) {
					messageCriteriaCount++;
					expect(criteria.Message).toEqual('Def 456');
				} else if (criteria.hasOwnProperty('Status')) {
					statusCriteriaCount++;
					expect(criteria.Status).toEqual('0 3');
				} else if (criteria.hasOwnProperty('Type')) {
					absenceCriteriaCount++;
					expect(criteria.Type).toEqual('00 02 03');
				} else if (criteria.hasOwnProperty('ShouldBeIgnored')) {
					shouldIgnoredCriteriaCount++;
				}
			}

			expect(subjectCriteriaCount).toEqual(1);
			expect(messageCriteriaCount).toEqual(1);
			expect(statusCriteriaCount).toEqual(1);
			expect(absenceCriteriaCount).toEqual(1);
			expect(shouldIgnoredCriteriaCount).toEqual(0);
		});

		it('Should remove single filter when no keyword set', function() {
			target.setFilter('Subject', 'Abc 123', requestsTabNames.absenceAndText);
			target.setFilter('Message', '000', requestsTabNames.absenceAndText);
			target.setFilter('Status', '0 3 9', requestsTabNames.absenceAndText);
			target.setFilter('Type', '00 01', requestsTabNames.absenceAndText);

			expect(target.filters[requestsTabNames.absenceAndText].length).toEqual(4);

			target.setFilter('Subject', undefined, requestsTabNames.absenceAndText);
			target.setFilter('Message', '', requestsTabNames.absenceAndText);
			target.setFilter('Status', '', requestsTabNames.absenceAndText);
			target.setFilter('Type', undefined, requestsTabNames.absenceAndText);

			expect(target.filters[requestsTabNames.absenceAndText].length).toEqual(0);
		});

		it('Should reset single filter', function() {
			target.setFilter('Subject', 'Abc 123', requestsTabNames.absenceAndText);
			target.setFilter('Message', '000', requestsTabNames.absenceAndText);
			target.setFilter('Status', '0 3 9', requestsTabNames.absenceAndText);
			target.setFilter('Type', '00 01', requestsTabNames.absenceAndText);

			expect(target.filters[requestsTabNames.absenceAndText].length).toEqual(4);

			target.removeFilter('Subject', requestsTabNames.absenceAndText);
			target.removeFilter('Message', requestsTabNames.absenceAndText);
			target.removeFilter('Status', requestsTabNames.absenceAndText);
			target.removeFilter('Type', requestsTabNames.absenceAndText);

			expect(target.filters[requestsTabNames.absenceAndText].length).toEqual(0);
		});

		it('Should reset all filters', function() {
			target.setFilter('Subject', 'Abc 123', requestsTabNames.absenceAndText);
			target.setFilter('Message', '000', requestsTabNames.absenceAndText);
			target.setFilter('Status', '0 3 9', requestsTabNames.absenceAndText);
			target.setFilter('Type', '00 01', requestsTabNames.absenceAndText);

			expect(target.filters[requestsTabNames.absenceAndText].length).toEqual(4);

			target.resetFilter(requestsTabNames.absenceAndText);
			expect(target.filters[requestsTabNames.absenceAndText].length).toEqual(0);
		});

		it('Should reset all filters for shiftTrade also', function() {
			target.setFilter('Subject', 'Abc 123', requestsTabNames.shiftTrade);
			target.setFilter('Message', '000', requestsTabNames.shiftTrade);
			target.setFilter('Status', '0 3 9', requestsTabNames.shiftTrade);
			target.setFilter('Type', '00 01', requestsTabNames.shiftTrade);

			expect(target.filters[requestsTabNames.shiftTrade].length).toEqual(4);

			target.resetFilter(requestsTabNames.shiftTrade);
			expect(target.filters[requestsTabNames.shiftTrade].length).toEqual(0);
		});
	});
})();
