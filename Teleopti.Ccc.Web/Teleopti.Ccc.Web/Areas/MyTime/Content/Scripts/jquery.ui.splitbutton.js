(function ($) {
	$.widget("ui.splitbutton", {
		_create: function () {
			var self = this;
			var buttonset = this.element
				.addClass('ui-splitbutton')
				;
			var button = $('button', buttonset)
				.addClass('ui-splitbutton-button')
				.html('&nbsp;')
				.height(20)
				.width(100)
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
			var select = $('select', buttonset)
				.hide()
				;
			var listButton = $('<button type="button">&nbsp;</button>')
				.attr("title", select.attr("title"))
				.attr("id", buttonset.attr('id') + '-list-button')
				.height(20)
				.insertAfter(button)
				.removeClass("ui-corner-all")
				.addClass("ui-corner-right ui-button-icon")
				.button({
					text: false,
					icons: {
						primary: 'ui-icon-triangle-1-s'
					}
				})
				.click(function () {
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
				})
				;
			buttonset.buttonset();

			var menu = $('<div>');
			menu.attr("id", buttonset.attr('id') + '-menu')
				.addClass('ui-splitbutton-menu')
				.autocomplete({
					appendTo: menu,
					minLength: 0,
					select: function (event, ui) {
						button.button('option', 'label', $('<div/>').text(ui.item.label).html());
						button.data('value', ui.item.value);
						button.data('label', ui.item.label);
						self._trigger("clicked", event, {
							label: ui.item.label,
							value: ui.item.value
						});
					},
					source: function (request, response) {
						response(select.children("option").map(function () {
							var text = $(this).text();
							var value = $(this).val();
							var bgColor = $(this).attr('bg-color');
							return {
								label: text,
								value: value,
								bgColor: bgColor,
								option: this
							};
						}));
					}
				})
				.insertAfter(listButton)
				;

			menu.data("autocomplete")._renderItem = function (ul, item) {
				if (item.value == "-")
					return $('<li></li>')
						.addClass('ui-splitbutton-menu-splitter')
						.append($('<div>'))
						.appendTo(ul)
						;

				var text = $('<span>')
					.addClass('ui-splitbutton-menu-text')
					.text(item.label)
					;
				var secondaryIcon = $("<span>")
					.addClass('ui-splitbutton-menu-icon-secondary')
					.addClass('ui-corner-all')
					.css("background-color", item.bgColor)
					;
				var itemButton = $("<a></a>")
					.append(text)
					.append(secondaryIcon)
					;
				return $("<li></li>")
					.data("item.autocomplete", item)
					.append(itemButton)
					.appendTo(ul)
					;
			};

		},

		destroy: function () {
		}
	});
})(jQuery);