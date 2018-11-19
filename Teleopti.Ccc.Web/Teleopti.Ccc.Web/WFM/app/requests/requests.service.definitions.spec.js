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
	});
})();
