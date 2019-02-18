
/*
* Depends:
*	jquery.ui.core.js
*	jquery.ui.mouse.js
*	jquery.ui.widget.js
*	jquery.ui.selectable.js
*/

(function ($, undefined) {
	$.widget("ui.calendarselectable", $.ui.selectable, {
		options: {
			distance: 0,
			filter: "ul.calendarview-week li.editable",
			cancel: "ul.weekdays li, ul.calendarview-week li.non-editable"
		},
		_create: function () {
			var self = this;
			self.options.stop = function (event, ui) {
				self._handleSelected(event, ui);
			};
			this.update = function(event) {
				self._handleSelected(event);
			};
			this.selectByDate = function (date) {
				var cells = $(self.options.filter, self.element[0]);
				cells.each(function () {
					var cell = $(this);
					if (cell.data('mytime-date') == date) {
						cell.addClass('ui-selecting');
					}
				});
				$.ui.selectable.prototype._mouseStop.call(this, null);
			};

			$.ui.selectable.prototype._create.call(this);

			$.ui.selectable.prototype.options.stop = function (event, ui) {
				self._handleSelected(event, ui);
			};
		},
		_handleSelected: function (event) {
			var self = this;
			var dates = [];
			$('.ui-selected', this.element[0]).each(function () {
				dates.push($(this).data('mytime-date'));
			});
			self._trigger("datesChanged", event, {
				dates: dates
			});
		},
		destroy: function () {
			$.ui.selectable.prototype.destroy.call(this);
		}
	});
	$.extend($.ui.calendarselectable, {
		version: "1.0"
	});
})(jQuery);
