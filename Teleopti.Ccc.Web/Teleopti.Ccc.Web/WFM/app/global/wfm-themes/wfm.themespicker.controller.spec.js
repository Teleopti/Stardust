'use strict';
describe('ThemesPickerController', function () {
	var $q,
		$rootScope,
		$httpBackend;

	beforeEach(function () {
		module('wfm.themes');
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$rootScope.setTheme = function () {
			return
		};
	}));


	var setUpTemplate = function (option) {
		if (option === "withHash") {
			var themeModules = document.createElement("link");
			themeModules.setAttribute("id", "themeModules");
			themeModules.setAttribute("href", "orig?123");
			document.head.appendChild(themeModules);
			var themeStyle = document.createElement("link");
			themeStyle.setAttribute("id", "themeStyle");
			themeStyle.setAttribute("href", "orig?123");
			document.head.appendChild(themeStyle);
		}
		else {
			var themeModules = document.createElement("link");
			themeModules.setAttribute("id", "themeModules");
			themeModules.setAttribute("href", "");
			document.head.appendChild(themeModules);
			var themeStyle = document.createElement("link");
			themeStyle.setAttribute("id", "themeStyle");
			themeStyle.setAttribute("href", "");
			document.head.appendChild(themeStyle);
		}

		var checkBox = document.createElement("input");
		checkBox.setAttribute("id", "darkTheme");
		document.body.appendChild(checkBox);
		var container = document.createElement("div");
		container.setAttribute("id", "themeMenu");
		document.body.appendChild(container);

	};
	var teardownTemplate = function () {
		document.getElementById('themeMenu').remove();
		document.getElementById('themeModules').remove();
		document.getElementById('themeStyle').remove();
	};



	it('should toggle theme to dark', inject(function ($controller) {
		var scope = $rootScope.$new();
		setUpTemplate();
		var vm = $controller('ThemesPickerController', {
			$scope: scope
		});

		expect(vm.currentTheme).toEqual(undefined);
		vm.toggleTheme();
		expect(vm.currentTheme).toEqual('dark');
		teardownTemplate();
	}));



});
