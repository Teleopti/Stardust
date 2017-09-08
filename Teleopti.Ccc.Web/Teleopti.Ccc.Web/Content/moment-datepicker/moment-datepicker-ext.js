var datepicker = $.fn.datepicker.Constructor.prototype;

datepicker.place = function() {
    var sourceItem = this.component ? this.component : this.element;
    var offset = sourceItem.offset();

    var topPos = offset.top + this.height;
    var calHeight = 260; //datepicker's height: 260

    if(topPos + calHeight > window.innerHeight){
        topPos = offset.top - calHeight;
    }

    if (this.calendarPlacement === 'left') {
        var leftValue = offset.left + sourceItem[0].offsetWidth - this.picker[0].offsetWidth;

        if (leftValue < 1) {
            leftValue = offset.left + sourceItem[0].offsetWidth - sourceItem[0].parentElement.parentElement.offsetWidth;
        }
        this.picker.css({
            top: topPos,
            left: leftValue
        });
    } else if(this.calendarPlacement === 'center'){
        this.picker.css({
            transform: 'translateX(calc(-50% + ' + (sourceItem.innerWidth() / 2).toFixed(0) +'px))',
            top: topPos,
            left: offset.left
        });
    } else {
        this.picker.css({
            top: topPos,
            left: offset.left
        });
    }
};

datepicker.show = function (e) {
    if (this.isInput && this.element.is(':disabled')) { return; }

    else if (this.element.children('input').is(':disabled')) { return; }

    if (!this.isInput && this.picker.isOpened){
        this.picker.hide();
        return;
    }

    $('button[data-bind^=\'datepicker\']').each(function (index, item) {
        var datepickerObj = $(item).data("datepicker");
        datepickerObj && datepickerObj.hide();
    });

    this.picker.show();
    this.picker.isOpened = true;

    this.height = (this.component && this.component.outerHeight()) || this.element.outerHeight();
    this.place();
    $(window).on('resize', $.proxy(this.place, this));
    if (e) {
        e.stopPropagation();
        e.preventDefault();
    }
    if (!this.isInput) {
        $(document).on('mouseup', $.proxy(this.hide, this));
    }

    this.element.trigger({
        type: 'show'
    });
};

datepicker.hide = function () {
    this.picker.hide();
    $(window).off('resize', this.place);
    this.viewMode = this.startViewMode;
    this.showMode();
    if (!this.isInput) {
        $(document).off('click', this.hide);
    }
    this.refresh();
    this.element.trigger({
        type: 'hide'
    });
    this.picker.isOpened = false;
};