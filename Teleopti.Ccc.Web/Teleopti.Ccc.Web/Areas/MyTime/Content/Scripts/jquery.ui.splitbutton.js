(function ($) {
	$.widget("ui.splitbutton", {

		options: {
			opened: false
		},

		_create: function () {
			var self = this;
			var buttonset = this.element
				.addClass('ui-splitbutton')
				;
			var button = $('button', buttonset)
				.addClass('ui-splitbutton-button')
				.html('&nbsp;')
				.button({
					text: true
				})
				.click(function () {
					var value = $(this).data('value');
					var label = $(this).data('label');
					if (value) {
						self._trigger("clicked", event, {
							label: label,
							value: value
						});
					}
				})
				;
			var select = this._select =
				$('select', buttonset)
					.hide();
			var listButton = $('<button type="button">&nbsp;</button>')
				.attr("title", select.attr("title"))
				.attr("id", buttonset.attr('id') + '-list-button')
				.height(20)
				.insertAfter(button)
				.removeClass("ui-corner-all")
				.addClass('ui-splitbutton-list-button')
				.addClass("ui-corner-right ui-button-icon")
				.button({
					text: false,
					icons: {
						primary: 'ui-icon-triangle-1-s'
					}
				})
				.click(function (e) {
					self._toggleMenu();
					e.stopPropagation();
				})
				;
			buttonset.buttonset();

			var menuContainer = this._menuContainer =
				$('<div />')
					.insertAfter(listButton)
					.addClass('ui-splitbutton-menu')
					.attr("id", buttonset.attr('id') + '-menu')
				;

			var menu = this._menu = $('<ul />')
				.appendTo(menuContainer);

			this._createMenuItems();

			menu.menu({
				select: function (event, ui) {
					var item = ui.item.data("splitbutton-item");
					button.button('option', 'label', $('<div/>').text(item.label).html());
					button.data('value', item.value);
					button.data('label', item.label);
					self._trigger("clicked", event, {
						label: item.label,
						value: item.value
					});
					self._displayMenu(false);
				}
			});

			this.option(this.options);

		},

		_toggleMenu: function () {
			this._displayMenu(!this.options.opened);
		},

		_displayMenu: function (value) {
			this.options.opened = value;
			if (value) {
				this._menu.show();
			} else
				this._menu.hide();
		},

		_createMenuItems: function () {
			var items = this._select
				.children("option").map(function () {
					var text = $(this).text();
					var value = $(this).val();
					var color = $(this).attr('data-color');
					return {
						label: text,
						value: value,
						color: color,
						option: this
					};
				});

			this._menu.empty();
			for (var i = 0; i < items.length; i++) {
				var item = this._createMenuItem(items[i]);
				this._menu.append(item);
			}
		},

		_createMenuItem: function (item) {

			if (item.value == "-")
				return $('<li></li>');

			var text = $('<span>')
					.addClass('ui-splitbutton-menu-text')
					.text(item.label)
					;
			var secondaryIcon = $("<span>")
					.addClass('ui-splitbutton-menu-icon-secondary')
					.addClass('ui-corner-all')
					.css("background-color", item.color)
					;
			var itemButton = $("<a></a>")
					.append(text)
					.append(secondaryIcon)
					;
			return $("<li></li>")
					.data("splitbutton-item", item)
					.append(itemButton)
					;
		},

		_setOption: function (key, value) {
			switch (key) {
				case "clear":
					// handle changes to clear option
					break;
				case "opened":
					this._displayMenu(value);
					break;
			}

			if (this._super)
			// In jQuery UI 1.9 and above, you use the _super method instead
			{
				this._super("_setOption", key, value);
			}
			else
			// In jQuery UI 1.8, you have to manually invoke the _setOption method from the base widget
			{
				$.Widget.prototype._setOption.apply(this, arguments);
			}
		},

		destroy: function () {
		}
	});
})(jQuery);