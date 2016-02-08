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

		it('should receive registered key sequence and store in table', inject(function(ShortCuts){
			var specialKeySequence = [17];
			var stateName = 'rta';
			var keyCode = 49;
			var keySequenceName = keyCode + stateName;
			var callback = function (){};

			ShortCuts.registerKeySequence(keyCode, stateName, specialKeySequence, callback);

			expect(ShortCuts.keySequenceTable[keySequenceName].length).toEqual(2);
			expect(ShortCuts.keySequenceTable[keySequenceName][0]).toEqual(specialKeySequence);
			expect(ShortCuts.keySequenceTable[keySequenceName][1]).toEqual(callback);
		}));

		it('should execute callback function for given key sequence', inject(function(ShortCuts){
			var specialKeySequence = [17];
			var stateName = 'rta';
			var keyCode = 49;
			var keySequenceName = keyCode + stateName;
			var callback = function (){};
			var specialKeyEvent = {
				keyCode: 17
			};
			var normalKeyEvent = {
				keyCode: 49
			};
			ShortCuts.state = stateName;

			ShortCuts.registerKeySequence(keyCode, stateName, specialKeySequence, callback);
			ShortCuts.handleKeyEvent(specialKeyEvent)

			expect(ShortCuts.handleKeyEvent(normalKeyEvent)).toEqual(true);
		}));
	});
})();
