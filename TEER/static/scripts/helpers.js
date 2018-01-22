var Msg = {};
Msg.ERROR_OCCURRED_ON_SERVER = "An unexpected condition has occurred which is preventing the server from fulfilling the request.";
Msg.ERROR_OCCURRED_ON_SERVER_SAVE_FAIL = "An unexpected event on the server is preventing the system from saving the information.";

function BlockUI(message, userUnblockAfterSeconds) {
    if (!message) {
        message = "Loading...";
    }

    var html = ""
        + "<div class='blockUIMessage'>"
        + "<div class='message'>" + message + "</div>"
        + "<div class='animation'>&nbsp;</div>"
        + "     <div data-user-prompt='1' class='hidden' style='margin-top:20px'>"
        + "         <span style='font-size: 14px'>Taking too long to complete?</span>"
        + "         <a href=javascript:void(0); class='error-text' style='font-weight: bold'>Cancel</a>"
        + "     </div>"
        + "</div>";

    var $html = $(html);

    $.blockUI({
        message: $html,
        css: { cursor: 'default', border: 'solid 1px #507CD1', padding: '15px' }
    });

    if ((userUnblockAfterSeconds || 0) > 0) {
        $html.on("click", "a", function (e) {
            e.stopPropagation();
            unblockUI();
        });

        var handle = setTimeout(function () {
            $html.find("div[data-user-prompt=1]").removeClass("hidden");

            clearTimeout(handle);
        }, userUnblockAfterSeconds * 1000);
    }
}

function unblockUI() {
    $.unblockUI();
}

function ProgressElement(message) {
    if (!message) {
        message = "Loading...";
    }

    return $("<div class='blockUIMessage' style='margin-top:20px'>" + message + "<div class='animation'>&nbsp;</div></div>");
}

function InvokeButtonClick(button) {
    disableTimer();

    BlockUI("Refreshing...");

    button.removeAttr("disabled");
    button.trigger("click");
    button.attr("disabled", "disabled");
}

function RestrictToIntegers(event) {
    if (IsSpecialKey(event.keyCode)) {
        return;
    }

    var validChars = "0123456789"
    var c = String.fromCharCode(event.keyCode);
    if (c && validChars.indexOf(c) === -1) {
        event.preventDefault();
    }
}

function IsNumeric(text) {
    var validChars = "0123456789";
    if (!(text && text.length))
        return false;

    for (i = 0; i < text.length; i++) {
        if (validChars.indexOf(text.charAt(i)) === -1)
            return false;
    }

    return true;
}

function IsTime24(time) {
    var currVal = time;
    if (currVal === null || typeof (currVal) == 'undefined' || currVal.length === 0) {
        return false;
    }

    var timeParts = time.split(':');
    if (timeParts.length !== 2) {
        return false;
    }

    var hour = timeParts[0];
    var min = timeParts[1];

    if (!IsNumeric(hour) || !IsNumeric(min)) {
        return false;
    }

    if (hour < 0 || hour > 23 || min < 0 || min > 59) {
        return false;
    }

    return true;
}

function IsSpecialKey(keyCode) {
    if (keyCode === KEYCODE.BACKSPACE
                || keyCode === KEYCODE.ENTER
                || keyCode === KEYCODE.TAB
                || keyCode === KEYCODE.SHIFT
                || keyCode === KEYCODE.CTRL
                || keyCode === KEYCODE.LEFTARROW
                || keyCode === KEYCODE.UPARROW
                || keyCode === KEYCODE.RIGHTARROW
                || keyCode === KEYCODE.DOWNARROW
                || keyCode === KEYCODE.DELETE
                || keyCode === KEYCODE.HOME
                || keyCode === KEYCODE.END
                ) {
        return true;
    }
    else {
        return false;
    }
}