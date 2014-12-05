
define([
		'text!templates/layout.html'
], function (
		layoutTemplate
		) {

	function render() {
		$('body').append(layoutTemplate);

		require(['view'], function (view) {
		});
	}

	render();

});
