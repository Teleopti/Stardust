﻿(function ($) {
	$.widget("ui.selectbox", {

		options: {
			source: null,
			value: null
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
					// close if already visible
					if (menu.autocomplete("widget").is(":visible")) {
						menu.autocomplete("close");
						return;
					}
					// work around a bug (likely same cause as #5265)
					$(this).blur();
					// pass empty string as value to search for, displaying all results
					menu.autocomplete("search", "");
					menu.focus();
					e.stopPropagation();
				})
				.appendTo(container)
				;

			var menu = this._menu = $('<div>').appendTo(container);
			menu.attr("id", id + '-menu')
				.addClass('ui-selectbox-menu')
				.autocomplete({
					appendTo: menu,
					minLength: 0,
					select: function (event, ui) {
						self._selectOption(ui.item.option);
						self._trigger("changed", event, {
							item: ui.item.option
						});
					},
					source: function (request, response) {
						if (self.options.source == null)
							self._mapOptions(response);
						else
							self._refresh(function () {
								self._mapOptions(response);
							});
					}
				})
				.click(function (e) {
					e.stopPropagation();
				})
				;

			menu.data("autocomplete")._renderItem = function (ul, item) {
				if (item.label == "-")
					item.label = "";
				if (item.value == "-" && item.label != "") {
					return $('<li></li>')
						.addClass('ui-selectbox-menu-splitter')
						.append(item.label)
						.appendTo(ul);
				}
				if (item.value == "-") {
					return $('<li></li>')
						.addClass('ui-selectbox-menu-splitter')
						.append($('<div>').text(item.label))
						.appendTo(ul);
				}

				if (item.color) {
					var text = $('<span>')
						.text(item.label);
					var secondaryIcon = $("<span>")
						.addClass('menu-icon-secondary')
						.addClass('ui-corner-all')
						.css("background-color", item.color);
					var itemButton = $("<a></a>")
						.append(text)
						.append(secondaryIcon);
					return $("<li></li>")
						.data("item.autocomplete", item)
						.append(itemButton)
						.appendTo(ul);
				}
				return $("<li></li>")
					.data("item.autocomplete", item)
					.append($("<a></a>").text(item.label))
					.appendTo(ul);




			};

			this._selectOption(select.children(":selected"));

			$("html").click(function () {
				if (menu.autocomplete("widget").is(":visible")) {
					menu.autocomplete("close");
				}
			})
			;

			button.button('enable');
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

		_refresh: function (success) {
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
					success();
				}
			});
		},

		_setEnabled: function (value) {
			if (value) {
				this._button.removeAttr('disabled');
			} else {
				this._button.attr('disabled', 'disabled');
			}
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
			}

			if (this._super)
			// In jQuery UI 1.9 and above, you use the _super method instead
				this._super("_setOption", key, value);
			else
			// In jQuery UI 1.8, you have to manually invoke the _setOption method from the base widget
				$.Widget.prototype._setOption.apply(this, arguments);
		},

		_setEnabled: function (value) {
			if (value) {
				this._select.removeAttr('disabled');
				this._button.removeAttr('disabled');
			} else {
				this._select.attr('disabled', 'disabled');
				this._button.attr('disabled', 'disabled');
			}
		},

		refresh: function (success) {
			this._refresh(success);
		},

		contains: function (value) {
			return this._optionByValue(value).length > 0;
		},

		select: function (value) {
			this._selectValue(value);
		},

		hide: function () {
			this._container.hide();
		},

		show: function () {
			this._container.show();
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