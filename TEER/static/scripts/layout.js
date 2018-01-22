$(document).on("ready.sitemaster", masterDocReady);

function masterDocReady() {
    $("input:text,input:password").addClass("input-text");
    $("input:text[readonly]").addClass("readonly");
    $("input:button,input:submit").addClass("button");

    // disable browser auto-fill
    $("input:text").attr("autocomplete", "off");

    // apply jqueryUI styling to buttons
    $("input.button:not([data-primary-icon]),button.button:not([data-primary-icon]),a.button:not([data-primary-icon])").button();
    $("a.button.aspNetDisabled").button("option", "disabled", true);

    $("input.button[data-primary-icon],a.button[data-primary-icon]").each(function () {
        $this = $(this);

        var primaryIcon = $this.attr("data-primary-icon");

        if (primaryIcon) {
            var iconOnly = $this.attr("data-icon-only");
            if (iconOnly == "true" || iconOnly == "1") {
                iconOnly = true;
            }
            else {
                iconOnly = false;
            }

            $this.button({ icons: { primary: primaryIcon }, text: !iconOnly });
        }
        else {
            $this.button();
        }
    });

    // prevent browser back navigation
    $("input[readonly],textarea[readonly],select").keydown(function (e) {
        if (e.keyCode == KEYCODE.BACKSPACE) {
            e.preventDefault();
        }
    });

    // set defaults;
    //$.blockUI.defaults.fadeIn = 0;
    //$.blockUI.defaults.fadeOut = 0;

    // apply mask and attach datepickers
    $("input.masked-input-date")
        .not("[readonly=readonly]")
        .mask("99/99/9999", { placeholder: "_" })
        .datepicker({
            buttonImage: "./static/images/calendar.png",
            buttonImageOnly: true,
            showOn: "button",
            showButtonPanel: true,
            changeMonth: true,
            changeYear: true
        });

    // apply mask to time input and restrict character input to numbers
    $("input.masked-input-time")
        .not("[readonly=readonly]")
        .mask("99:99").keypress(function (e) {
            RestrictToIntegers(e);
        });

    // restrict character input to numbers
    $("input[data-integers-only=1]").keypress(function (e) {
        RestrictToIntegers(e);
    });

    // shows an overlay when the button is clicked
    $("input[data-show-overlay=1]:not('[disabled]'),button[data-show-overlay=1]:not('[disabled]')").on("click", function () {
        setTimeout(BlockUI, 100);
    });
    $("select[data-show-overlay=1]:not('[disabled]')").on("change", function () {
        setTimeout(BlockUI, 100);
    });

    // download links
    $("body").on("click", "a[data-download-links=1][target!=_blank]", function (e) {
        e.preventDefault();

        var target = $(this);

        //target.addClass("download-progress");
        BlockUI("Downloading...", 60);

        var downloadUrl = target.attr("href") + "&attachment=true";

        $.fileDownload(downloadUrl)
            .done(function () {
                //target.removeClass("download-progress");
                unblockUI();
            })
            .fail(function (response, url) {
                //target.removeClass("download-progress");
                unblockUI();

                var message = "Error occurred while downloading the file.";

                if (response) {
                    var obj = null;

                    try {
                        obj = JSON.parse(response);
                    } catch (e) {
                        // ignore
                    }

                    if (obj != null) {
                        message = "<ul>";

                        if ($.isArray(obj)) {
                            for (var i = 0; i < obj.length; i++) {
                                message += "<li style='list-style-type: disc; padding: 3px 5px; margin-left: 20px;'>" + obj[i] + "</li>";
                            }
                        }
                        else {
                            message += "<li>" + obj + "</li>";
                        }

                        message += "</ul>";
                    }
                }

                showDialog(message, "Download Failed", { messageIcon: "error" });
            })
            .always(function () {
                unblockUI();
            });
    });

    showMasterDialog();
}

function showMessage(message, style) {
    var div = $("<div></div>").css("padding", "10px 0px");

    div.addClass("notif-summary");
    div.addClass((style || "error") + "-summary");
    div.addClass("ui-corner-all");

    var divInner = $("<div></div>");
    div.append(divInner);

    var ul = $("<ul style='margin-left: 20px;'></ul>").appendTo(divInner);

    if ($.isArray(message)) {
        $.each(message, function (item) {
            ul.append("<li>" + message[i] + "</li>");
        });
    }
    else {
        ul.append("<li>" + message + "</li>");
    }

    $("#notificationWrapper").empty().append(div);
}

