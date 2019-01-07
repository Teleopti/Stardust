(function() {
	'use strict';

	describe('requests definitions service test', function() {
		var target;

		beforeEach(function() {
			module('wfm.requests');
		});

		beforeEach(inject(function(_requestsDefinitions_) {
			target = _requestsDefinitions_;
		}));

		it('should get correct search term when searching all conditions', function() {
			var searchKeyWords = 'search all using this keyword';
			var formatedSearchTerms = target.formatAgentSearchTerm(searchKeyWords);

			expect(JSON.stringify(formatedSearchTerms)).toEqual('{"All":"' + searchKeyWords + '"}');
		});

		it('should get agent name asc sorting order', function() {
			var input = {
				displayName: 'AgentName',
				sort: {direction:'asc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(0);
		});

		it('should get agent name desc sorting order', function () {
			var input = {
				displayName: 'AgentName',
				sort: {direction:'desc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(1);
		});

		it('should get StartTime asc sorting order', function () {
			var input = {
				displayName: 'StartTime',
				sort: {direction:'asc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(8);
		});

		it('should get StartTime desc sorting order', function () {
			var input = {
				displayName: 'StartTime',
				sort: {direction:'desc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(9);
		});

		it('should get EndTime asc sorting order', function () {
			var input = {
				displayName: 'EndTime',
				sort: {direction:'asc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(10);
		});

		it('should get EndTime desc sorting order', function () {
			var input = {
				displayName: 'EndTime',
				sort: {direction:'desc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(11);
		});

		it('should get UpdatedOn asc sorting order', function () {
			var input = {
				displayName: 'UpdatedOn',
				sort: {direction:'asc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(18);
		});

		it('should get UpdatedOn desc sorting order', function () {
			var input = {
				displayName: 'UpdatedOn',
				sort: {direction:'desc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(19);
		});

		it('should get CreatedOn asc sorting order', function () {
			var input = {
				displayName: 'CreatedOn',
				sort: {direction:'asc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(2);
		});

		it('should get CreatedOn desc sorting order', function () {
			var input = {
				displayName: 'CreatedOn',
				sort: {direction:'desc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(3);
		});

		it('should get DenyReason asc sorting order', function () {
			var input = {
				displayName: 'DenyReason',
				sort: {direction:'asc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(4);
		});

		it('should get DenyReason desc sorting order', function () {
			var input = {
				displayName: 'DenyReason',
				sort: {direction:'desc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(5);
		});

		it('should get Message asc sorting order', function () {
			var input = {
				displayName: 'Message',
				sort: {direction:'asc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(6);
		});

		it('should get Message desc sorting order', function () {
			var input = {
				displayName: 'Message',
				sort: {direction:'desc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(7);
		});

		it('should get Subject asc sorting order', function () {
			var input = {
				displayName: 'Subject',
				sort: {direction:'asc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(14);
		});

		it('should get Subject desc sorting order', function () {
			var input = {
				displayName: 'Subject',
				sort: {direction:'desc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(15);
		});

		it('should get Seniority asc sorting order', function () {
			var input = {
				displayName: 'Seniority',
				sort: {direction:'asc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(12);
		});

		it('should get Seniority desc sorting order', function () {
			var input = {
				displayName: 'Seniority',
				sort: {direction:'desc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(13);
		});

		it('should get Team asc sorting order', function () {
			var input = {
				displayName: 'Team',
				sort: {direction:'asc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(16);
		});

		it('should get Team desc sorting order', function () {
			var input = {
				displayName: 'Team',
				sort: {direction:'desc'}
			}

			var result = target.translateSingleSortingOrder(input);

			expect(result).toEqual(17);
		});
	});
})();
