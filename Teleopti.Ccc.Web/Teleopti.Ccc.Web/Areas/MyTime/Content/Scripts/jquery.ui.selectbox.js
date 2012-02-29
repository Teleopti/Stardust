(function ($) {
	$.widget("ui.selectbox", {

		options: {
			source: null
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

			var button = this._button = $('<button></button>')
				.attr("id", id + '-button')
				.addClass('ui-selectbox-button')
				.html('&nbsp;')
				.button({
					icons: { secondary: 'ui-icon-triangle-1-s' },
					text: true
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

			var menu = this._menu = $('<div>');
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
				.appendTo(container)
				;

			menu.data("autocomplete")._renderItem = function (ul, item) {
				if (item.value == "-") {
					return $('<li></li>')
						.addClass('ui-selectbox-menu-splitter')
						.append($('<div>').text(item.label))
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
		},

		_mapOptions: function (response) {
			var items = this._select
				.children("option")
				.map(function () {
					console.log(('mapping option ' + this.value));
					var text = $(this).text();
					var value = this.value;
					return {
						label: text,
						value: value,
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

		destroy: function () {
		}
	});
})(jQuery);