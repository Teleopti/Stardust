(function() {
	'use strict';

	describe('shortcutsService', function() {

		beforeEach(function() {
			module('shortcutsService');
		})

		it('should check if key event is special key', inject(function(ShortCuts, keyCodes) {
			var specialKeyEvent = {
				keyCode: 17
			};
			var normalKeyEvent = {
				keyCode: 49
			};

			expect(ShortCuts.checkSpecialKey(specialKeyEvent)).toEqual(true);
			expect(ShortCuts.checkSpecialKey(normalKeyEvent)).toEqual(false);
		}));

		it('should execute callback function for given key sequence', inject(function(ShortCuts){
			var specialKeySequence = [17];
			var keyCode = 49;
			var keySequenceName = keyCode;
			var callback = function (){};
			var specialKeyEvent = {
				keyCode: 17
			};
			var normalKeyEvent = {
				keyCode: 49
			};

			ShortCuts.registerKeySequence(keyCode, specialKeySequence, callback);
			ShortCuts.handleKeyEvent(specialKeyEvent)

			expect(ShortCuts.handleKeyEvent(normalKeyEvent)).toEqual(true);
		}));
	});
})();
