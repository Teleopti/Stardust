(function() {
	'use strict';

	angular.module('shortcutsService', [])
		.constant('keyCodes', {
			A: 65,
    B: 66,
    C: 67,
    D: 68,
    E: 69,
    F: 70,
    G: 71,
    H: 72,
    I: 73,
    J: 74,
    K: 75,
    L: 76,
    M: 77,
    N: 78,
    O: 79,
    P: 80,
    Q: 81,
    R: 82,
    S: 83,
    T: 84,
    U: 85,
    V: 86,
    W: 87,
    X: 88,
    Y: 89,
    Z: 90,
    ZERO: 48,
    ONE: 49,
    TWO: 50,
    THREE: 51,
    FOUR: 52,
    FIVE: 53,
    SIX: 54,
    SEVEN: 55,
    EIGHT: 56,
    NINE: 57,
    NUMPAD_0: 96,
    NUMPAD_1: 97,
    NUMPAD_2: 98,
    NUMPAD_3: 99,
    NUMPAD_4: 100,
    NUMPAD_5: 101,
    NUMPAD_6: 102,
    NUMPAD_7: 103,
    NUMPAD_8: 104,
    NUMPAD_9: 105,
    NUMPAD_MULTIPLY: 106,
    NUMPAD_ADD: 107,
    NUMPAD_ENTER: 108,
    NUMPAD_SUBTRACT: 109,
    NUMPAD_DECIMAL: 110,
    NUMPAD_DIVIDE: 111,
    F1: 112,
    F2: 113,
    F3: 114,
    F4: 115,
    F5: 116,
    F6: 117,
    F7: 118,
    F8: 119,
    F9: 120,
    F10: 121,
    F11: 122,
    F12: 123,
    F13: 124,
    F14: 125,
    F15: 126,
    COLON: 186,
    EQUALS: 187,
    UNDERSCORE: 189,
    QUESTION_MARK: 191,
    TILDE: 192,
    OPEN_BRACKET: 219,
    BACKWARD_SLASH: 220,
    CLOSED_BRACKET: 221,
    QUOTES: 222,
    BACKSPACE: 8,
    TAB: 9,
    CLEAR: 12,
    ENTER: 13,
    SHIFT: 16,
    CONTROL: 17,
    ALT: 18,
    CAPS_LOCK: 20,
    ESC: 27,
    SPACEBAR: 32,
    PAGE_UP: 33,
    PAGE_DOWN: 34,
    END: 35,
    HOME: 36,
    LEFT: 37,
    UP: 38,
    RIGHT: 39,
    DOWN: 40,
    INSERT: 45,
    DELETE: 46,
    HELP: 47,
    NUM_LOCK: 144
		})
		.service('ShortCuts', ['$document', 'keyCodes', function($document, keyCodes) {
			var service = {};
			var specialKeys = {};
			specialKeys[keyCodes.SHIFT] = false;
			specialKeys[keyCodes.CONTROL] = false;
			specialKeys[keyCodes.ALT] = false;

			service.keySequenceTable = {};

			$document.on('keydown', function(event) {
				// if(event.target.tagName !== "INPUT")
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

				if (match) {
          event.preventDefault ? event.preventDefault() : event.returnValue = false;
          arr[1].apply();
        }
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
