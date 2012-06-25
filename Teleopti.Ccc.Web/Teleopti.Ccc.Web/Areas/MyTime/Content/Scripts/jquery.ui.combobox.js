(function ($) {
	$.widget("ui.combobox", {
		_create: function () {
			var width = this.element.getWidth();
			var self = this,
					select = this.element.hide(),
					selected = select.children(":selected"),
					value = selected.val() ? selected.text() : "";
			var input = this.input = $('<input type="text">')
					.insertAfter(select)
					.attr("id", select.attr('id') + '-input')
					.val(value)
					.width(width)
					.addClass(select.attr('class'))
					.autocomplete({
						delay: 0,
						minLength: 0,
						source: function (request, response) {
							var matcher = new RegExp($.ui.autocomplete.escapeRegex(request.term), "i");
							response(select.children("option").map(function () {
								var text = $(this).text();
								if (this.value && (!request.term || matcher.test(text)))
									return {
										label: text.replace(
											new RegExp(
												"(?![^&;]+;)(?!<[^<>]*)(" +
												$.ui.autocomplete.escapeRegex(request.term) +
												")(?![^<>]*>)(?![^&;]+;)", "gi"
											), "<strong>$1</strong>"),
										value: text,
										option: this
									};
							}));
						},
						select: function (event, ui) {
							ui.item.option.selected = true;
							self._trigger("selected", event, {
								item: ui.item.option
							});
						},
						change: function (event, ui) {
							if (!ui.item) {
								var matcher = new RegExp("^" + $.ui.autocomplete.escapeRegex($(this).val()) + "$", "i"),
									valid = false;
								select.children("option").each(function () {
									if ($(this).text().match(matcher)) {
										this.selected = valid = true;
										return false;
									}
								});
								// Do not clear even if not found in select
								return false;
							}
						}
					})
					.addClass("ui-widget ui-widget-content ui-corner-left")
					.keydown(function (event) {
						if (event.keyCode == 13) {
							//Prevent refresh in IE
							event.preventDefault();
							return false;
						}
					})
				;
			input[0].defaultValue = value;

			input.data("autocomplete")._renderItem = function (ul, item) {
				return $("<li></li>")
						.data("item.autocomplete", item)
						.append("<a>" + item.label + "</a>")
						.appendTo(ul);
			};

			input.data("autocomplete").menu.element.addClass(select.attr('class'));

			this.button = $("<button type='button'>&nbsp;</button>")
					.attr("tabIndex", -1)
					.attr("title", "Show All Items")
					.insertAfter(input)
					.button({
						icons: {
							primary: "ui-icon-triangle-1-s"
						},
						text: false
					})
					.removeClass("ui-corner-all")
					.addClass("ui-corner-right ui-button-icon")
					.click(function () {
						// close if already visible
						if (input.autocomplete("widget").is(":visible")) {
							input.autocomplete("close");
							return;
						}

						// work around a bug (likely same cause as #5265)
						$(this).blur();

						// pass empty string as value to search for, displaying all results
						input.autocomplete("search", "");
						input.focus();
					});
		},

		destroy: function () {
			this.input.remove();
			this.button.remove();
			this.element.show();
			$.Widget.prototype.destroy.call(this);
		},
		set: function (value) {
			$(this.input).val(value);
			$(this.input).trigger('change');
		}
	});
})(jQuery);