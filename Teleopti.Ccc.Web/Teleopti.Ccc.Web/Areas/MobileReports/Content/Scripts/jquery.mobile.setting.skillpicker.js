(function ($, undefined) {
	$.widget("mobile.skillpicker", $.extend({}, $.mobile.selectmenu.prototype, {
		myoptions: {
			allId: '-2',
			allIdCsv: ',-2,',
			values: null
		},
		_create: function () {
			this.element.data('selectmenu', this.element.data('skillpicker'));
			$.mobile.selectmenu.prototype._create.apply(this, arguments);
			var self = this, o = this.myoptions, caller = this.element;
			o.values = self._spCurrentValues();
			$(caller).bind('change', function () { self._spChange(); });
		},
		_spHasAllChanged: function () {
			var o = this.myoptions;
			var values = this._spCurrentValues();
			return o.values.indexOf(o.allIdCsv) != values.indexOf(o.allIdCsv);
		},
		_spCurrentValues: function () {
			return ',' + (this.element.val() || []).join(',') + ',';
		},
		_spUpdateAll: function (val) {
			this.element.find('option').each(function () {
				var opt = $(this);
				opt.prop('selected', val);
			});
		},
		_spChange: function () {
			var self = this,
				caller = this.element,
				o = this.myoptions;
			if (self._spHasAllChanged()) {
				var check = self._spCurrentValues().indexOf(o.allIdCsv) >= 0;
				self._spUpdateAll(check);
			} else {
				caller.find('option[value="' + o.allId + '"]').prop('selected', false);
			}
			o.values = this._spCurrentValues();
			caller.skillpicker('refresh', true);
		},
		setButtonText: function () { // all this to override Text...
			var self = this, o = self.myoptions, caller = this.element, selected = this.selected();
			var all = selected.filter(function () { return $(this).attr('value') == o.allId; });
			if (all.length) {
				this.button.find(".ui-btn-text").text(all.text());
				return;
			}
			$.mobile.selectmenu.prototype.setButtonText.apply(this);
		},
		setButtonCount: function () { 
			var self = this, o = self.myoptions, selected = this.selected();
			var all = selected.filter(function () { return $(this).attr('value') == o.allId; });
			if (all.length) {
				this.buttonCount["show"]().text(selected.length - 1);
				return;
			}
			$.mobile.selectmenu.prototype.setButtonCount.apply(this);
		}
	}));
})(jQuery);