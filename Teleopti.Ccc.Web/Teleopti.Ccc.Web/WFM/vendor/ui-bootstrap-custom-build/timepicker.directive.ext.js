(function() {
	'use strict';

	angular.module('ui.bootstrap.timepicker')
		.run([
			'$templateCache', function($templateCache) {
				$templateCache.put("template/timepicker/timepicker.html",
					"<table class=\"timepicker\">" +
					"  <tbody>" +
					"    <tr>" +
					"      <td class=\"form-group\" ng-class=\"{'has-error': invalidHours}\">" +
					"          <input type=\"text\" autocomplete=\"off\" ng-model=\"hours\" ng-change=\"updateHours()\" ng-readonly=\"::readonlyInput\" maxlength=\"2\" tabindex=\"{{::tabindex}}\" />" +
					"      </td>" +
					"      <td>:</td>" +
					"      <td class=\"form-group\" ng-class=\"{'has-error': invalidMinutes}\">" +
					"          <input type=\"text\" autocomplete=\"off\" ng-model=\"minutes\" ng-change=\"updateMinutes()\" ng-readonly=\"::readonlyInput\" maxlength=\"2\" tabindex=\"{{::tabindex}}\" />" +
					"      </td>" +
					"      <td ng-show=\"showMeridian\">" +
					"          <md-button type=\"button\" aria-label=\"toggle meridian\" ng-disabled=\"noToggleMeridian()\" ng-click=\"toggleMeridian()\" tabindex=\"{{::tabindex}}\">{{meridian}}</md-button>" +
					"      </td>" +
					"    </tr>" +
					"  </tbody>" +
					"</table>"
				);
			}
		]);
})();
