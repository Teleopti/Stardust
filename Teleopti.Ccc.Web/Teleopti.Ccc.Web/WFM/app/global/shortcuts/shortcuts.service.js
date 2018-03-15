(function (angular) {
	'use strict';
	angular
	.module('shortcutsService')
	.service('ShortCuts', ShortCuts);

	ShortCuts.$inject = ['$document', 'keyCodes'];

	function ShortCuts($document, keyCodes) {
		var specialKeys = {};
		specialKeys[keyCodes.SHIFT] = false;
		specialKeys[keyCodes.CONTROL] = false;
		specialKeys[keyCodes.ALT] = false;

		var keySequenceTable = {};
		this.handleKeyEvent = handleKeyEvent;
		this.registerKeySequence = registerKeySequence;
		this.checkSpecialKey = checkSpecialKey;


		$document.on('keydown', function(event) {
			if (event.target.tagName !== 'INPUT' || event.target.type === 'checkbox')
			handleKeyEvent(event);
		});

		$document.on('keyup', function(event){
			checkSpecialKey(event, false)
		});

		function handleKeyEvent(event) {
			var arr = [];
			if (checkSpecialKey(event, true) || !keySequenceTable[event.keyCode])
			return;

			arr = keySequenceTable[event.keyCode];

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
				
				arr[1].apply(null, [event]);
			}
			return match;
		};

		function registerKeySequence(keyCode, specialKeySequence, callback) {
			keySequenceTable[keyCode] = [specialKeySequence, callback];
		};

		function checkSpecialKey(event, isPressed) {
			if (specialKeys.hasOwnProperty(event.keyCode)) {
				specialKeys[event.keyCode] = isPressed;
				return true;
			} else {
				return false;
			}
		};
	}
})(angular);
