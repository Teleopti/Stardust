
Teleopti.MyTimeWeb.AsmMessageDetail = (function ($) {

    function _showCommunication(position) {
        _hideEditSection();
        _showEditSection(position);
    }

    function _showEditSection(position) {
        var topPosition = $('#AsmMessages-list').position().top - 1;
        if (!position)
            position = topPosition;
        if (position < topPosition)
            position = topPosition;
        $('.asmMessage-edit-section')
			.css({
			    'top': position
			})
			.fadeIn()
			;
    }

    function _hideEditSection() {
        $('.asmMessage-edit-section')
			.hide()
			;
    }

    function _fadeEditSection(func) {
        $('.asmMessage-edit-section')
            .fadeOut(400, func)
            ;
    }

    return {
        Init: function () {
        },
        HideEditSection: function () {
            _hideEditSection();
        },
        FadeEditSection: function (func) {
            _fadeEditSection(func);
        },
        ShowAsmMessage: function (position) {
            _showCommunication(position);
        }
    };

})(jQuery);

