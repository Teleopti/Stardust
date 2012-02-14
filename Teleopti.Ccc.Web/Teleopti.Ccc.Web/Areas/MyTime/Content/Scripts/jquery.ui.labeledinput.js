(function ($) {
	$.widget("ui.labeledinput", {
		_create: function () {
			var input = this.element;
			var htmlElement = this.element[0];
			var label = $('label[for="' + htmlElement.id + '"]');
			if (htmlElement.value)
				label.hide();

			label
				.addClass('ui-labeled-input-label')
				;

			input
				.addClass('ui-labeled-input')
				.addClass('ui-widget')
				.addClass('ui-widget-content')
				.addClass("ui-corner-all")
				.change(function () {
					if (!this.value) {
						label.show();
					} else {
						label.hide();
					}
				})
				.focus(function () {
					label.hide();
				})
				.blur(function () {
					if (!this.value) {
						label.show();
					}
				})
				;
		},

		destroy: function () {
		}
	});
})(jQuery);