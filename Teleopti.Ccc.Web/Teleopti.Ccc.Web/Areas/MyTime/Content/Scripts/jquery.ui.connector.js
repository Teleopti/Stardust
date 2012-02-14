(function ($) {
	$.widget("ui.connector", {

		options: {
			parentSelector: 'li'
		},

		_self: null,
		_connector: null,
		_arrow: null,
		_parent: null,

		isConnected: function () {
			return this._connector.data('connected');
		},

		_setConnected: function (value) {
			if (value != undefined)
				this._connector.data('connected', value);
			else
				this._connector.data('connected', true);
		},

		_create: function () {
			this._self = this;
			var self = this._self;

			this._connector = this.element
				.css({
					'opacity': '0' //because IE cant set opacity from the css file
				})
				.addClass('ui-connector')
				;

			this._arrow = $('<div></div>')
				.addClass('ui-arrow-right')
				.appendTo(this._connector)
				;

			this._parent = this._connector
				.parent(this.options.parentSelector)
				.hover(function () {
					if (self.isConnected())
						return;
					self._slideInConnector();
				},
				function () {
					if (self.isConnected())
						return;
					self._fadeConnector();
				})
				;

		},

		_fadeConnector: function () {
			this._connector
				.animate({
					'right': '0',
					'opacity': '0'
				}, {
					queue: false,
					duration: 200
				})
				;
		},

		_slideInConnector: function () {
			this._connector
				.animate({
					'right': '-15',
					'opacity': '100'
				}, {
					queue: false,
					duration: 200
				})
				;
		},

		connecting: function () {
			this._setConnected();
			this._arrow
				.removeClass('ui-arrow-right')
				.addClass('ui-connecting')
				;
		},

		connect: function () {
			this._setConnected();
			this._connector
				.animate({
					'right': '+=8',
					'border-right': 0
				})
				;
			this._arrow
				.hide()
				.addClass('ui-arrow-right')
				.removeClass('ui-connecting')
				;
		},

		disconnect: function () {
			this._setConnected(false);
			this._arrow
				.show()
				.css('border-right', '1px solid rgba(0, 0, 0, 0.15)')
				;
			this._fadeConnector();
		},

		destroy: function () {

		}
	});
})(jQuery);