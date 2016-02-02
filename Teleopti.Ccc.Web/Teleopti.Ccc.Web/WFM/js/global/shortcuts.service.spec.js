(function(){
'use strict';

describe('shortcutsService', function () {

 beforeEach(function () {
  module('shortcutsService');
 });

it('should catch key event and store in array', inject(function(ShortCuts){
var event = {
 keyCode: 17
};

expect(ShortCuts.checkSpecialKey(event)).toEqual(true);
}));
it('should recive key sequence and store in table', inject(function(ShortCuts){
var specialKeySequence = {shift: false, control: true, alt: false};
var stateName = 'Rta';
var keyCode = 59;
var key = keyCode + stateName;

ShortCuts.registerKeySequence(keyCode, stateName, specialKeySequence, function(){
});

expect(ShortCuts.keySequenceTable.key).not.toBeNull();
}));
it('should execute callback function for given key sequence', inject(function(ShortCuts){
var event1 = {
  keyCode: 16
};
var event2 = {
 keyCode: 59
};
var specialKeySequence = [16];
var stateName = 'Rta';
var keyCode = 59;
var key = keyCode + stateName;

ShortCuts.registerKeySequence(keyCode, stateName, specialKeySequence, function(){
});
ShortCuts.state = stateName;
ShortCuts.handleKeyEvent(event1);

expect(ShortCuts.handleKeyEvent(event2)).toEqual(true);
}));
});
})();
