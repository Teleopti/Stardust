(function() {
	'use strict';

	fdescribe('wfm.themes', function() {
		var $httpBackend;

		beforeEach(function() {
			module('wfm.themes');
		});

		beforeEach(inject(function(_$httpBackend_) {
			$httpBackend = _$httpBackend_;
		}));

		it('should set theme', inject(function(ThemeService) {
			var themeModules = document.createElement("link");
			themeModules.setAttribute("id", "themeModules");
			themeModules.setAttribute("href", "");
			document.head.appendChild(themeModules);
			var themeStylesheet = document.createElement("link");
			themeStylesheet.setAttribute("id", "themeStylesheet");
			themeStylesheet.setAttribute("href", "");
			document.head.appendChild(themeStylesheet);

			ThemeService.setTheme("dark");

			expect(document.getElementById('themeModules').getAttribute('href')).toBe('dist/modules_dark.min.css');
			expect(document.getElementById('themeStylesheet').getAttribute('href')).toBe('dist/style_dark.min.css');

   document.getElementById('themeModules').remove();
   document.getElementById('themeStylesheet').remove();
		}));

				it('should init theme', inject(function(ThemeService) {
					$httpBackend.expectGET("../api/Theme")
						.respond(200, {Name: "light"});

						var themeModules = document.createElement("link");
						themeModules.setAttribute("id", "themeModules");
						themeModules.setAttribute("href", "");
						document.head.appendChild(themeModules);
						var themeStylesheet = document.createElement("link");
						themeStylesheet.setAttribute("id", "themeStylesheet");
						themeStylesheet.setAttribute("href", "");
						document.head.appendChild(themeStylesheet);

						ThemeService.init();

						expect(document.getElementById('themeModules').getAttribute('href')).toBe('dist/modules_light.min.css');
						expect(document.getElementById('themeStylesheet').getAttribute('href')).toBe('dist/style_light.min.css');

			   document.getElementById('themeModules').remove();
			   document.getElementById('themeStylesheet').remove();
				}));

	});
})();
