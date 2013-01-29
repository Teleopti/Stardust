(function ($) {
	$.widget("ui.tabberiet", {

		options: {
			click: null,
			emptyContentSelector: null
		},

		_create: function () {
			var self = this._self = this;
			var element = this._element = this.element;

			// Enable tabs on all tab widgets. The `event` property must be overridden so
			// that the tabs aren't changed on click, and any custom event name can be
			// specified. Note that if you define a callback for the 'select' event, it
			// will be executed for the selected tab whenever the hash changes.
			element.tabs({ collapsible: true, event: 'change', active: false });

			var tabs = element.find('ul.ui-tabs-nav a');
			tabs.click(self.options.click);

			this._emptyContent = this._element
				.find(self.options.emptyContentSelector)
				;

		},

		_selectById: function (id) {
			var tab = this._element
				.find('ul.ui-tabs-nav a')
				.filter((function () { return this.hash == id; }))
				;
			var tabContent = $(id);

			// if no tab was found, select no tab
			if (tab.length == 0) {
				this._element.tabs("option", "active", false);
				// seems to be the only way to "unselect" the selected tab
				this._element.find(".ui-tabs-active")
					.removeClass("ui-state-active")
					.removeClass("ui-tabs-active")
					;
			} else {
				tab.triggerHandler('change');
			}

			// if no content was found, show empty content
			if (tabContent.length == 0) {
				this._element
					.find('.ui-tabs-panel')
					.hide()
					;
				this._emptyContent.show();
			} else {
				this._emptyContent.hide();
			}

		},

		selectById: function (id) {
			this._selectById(id);
		},

		destroy: function () {
		}
	});
})(jQuery);