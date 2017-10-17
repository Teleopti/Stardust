(function () {
	'use strict';

	angular
		.module('wfm.themes')
		.controller('ThemesPickerController', ThemesPickerController);

	ThemesPickerController.$inject = ['$scope', 'Toggle', '$rootScope', 'ThemeService', '$q'];

	function ThemesPickerController($scope, Toggle, $rootScope, ThemeService, $q) {
		var vm = this;
		vm.toggleTheme = toggleTheme;
		vm.toggleOverlay = toggleOverlay;
		vm.showOverlay = showOverlay;
		vm.personalizeToggle = true;


		checkThemeState().then(function (result) {
			if (result.data.Name == null) {
				result.data.Name = "classic";
			};
			vm.showOverlay = result.data.Overlay;
			vm.currentTheme = result.data.Name;
			replaceCurrentTheme(vm.currentTheme, vm.showOverlay);
			if (vm.currentTheme === "dark") {
				vm.darkTheme = true;
			} else {
				vm.darkTheme = false;
			}
		});

		function showOverlay() {
			return overylay = true;
		}

		function toggleOverlay(state) {

			if (state === true || state === false) {
				vm.showOverlay = state;
				replaceCurrentTheme(vm.currentTheme, vm.showOverlay);
			} else {
				toggleActiveOverlay();
			}
		}

		function toggleTheme(theme) {
			if (theme === "classic" || theme === "dark") {
				vm.currentTheme = theme;
				replaceCurrentTheme(theme, vm.showOverlay);
				return;
			} else {
				toggleActiveTheme();
			}
			vm.darkTheme = !vm.darkTheme;
		};

		function toggleActiveTheme() {
			if (vm.darkTheme) {
				vm.currentTheme = 'classic';
				replaceCurrentTheme('classic', vm.showOverlay);
			} else {
				vm.currentTheme = 'dark';
				replaceCurrentTheme('dark', vm.showOverlay);
			}
		}

		function replaceCurrentTheme(theme, overlay) {
			$rootScope.setTheme(theme);
			ThemeService.saveTheme(theme, overlay);
		}

		function toggleActiveOverlay() {
			vm.showOverlay = !vm.showOverlay;
			replaceCurrentTheme(vm.currentTheme, vm.showOverlay);
		}

		function checkThemeState() {
			var deferred = $q.defer();
			ThemeService.getTheme().then(function (response) {
				deferred.resolve(response);
			});
			return deferred.promise;
		}
	}
})();
