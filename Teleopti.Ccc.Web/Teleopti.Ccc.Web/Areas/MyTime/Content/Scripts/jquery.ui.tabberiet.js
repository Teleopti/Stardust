(function ($) {
	$.widget("ui.tabberiet", {

		options: {
			click: null
		},

		_create: function () {
			var self = this._self = this;
			var element = this._element = this.element;

			// Enable tabs on all tab widgets. The `event` property must be overridden so
			// that the tabs aren't changed on click, and any custom event name can be
			// specified. Note that if you define a callback for the 'select' event, it
			// will be executed for the selected tab whenever the hash changes.
			element.tabs({ event: 'change' });

			var tabs = element.find('ul.ui-tabs-nav a');
			tabs.click(self.options.click);
		},

		_selectById: function (id) {
			var tab = this._element
				.find('ul.ui-tabs-nav a')
				.filter((function () { return this.hash == id; }))
				;

			var tabContent = $(id);

			// if no tab was found, select no tab
			if (tab.length == 0) {
				$('#tabs').tabs("select", -1);
				$(".ui-tabs-selected").removeClass("ui-state-active").removeClass("ui-tabs-selected");
			} else {
				tab.triggerHandler('change');
			}

			// if no content was found, show empty content
			var emptyContent = this._element
				.find('#EmptyTabTest')
				;
			if (tabContent.length == 0) {
				this._element
					.find('.ui-tabs-panel')
					.addClass('ui-tabs-hide')
					;
				emptyContent.removeClass('ui-tabs-hide');
			} else {
				emptyContent.addClass('ui-tabs-hide');
			}

		},

		selectById: function (id) {
			this._selectById(id);
		},

		destroy: function () {
		}
	});
})(jQuery);