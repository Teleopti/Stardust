(function () {
  'use strict';
  var personalize = angular.module('wfm.personalize',  []);

  personalize.controller('personalizeController', [
    '$scope','Toggle', function ($scope,toggleService) {

      $scope.showOverlay = false;
	  $scope.darkTheme = false;

      toggleService.togglesLoaded.then(function() {
        $scope.personalizeToggle = toggleService.WfmGlobalLayout_personalOptions_37114;
      });

      var focusMenu = function(){
        document.getElementById("personalizeMenu").focus();
      }
	  var themeToggleFromTo = function(to, from){
		  console.log(to,from);
			  document.getElementById(to+'Modules').removeAttribute('disabled');
			  document.getElementById(to+'Style').removeAttribute('disabled');
			  document.getElementById(from+'Modules').setAttribute('disabled', true);
			  document.getElementById(from+'Style').setAttribute('disabled',true);
	  }
      $scope.toggleOverlay = function () {
        $scope.showOverlay =! $scope.showOverlay;
        focusMenu();
      };
	  $scope.toggleDarkTheme = function(){
		  $scope.darkTheme =! $scope.darkTheme;
		  focusMenu();
		  if ($scope.darkTheme) {
			  themeToggleFromTo('dark','classic')
		  }else {
			  themeToggleFromTo('classic','dark');
		  }
	  }
    }
  ]);
})();
