var timepicker = $.fn.timepicker.Constructor.prototype;

timepicker._init = function() {
	var self = this;
	if (this.$element.hasClass('right-align')) {
		this.rightAlign = true;
	} else {
		this.rightAlign = false;
	}

	if (this.$element.parent().hasClass('input-group') || this.$element.parent().hasClass('input-group')) {
		this.$element.parent('.input-group, .input-group').find('.input-group-addon').on({
			'mousedown.timepicker': $.proxy(this.showWidget, this)
		});
		this.$element.on({
			'focus.timepicker': $.proxy(this.highlightUnit, this),
			'click.timepicker': $.proxy(this.highlightUnit, this),
			'keydown.timepicker': $.proxy(this.elementKeydown, this),
			'blur.timepicker': $.proxy(this.blurElement, this)
		});
	} else {
		if (this.template) {
			this.$element.on({
				//disable this focus binding cause it will tigger showWidget() twice at first 
				// 'focus.timepicker': $.proxy(this.showWidget, this),
				'click.timepicker': $.proxy(this.showWidget, this),
				'blur.timepicker': $.proxy(this.blurElement, this)
			});
		} else {
			this.$element.on({
				'focus.timepicker': $.proxy(this.highlightUnit, this),
				'click.timepicker': $.proxy(this.highlightUnit, this),
				'keydown.timepicker': $.proxy(this.elementKeydown, this),
				'blur.timepicker': $.proxy(this.blurElement, this)
			});
		}
	}

	if (this.template !== false) {
		this.$widget = $(this.getTemplate()).prependTo(this.$element.parents(this.appendWidgetTo)).on('click', $.proxy(this.widgetClick, this));
		$('.bootstrap-timepicker-hour, .bootstrap-timepicker-minute, .bootstrap-timepicker-second, .bootstrap-timepicker-meridian', this.$widget)
			.on('blur', $.proxy(function() {
				this.updateFromWidgetInputs();
			}, this));
	} else {
		this.$widget = false;
	}

	if (this.showInputs && this.$widget !== false) {
		this.$widget.find('input').each(function() {
			$(this).on({
				'click.timepicker': function() {
					$(this).select();
				},
				'keydown.timepicker': $.proxy(self.widgetKeydown, self)
			});
		});
	}

	this.setDefaultTime(this.defaultTime);
};

timepicker.showWidget = function() {
	if (this.isOpen) {
		this.hideWidget();
		return;
	}

	if (this.$element.is(':disabled')) {
		return;
	}

	var self = this;
	$(document).on('mousedown.timepicker, touchend.timepicker', function(e) {
		// Clicked outside the timepicker, hide it
		if ($(e.target).closest('.bootstrap-timepicker-widget').length === 0  && $(e.target).closest('.bootstrap-timepicker .input-group-btn').length === 0 && $(e.target).closest('.bootstrap-timepicker .input-group-addon').length === 0) {
			self.hideWidget();
		}
	});

	this.$element.trigger({
		'type': 'show.timepicker',
		'time': {
			'value': this.getTime(),
			'hours': this.hour,
			'minutes': this.minute,
			'seconds': this.second,
			'meridian': this.meridian
		}
	});

	if (this.disableFocus) {
		this.$element.blur();
	}

	this.updateFromElementVal();

	if (this.template === 'modal' && this.$widget.modal) {
		this.$widget.modal('show').on('hidden', $.proxy(this.hideWidget, this));
	} else {
		if (this.isOpen === false) {
			this.$widget.addClass('open');
		}
	}

	this.isOpen = true;
};