'use strict';
(function() {
var wfmNotice = angular.module('wfm.notice');
wfmNotice.controller('wfmNoticeCtrl', wfmNoticeCtrl);
wfmNoticeCtrl.$inject = [];
 function wfmNoticeCtrl () {
  var vm = this;

 }

 wfmNotice.directive('wfmNotice', wfmNoticeDirective);
 function wfmNoticeDirective () {
  return {
   controller: 'wfmNoticeCtrl',
   controllerAs: 'vm',
   template: "<div growl></div>"
 }

 }
})();
