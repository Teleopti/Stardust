(function() {
  'use strict';

  angular
  .module('wfm')
  .controller('MainController', MainController);

  MainController.$inject = ['$scope', 'Toggle', '$rootScope', 'ThemeService', '$q'];

  /* @ngInject */
  function MainController($scope, Toggle, $rootScope, ThemeService, $q) {
    var vm = this;

    ThemeService.getTheme().then(function(result){
      vm.currentStyle = result.data.Name;
    });

    $rootScope.setTheme = function(theme) {
      var darkThemeElement = document.getElementById('darkTheme');
      if (darkThemeElement) {
        darkThemeElement.checked = (theme === "dark");
      }
      vm.style = theme;
      if (checkCurrentTheme() != theme) {
        modifyDOMHeader(theme);
      }
    };

    function modifyDOMHeader (theme) {
      var styleElements = ["Modules", "Style"];
      styleElements.forEach(function(element) {
        var themeComponent = document.getElementById('theme' + element);
        if (themeComponent) {
          themeComponent.setAttribute('href', 'dist/' + element.toLowerCase() + '_' + theme + '.min.css' + '?' + new Date().getTime());
          themeComponent.setAttribute('class',theme);
          if (element === "Modules") {
            themeComponent.onload = function() {
              $scope.$apply(function() {
                vm.styleIsFullyLoaded = true;
              });
            };
          }
        }
      });
    }

    function checkCurrentTheme(){
      var currentStyle;
      if (document.getElementById('themeStyle').className) {
        currentStyle = document.getElementById('themeStyle').className;
      }
      return currentStyle;
    }
  }
})();
