(function() {
  'use strict';
  describe('WfmShortcuts', function() {

    var style1 = {
      outline: ''
    }
    var target1 = {
      style: style1
    }
    var keyUpEvent1 = {
      keyCode: 9,
      target: target1
    }

    var style2 = {
      outline: ''
    }
    var target2 = {
      style: style2
    }
    var keyUpEvent2 = {
      keyCode: 9,
      target: target2
    }

    beforeEach(function() {
      module('wfm');
    })

    it('should add target element to array on tab keyup event', inject(function(WfmShortcuts) {
      WfmShortcuts.traverseWithTab(keyUpEvent1);

      expect(WfmShortcuts.tabbedTargets.length).toEqual(1);
    }));

    it('should remove first element from array on second tab keyup event', inject(function(WfmShortcuts) {
      WfmShortcuts.traverseWithTab(keyUpEvent1);
      WfmShortcuts.traverseWithTab(keyUpEvent2);

      expect(WfmShortcuts.tabbedTargets.length).toEqual(1);
    }));

    it('should apply outline style to element in array', inject(function(WfmShortcuts) {
      WfmShortcuts.traverseWithTab(keyUpEvent1);

      expect(WfmShortcuts.tabbedTargets[0].target.style.outline).not.toEqual('');
    }));

    it('should remove outline style from first element in array', inject(function(WfmShortcuts) {
      WfmShortcuts.traverseWithTab(keyUpEvent1);
      WfmShortcuts.traverseWithTab(keyUpEvent2);

      expect(WfmShortcuts.tabbedTargets[0].target.style.outline).not.toEqual('');
    }));

    it('should remove targeted element from array when mousedown event',inject(function(WfmShortcuts) {
      WfmShortcuts.traverseWithTab(keyUpEvent1);
      WfmShortcuts.clearTabbedTarget();

      expect(WfmShortcuts.tabbedTargets.length).toEqual(0);
    }));

    it('should remove outline style on targeted element when mousedown event',inject(function(WfmShortcuts) {
      var temp = {};

  		WfmShortcuts.traverseWithTab(keyUpEvent1);
  		temp = WfmShortcuts.tabbedTargets[0];
  		WfmShortcuts.clearTabbedTarget();

  		expect(temp.target.style.outline).toEqual('');
    }));


  });
})();
