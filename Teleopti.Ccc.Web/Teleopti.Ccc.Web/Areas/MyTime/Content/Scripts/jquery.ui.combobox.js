(function ($) {
	$.widget("ui.combobox", {
		_create: function () {
			var self = this._self = this;

			var width = this.element.getWidth();
			var select = this.element.hide();
			var selected = select.children(":selected");
			var value = this.value = selected.val() ? selected.text() : "";
			var input = this.input = $('<input type="text">')
				.insertAfter(select)
				.attr("id", select.attr('id') + '-input')
				.val(value)
				.width(width)
				.addClass("ui-combobox")
				.addClass(select.attr('class'))
				.blur(function () {
					self._setValue(self.input.val());
				})
				.autocomplete({
					delay: 0,
					minLength: 0,
					source: function (request, response) {
						var matcher = new RegExp($.ui.autocomplete.escapeRegex(request.term), "i");
						response(select.children("option").map(function () {

							var text = $(this).text();
							var html = $('<div/>').text(text).html();

							if (request.term) {
								// if we have a search term, we have to match that BEFORE we encode, or we will match encoded characters
								// although I dont understand the regex here (its from jquery examples I think) I think it might have been
								// designed to work, but it surely doesnt when a value have < or > in it.
								// MS/KM
								var matchedText = text.replace(
									new RegExp(
										"(?![^&;]+;)(?!<[^<>]*)(" +
											$.ui.autocomplete.escapeRegex(request.term) +
												")(?![^<>]*>)(?![^&;]+;)", "gi"
									), "**match**$1**endmatch**");
								var encodedText = $('<div/>').text(matchedText).html();
								html = encodedText.replace(/\*\*match\*\*/g, "<strong>").replace(/\*\*endmatch\*\*/g, "</strong>");
							}

							if (this.value && (!request.term || matcher.test(text)))
								return {
									label: html,
									value: text,
									option: this
								};
						}));
					},
					select: function (event, ui) {
						ui.item.option.selected = true;
						self._setValue(ui.item.value);
					},
					change: function (event, ui) {
						self._setValue(self.input.val());
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
				});

			input.attr('title', select.attr('title'));
			input[0].defaultValue = value;

			input.data("ui-autocomplete")._renderItem = function (ul, item) {
				return $("<li></li>")
									.data("ui-autocomplete-item", item)
									.append("<a>" + item.label + "</a>")
									.appendTo(ul);
			};

			input.data("ui-autocomplete").menu.element.addClass(select.attr('class'));

			this.button = $("<button type='button'>&nbsp;</button>")
				.attr("tabIndex", -1)
				.addClass("ui-combobox-button")
				.attr("title", select.attr('title'))
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

		_setValue: function (value) {
			this.value = value;
			this.element.val(value);
			this.input.val(value);
			this._trigger("changed", null, { value: value });
		},

		_setEnabled: function (value) {
			if (value) {
				this.input.removeAttr('disabled');
				this.button.removeAttr('disabled');
			} else {
				this.input.attr('disabled', 'disabled');
				this.button.attr('disabled', 'disabled');
			}
		},

		_setOption: function (key, value) {
			switch (key) {
				case "clear":
					// handle changes to clear option
					break;
				case "value":
					this._setValue(value);
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

		destroy: function () {
			this.input.remove();
			this.button.remove();
			this.element.show();
			$.Widget.prototype.destroy.call(this);
		}

	});
})(jQuery);