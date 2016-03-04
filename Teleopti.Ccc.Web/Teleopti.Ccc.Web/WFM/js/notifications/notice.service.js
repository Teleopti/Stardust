(function(){
'use strict';
 angular.module('wfm.notice')
 .service('NoticeService', ['$rootScope', function($rootScope){
  var service = {};

  service.destroyOnStateChange = function (message) {
   $rootScope.$on('$stateChangeSuccess', function(){
    if (message !== undefined)
     message.destroy();
   });
  };

  return service;
 }]);
})();
