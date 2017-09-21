(function () {
  'use strict';

  angular
    .module('wfm')
    .controller('MainController', MainController);

  MainController.$inject = ['$scope', 'Toggle', '$rootScope', 'ThemeService', '$q'];

  /* @ngInject */
  function MainController($scope, Toggle, $rootScope, ThemeService, $q) {
    var vm = this;

    ThemeService.getTheme().then(function (result) {
      vm.currentStyle = result.data.Name;
    });

    $rootScope.setTheme = function (theme) {
      var darkThemeElement = document.getElementById('darkTheme');
      if (darkThemeElement) {
        darkThemeElement.checked = (theme === "dark");
      }
      if (checkCurrentTheme() != theme) {
        vm.styleIsFullyLoaded = false;
        switchCssSrcRefs(theme);
      }
    };

    function refreshElement() {
      destroyElement()
      var moduleElement = document.createElement('link');
      var styleElement = document.getElementById('themeStyle');
      moduleElement.id = 'themeModules';
      moduleElement.rel = "stylesheet";
      moduleElement.href = "";
      moduleElement.onload = function () {
        $scope.$apply(function () {
          vm.styleIsFullyLoaded = true;
        });
      };
      document.head.insertBefore(moduleElement, styleElement);
      return moduleElement;
    }
    function destroyElement() {
      var themeElement = document.getElementById('themeModules');
      themeElement.remove();
    }

    function applyThemeToModules(theme) {
      var themeElement = refreshElement();
      themeElement.setAttribute('href', 'dist/resources/modules_' + theme + '.min.css');
      themeElement.setAttribute('class', theme)

    };

    function applyThemeToMain(theme) {
      var themeElement = document.getElementById('themeStyle');
      themeElement.setAttribute('href', 'dist/style_' + theme + '.min.css');
      themeElement.setAttribute('class', theme)
    }

    function switchCssSrcRefs(theme) {
      applyThemeToModules(theme)
      applyThemeToMain(theme)
    }

    function checkCurrentTheme() {
      var currentStyle;
      if (document.getElementById('themeStyle').className) {
        currentStyle = document.getElementById('themeStyle').className;
      }
      return currentStyle;
    }
  }
})();
