var datepicker = $.fn.datepicker.Constructor.prototype;

datepicker.place = function() {
    var sourceItem = this.component ? this.component : this.element;
    var offset = sourceItem.offset();
    if (this.calendarPlacement === 'left') {
        this.picker.css({
            top: offset.top + this.height,
            left: this.element.parents()[1].offsetLeft
        });
    } else {
        this.picker.css({
            top: offset.top + this.height,
            left: offset.left
        });
    }
}