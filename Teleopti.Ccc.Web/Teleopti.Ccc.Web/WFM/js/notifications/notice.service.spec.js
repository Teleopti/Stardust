(function() {
	'use strict';

	describe('wfm.notice', function() {

		beforeEach(function() {
			module('wfm.notice');
		})

		it('should create a notice', inject(function(NoticeService){
			var content = 'Test message';
			var timeToLive = 5000;
			var destroyOnStateChange = false;

			var expectedObject = {
				content: 'Test message',
				timeToLive: 5000,
				destroyOnStateChange: false
			};

			var result = NoticeService.addNotice(content, timeToLive, destroyOnStateChange);

			expect(result).toEqual(expectedObject);
		}));

	});
})();
