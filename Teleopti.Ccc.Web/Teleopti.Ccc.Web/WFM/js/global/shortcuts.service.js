(function() {
	'use strict';

	angular.module('shortcutsService', [])
		.constant('keyCodes', {
			ENTER: 13,
			UP: 38,
			DOWN: 40,
			SHIFT: 16,
			CONTROL: 17,
			ALT: 18,
			ZERO: 48,
			ONE: 49,
			TWO: 50,
			THREE: 51,
			FOUR: 52,
			FIVE: 53,
			SIX: 54,
			SEVEN: 55,
			EIGHT: 56,
			NINE: 57
		})
		.service('ShortCuts', ['$document', 'keyCodes', function($document, keyCodes) {
			var service = {};
			var specialKeys = {};
			specialKeys[keyCodes.SHIFT] = false;
			specialKeys[keyCodes.CONTROL] = false;
			specialKeys[keyCodes.ALT] = false;

			service.keySequenceTable = {};

			$document.on('keydown', function(event) {
				service.handleKeyEvent(event);
			});

			$document.on('keyup', function(event){
				specialKeys[keyCodes.SHIFT] = false;
				specialKeys[keyCodes.CONTROL] = false;
				specialKeys[keyCodes.ALT] = false;
			});

			service.handleKeyEvent = function(event) {
				var arr = [];
				if (service.checkSpecialKey(event) || !service.keySequenceTable[event.keyCode])
					return;

				arr = service.keySequenceTable[event.keyCode];

				var i = 0;
				var match = false;

				do {
				match = specialKeys.hasOwnProperty(arr[0][i]) && specialKeys[arr[0][i]];
				i++;
				} while (match && i < arr[0].length);

				if (arr[0].length === 0)
					match = true;

				if (match) arr[1]();
				return match;
			};

			service.registerKeySequence = function(keyCode, specialKeySequence, callback) {
				service.keySequenceTable[keyCode] = [specialKeySequence, callback];
			};

			service.checkSpecialKey = function(event) {
				if (specialKeys.hasOwnProperty(event.keyCode)) {
					specialKeys[event.keyCode] = true;
					return true;
				} else {
					return false;
				}
			};

			return service;
		}]);
})();
