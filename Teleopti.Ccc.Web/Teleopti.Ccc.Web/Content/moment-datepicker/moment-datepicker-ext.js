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
}