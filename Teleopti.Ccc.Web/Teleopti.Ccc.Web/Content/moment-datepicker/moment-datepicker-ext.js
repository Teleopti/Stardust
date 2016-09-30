var datepicker = $.fn.datepicker.Constructor.prototype;

datepicker.place = function() {
    var sourceItem = this.component ? this.component : this.element;
    var offset = sourceItem.offset();
    if (this.calendarPlacement === 'left') {
        this.picker.css({
            top: offset.top + this.height,
            left: offset.left + sourceItem[0].offsetWidth - this.picker[0].offsetWidth
        });
    } else {
        this.picker.css({
            top: offset.top + this.height,
            left: offset.left
        });
    }
}