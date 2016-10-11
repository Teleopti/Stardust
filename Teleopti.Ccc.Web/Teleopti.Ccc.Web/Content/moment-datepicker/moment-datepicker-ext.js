var datepicker = $.fn.datepicker.Constructor.prototype;

datepicker.place = function() {
    var sourceItem = this.component ? this.component : this.element;
    var offset = sourceItem.offset();
    if (this.calendarPlacement === 'left') {
        var leftValue = offset.left + sourceItem[0].offsetWidth - this.picker[0].offsetWidth;

        if (leftValue < 1) {
            leftValue = offset.left + sourceItem[0].offsetWidth - sourceItem[0].parentElement.parentElement.offsetWidth;
        }
        this.picker.css({
            top: offset.top + this.height,
            left: leftValue
        });
    } else {
        this.picker.css({
            top: offset.top + this.height,
            left: offset.left
        });
    }
}