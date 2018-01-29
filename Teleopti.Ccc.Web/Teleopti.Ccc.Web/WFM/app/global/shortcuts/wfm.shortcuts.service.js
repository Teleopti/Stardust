angular.module('wfm').service('tabService', function($document) {
  var focusStyle = '1px solid #09F';
  var tabbedTargets = [];
  var isTabbedClicked;

  $document.on('keyup', function(event) {
    if (event.keyCode == 9){
      isTabbedClicked = true;
      traverseWithTab(event);
    }
  });

  $document.on('mousedown', function () {
    if (isTabbedClicked)
      clearTabbedTarget();
  });

  function traverseWithTab(event) {
    tabbedTargets.push(event);
    tabbedTargets[0].target.style.outline = focusStyle;

    if (tabbedTargets.length > 1) {
      tabbedTargets[0].target.style.outline = "";
      tabbedTargets[1].target.style.outline = focusStyle;
      tabbedTargets.splice(0, 1);
    }
  };

  function clearTabbedTarget() {
      tabbedTargets.forEach(function(e){
        e.target.style.outline = "";
      })
      tabbedTargets = [];
      isTabbedClicked = false;
  };
});
