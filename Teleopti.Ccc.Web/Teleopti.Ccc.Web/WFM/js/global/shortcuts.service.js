(function() {
	'use strict';

	angular.module('shortcutsService', ['ui.router'])
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
		.service('ShortCuts', ['$document', '$timeout', '$state', 'keyCodes', function($document, $timeout, $state, keyCodes) {
			var service = {};
			var specialKeys = {};
			specialKeys[keyCodes.SHIFT] = false;
			specialKeys[keyCodes.CONTROL] = false;
			specialKeys[keyCodes.ALT] = false;

			service.keySequenceTable = {};
			service.state = null;

			$document.on('keydown', function(event) {
				if(event.ctrlKey)
				event.preventDefault();
				$timeout(function() {
				service.handleKeyEvent(event);
				});
			});

			$document.on('keyup', function(event){
				specialKeys[keyCodes.SHIFT] = false;
				specialKeys[keyCodes.CONTROL] = false;
				specialKeys[keyCodes.ALT] = false;
			});

			service.handleKeyEvent = function(event) {
				var arr = [];
				if (service.checkSpecialKey(event))
					return;

				 if (service.state === null)
					arr = service.keySequenceTable[event.keyCode + $state.current.name];
					else
					arr = service.keySequenceTable[event.keyCode + service.state];

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

			service.registerKeySequence = function(keyCode, stateName, specialKeySequence, callback) {
				service.keySequenceTable[keyCode + stateName] = [specialKeySequence, callback];
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
