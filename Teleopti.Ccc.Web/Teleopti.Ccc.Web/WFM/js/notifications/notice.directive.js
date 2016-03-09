(function(){
'use strict';

angular
 .module('wfm.notice')
 .directive('noticeDirective', function() {
  return {
   restrict: 'A',
   template: '<div class="notice"></div>'
  }
 });
})();