function showDialog(message, title, opts) {
    var dialogContent = null;

    if (jQuery.type(message) === "string") {
        if (message.trim().substring(0, 4) != "<ul>") {
            message = "<ul><li>" + message + "</li></ul>";
        }

        dialogContent = $("#masterDialog");
        dialogContent.empty().append("<div class='notif-summary error-dialog'>" + message + "</div>");
    }
    else {
        dialogContent = message;
    }

    if (title == null || typeof (title) == "undefined") {
        title = "Message";
    }

    var messageIcon = "info";
    var dialogWidth = 600;
    var dialogHeight = 200;
    var dialogResizable = true;
    var autoCloseTimeout = 0;
    var confirmFlag = false;

    var dialogPosition = { my: "center top", at: "center top+40%", of: $("div.page") };

    var dialogButtons = [{
        text: "OK",
        click: function () {
            $(this).dialog("close");

            // callback
            if (opts && opts.onOk && $.isFunction(opts.onOk)) {
                opts.onOk();
            }
        }
    }];

    if (opts) {
        if (!(opts.messageIcon == null || typeof (opts.messageIcon) == "undefined")) {
            messageIcon = opts.messageIcon;
        }

        if (!(opts.width == null || typeof (opts.width) == "undefined")) {
            dialogWidth = parseInt(opts.width, 10);
        }

        if (!(opts.height == null || typeof (opts.height) == "undefined")) {
            dialogHeight = parseInt(opts.height, 10);
        }

        if (opts.resizable == true || opts.resizable == false) {
            dialogResizable = opts.resizable;
        }

        if (!(opts.autoCloseTimeout == null || typeof (opts.autoCloseTimeout) == "undefined")) {
            autoCloseTimeout = parseInt(opts.autoCloseTimeout, 10);
        }

        if (opts.confirm === true) {
            confirmFlag = true;
        }
    }

    if (confirmFlag === true) {
        dialogButtons = [{
            text: "OK",
            click: function () {
                $(this).dialog("close");

                // raise callback
                if (opts && opts.onOk && $.isFunction(opts.onOk)) {
                    opts.onOk();
                }
            }
        },
        {
            text: "Cancel",
            click: function () {
                $(this).dialog("close");

                // raise callback
                if (opts && opts.onCancel && $.isFunction(opts.onCancel)) {
                    opts.onCancel();
                }
            }
        }];

    } else if (autoCloseTimeout > 0) {
        dialogButtons = [];
    }

    var masterDialog = dialogContent.dialog({
        width: dialogWidth,
        height: dialogHeight,
        position: dialogPosition,
        modal: true,
        draggable: true,
        closeOnEscape: true,
        title: title,
        resizable: dialogResizable,
        stack: true,
        autoOpen: false,
        dialogClass: "master-dialog " + messageIcon + "-dialog",
        buttons: dialogButtons,
        close: function (event, ui) {
            $(this).dialog("destroy");

            // raise callback
            if (opts && opts.onClose && $.isFunction(opts.onClose)) {
                opts.onClose();
            }
        },
        open: function (event, ui) {
            // start timer if autoClose option is set
            if (autoCloseTimeout > 0) {
                var timeoutHandle = setTimeout(function () {
                    // error occurs if dialog was closed by clickig the [X]
                    try {
                        if (masterDialog.dialog("isOpen") === true) {
                            masterDialog.dialog("close");
                        }
                    } catch (e) {

                    }
                    clearTimeout(timeoutHandle);

                }, autoCloseTimeout);
            }
        }
    });

    // open the dialog
    masterDialog.dialog("open");

    // return a reference to dialog to the caller
    return masterDialog;
}

function showMasterDialog(content, title, cssClass) {
    var dialogWidth = 600;
    var dialogHeight = 200;
    var dialogResizable = true;
    var dialogTitle = null, dialogClass = null;

    var divNotif = $("div[role=notif-cont]");

    if (divNotif.length == 0 || divNotif.attr("show-dialog") != "1") {
        return;
    }

    divNotif.find("li:gt(0)").css("margin-top", "10px");

    var width = divNotif.attr("dialog-width");
    if (width) {
        dialogWidth = width;
    }
    var height = divNotif.attr("dialog-height");
    if (height) {
        dialogHeight = height;
    }
    var resizable = divNotif.attr("dialog-resizable");
    if (resizable) {
        dialogResizable = resizable;
    }

    dialogTitle = divNotif.attr("dialog-title");
    dialogClass = "master-dialog " + divNotif.attr("dialog-class");

    if (dialogTitle == null || typeof (dialogTitle) == "undefined") {
        dialogTitle = "Message";
    }

    divNotif.removeClass("hidden").dialog({
        width: dialogWidth,
        height: dialogHeight,
        position: { my: "center top", at: "center top+100", of: $("div.page") },
        modal: true,
        draggable: true,
        closeOnEscape: true,
        title: dialogTitle,
        resizable: dialogResizable,
        stack: true,
        dialogClass: dialogClass,
        buttons: {
            "OK": function () {
                $(this).dialog("close");
                $(document).trigger("master-dialog-ok", event);
            }
        },
        close: function (event, ui) {
            $(this).dialog("destroy");
        }
    });
}
