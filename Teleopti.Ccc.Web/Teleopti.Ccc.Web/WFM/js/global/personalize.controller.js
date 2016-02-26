(function () {
  'use strict';
  var personalize = angular.module('wfm.personalize',  []);

  personalize.controller('personalizeController', [
    '$scope','Toggle', function ($scope,toggleService) {

      $scope.showOverlay=false;

      toggleService.togglesLoaded.then(function() {
        $scope.personalizeToggle = toggleService.WfmGlobalLayout_personalOptions_37114;
      });

      var focusMenu = function(){
        document.getElementById("personalizeMenu").focus();
      }

      $scope.toggleOverlayFocus = function () {
        $scope.showOverlay=!$scope.showOverlay;
        focusMenu();
      };
    }
  ]);
})();
