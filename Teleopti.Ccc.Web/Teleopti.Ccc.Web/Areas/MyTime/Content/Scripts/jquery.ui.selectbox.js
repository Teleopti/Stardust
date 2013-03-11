(function ($) {
	$.widget("ui.selectbox", {

		options: {
			source: null,
			value: null,
			enabled: true,
			opened: false,
			visible: true
		},

		_create: function () {
			var self = this._self = this;
			var select = this._select = this.element;
			var id = select.attr("id");

			var container = this._container = $('<div>')
				.attr("id", id + '-container')
				.addClass(select.attr('class'))
				.addClass('ui-selectbox')
				.insertAfter(select)
				;

			select
				.hide()
				.appendTo(container)
				;

			container.attr('title', select.attr('title'));

			var button = this._button = $('<button></button>')
				.attr("id", id + '-button')
				.attr("disabled", "disabled")
				.addClass('ui-selectbox-button')
				.html('&nbsp;')
				.button({
					icons: { secondary: 'ui-icon-triangle-1-s' },
					text: true,
					disabled: true
				})
				.click(function (e) {
					self._toggleMenu();
				})
				.appendTo(container);

			var menuContainer = this._menuContainer =
				$('<div />')
					.appendTo(container)
					.addClass('ui-selectbox-menu')
					.attr("id", id + '-menu')
				;

			var menu = this._menu = $('<ul />')
				.appendTo(menuContainer)
				;

			$("html").click(function (event) {
				var children = self._button.children();
				if (children[0] == event.target || children[1] == event.target || self._button[0] == event.target)
					return;
				self._displayMenu(false);
			});

			this.option(this.options);

			this._selectOption(select.children(":selected"));

			button.button('enable');
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

		_createOrRefreshMenu: function () {
			var self = this;

			this._createMenuItems();

			if (this._menu.hasClass("ui-menu")) {
				this._menu.menu("refresh");
			} else {
				this._menu.menu({
					select: function (event, ui) {
						var dataItem = ui.item.data("selectbox-item");
						self._selectOption(dataItem.option);
						self._trigger("changed", event, {
							item: dataItem.option
						});
					}
				});
			}

		},

		_refreshMenu: function () {
			var self = this;

			var items = this._select
				.children("option")
				.map(function () {
					var text = $(this).text();
					var value = this.value;
					var color = $(this).data('color');
					return {
						label: text,
						value: value,
						color: color,
						option: this
					};
				});

			this._menu.empty();
			for (var i = 0; i < items.length; i++) {
				var item = this._createMenuItem(items[i], true);
				this._menu.append(item);
			}

			this._menu.menu("refresh");
			this._menu.menu({
				select: function (event, ui) {
					var dataItem = ui.item.data("selectbox-item");
					self._selectOption(dataItem.option);
					self._trigger("changed", event, {
						item: dataItem.option
					});
				}
			});

		},

		_createMenuItems: function () {
			var items = this._select
				.children("option")
				.map(function () {
					var text = $(this).text();
					var value = this.value;
					var color = $(this).data('color');
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

		_createMenuItem: function (item, removable) {

			var self = this._self;

			if (item.label == "-")
				item.label = "";
			if (item.value == "-" && item.label != "") {
				return $('<li></li>')
					.append(item.label);
			}
			if (item.value == "-") {
				return $('<li></li>')
					.addClass('ui-widget-content')
					.addClass('ui-menu-divider')
					.append($('<div>').text(item.label));
			}

			var text;
			var secondaryIcon = null;
			if (item.color) {
				text = $('<span>')
					.text(item.label);
				secondaryIcon = $("<span>")
					.addClass('menu-icon-secondary')
					.addClass('ui-corner-all')
					.css("background-color", item.color);
			} else {
				text = $('<span/>').text(item.label);
			}
			var removableLink = null;
			if (removable && item.value) {
				removableLink = $('<span>')
					.addClass('menu-icon-delete')
					.addClass('floatright')
					.css("color", "grey")
					.append('x')
					.click(function(evt) {
						self._trigger("removeItem", event, {
							value: item.value
						});
						evt.stopPropagation();
					});
			}

			var link = $('<a>')
				.append(text);
			if (secondaryIcon)
				link.append(secondaryIcon);
			if (removableLink) {
				link.append(removableLink);
			}

			var listItem = $('<li>')
				.data("selectbox-item", item)
				.append(link);

			return listItem;
		},

		_mapOptions: function (response) {
			var items = this._select
				.children("option")
				.map(function () {
					var text = $(this).text();
					var value = this.value;
					var color = $(this).data('color');
					return {
						label: text,
						value: value,
						color: color,
						option: this
					};
				});
			response(items);
		},

		_selectOption: function (option) {
			if (option instanceof jQuery) {
				if (option.length == 0)
					return;
				option = option[0];
			}
			var label = $(option).html();
			var value = option.value;
			option.selected = true;
			this._button.button('option', 'label', label);
			this._button.data('value', value);
			this._button.data('label', label);
		},

		_selectValue: function (value) {
			this._selectOption(this._optionByValue(value));
		},

		_optionByValue: function (value) {
			return $('option[value="' + value + '"]', this._select);
		},

		_setOption: function (key, value) {
			switch (key) {
				case "clear":
					// handle changes to clear option
					break;
				case "value":
					this._selectValue(value);
					break;
				case "enabled":
					this._setEnabled(value);
					break;
				case "source":
					this._setSource(value);
					break;
				case "opened":
					this._displayMenu(value);
					break;
				case "visible":
					this._setVisibility(value);
					break;
				case "refreshMenu":
					this._refreshMenu(value);
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

		_setEnabled: function (value) {
			this.options.enabled = value;
			if (value) {
				this._select.removeAttr('disabled');
				this._button.removeAttr('disabled');
			} else {
				this._select.attr('disabled', 'disabled');
				this._button.attr('disabled', 'disabled');
			}
		},

		_setVisibility: function (value) {
			if (value) {
				this._container.show();
			} else {
				this._container.hide();
			}
		},

		_setSource: function (value) {
			this.options.source = value;
			if (value) {
				this._loadSource();
				return;
			}
			this._createOrRefreshMenu();
			this._trigger("rendered");
		},

		_loadSource: function () {
			var self = this;

			var url = this.options.source;
			var select = this._select.empty();
			$.ajax({
				url: url,
				dataType: "json",
				type: "GET",
				global: false,
				cache: false,
				success: function (data, textStatus, jqXHR) {
					$.each(data, function (index, value) {
						var option = $('<option></option>')
							.attr("value", value.Value)
							.text(value.Text);
						select.append(option);
					});
					self._createOrRefreshMenu();
					self._trigger("rendered");
				}
			});
		},

		refresh: function () {
			this._loadSource();
		},

		contains: function (value) {
			return this._optionByValue(value).length > 0;
		},

		select: function (value) {
			this._selectValue(value);
		},

		selectableOptions: function () {
			return $(':not(option[value="-"])', this._select);
		},

		value: function () {
			return this._button.data("value");
		},

		destroy: function () {
		}
	});
})(jQuery);