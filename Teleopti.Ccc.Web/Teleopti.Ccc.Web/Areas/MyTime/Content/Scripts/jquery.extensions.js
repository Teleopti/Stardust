
$.fn.extend({
	reset: function () {
		return this.each(function () {
			var defaultValue = this.defaultValue;
			if ($(this)[0].type == "hidden")
				defaultValue = '';
			if (!defaultValue)
				defaultValue = $(this).data('default-value');
			if (defaultValue)
				$(this).val(defaultValue);
			else
				$(this).val('');
			$(this).change();
		});
	}
});

jQuery.fn.exists = function () { return (this.length > 0); };

$.fn.extend({
	getHeight: function () {
		var element = $(this[0]);
		element.css({ 'visibility': 'hidden', 'display': 'block' });
		var height = element.height();
		element.removeAttr('style');
		return height;
	},
	getWidth: function () {
		var element = $(this[0]);
		element.css({ 'visibility': 'hidden', 'display': 'block' });
		var height = element.width();
		element.removeAttr('style');
		return height;
	}
});

$.fn.extend({
	hasHiddenContent: function () {
		var total = this[0].scrollWidth;
		var visible = this[0].clientWidth;
		return (total != visible);
	}
});