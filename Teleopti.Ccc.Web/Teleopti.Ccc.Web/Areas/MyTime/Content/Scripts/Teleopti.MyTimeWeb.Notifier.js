Teleopti.MyTimeWeb.Notifier = (function() {
	function _notify(options) {
		return noty({
			text: options.text,
			layout: 'bottom',
			timeout: 5000,
			animation: {
				open: { height: 'toggle' },
				close: { height: 'toggle' },
				easing: 'swing',
				speed: 500 // opening & closing animation speed
			}
		});
	}
	return {
		Notify: function (options) {
			_notify(options);
		}
	};
})(jQuery)