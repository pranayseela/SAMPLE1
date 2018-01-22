/// <reference path="~/static/scripts/jquery-1.8.3-vsdoc.js" />

$(document).ready(docReady);

var html = ""
+ "<div class='blockUIMessage'>"
+ "<div class='message'>Loading...</div>"
+ "<div class='animation'>&nbsp;</div>"
+ "</div>";

jQuery.fn.center = function () {
    this.css("position", "absolute");
    this.css("top", Math.max(0, (($(window).height() - $(this).outerHeight()) / 2) +
                                                $(window).scrollTop()) + "px");
    this.css("left", Math.max(0, (($(window).width() - $(this).outerWidth()) / 2) +
                                                $(window).scrollLeft()) + "px");
    return this;
}


function docReady() {
    var $form = $("#formMain");
    var $popup = $("#popup");
    var $html = $(html);

    $html.css({
        cursor: 'default',
        border: 'solid 1px #507CD1',
        padding: '15px',
        position: 'fixed',
        width: '600px',
        background: 'white'
    });

    $html.appendTo("body");

    var ajaxSubmit = (function () {
        var $form = $(this);

        var options = {
            url: $form.attr("action"),
            type: $form.attr("method"),
            data: $form.serialize(),
            beforeSend: function () {
                $('.blockUIMessage').center().show();
            }
        };

        $.ajax(options).done(function (data) {
            $($form.attr("data-wkc-target")).replaceWith(data);

            // hide the progress message
            $('.blockUIMessage').hide();

            // re-apply the tooltip style 
            $("#EmployeeDetails table.data a[data-status-alert=1]").tooltip({
                position: {
                    my: "left bottom-10",
                    at: "left top"
                },
                tooltipClass: "warning-summary"
            });
        });

        return false;
    });

    $form.submit(ajaxSubmit);

    $("#clearFilter").click(function () {
        $form[0].reset();

        ajaxSubmit();
    });

    $("#ddlPageNumber").change(function (e) {
        $form.submit();
    });

    $("#btnPrevious:not('[disabled]')").click(function (e) {
        var el = $("#ddlPageNumber")
        el.val(parseInt(el.val(), 10) - 1);

        $form.submit();
    });

    $("#btnNext:not('[disabled]')").click(function (e) {
        var el = $("#ddlPageNumber")
        el.val(parseInt(el.val(), 10) + 1);

        $form.submit();
    });

    // setup tooltip
    $("table.data a[data-status-alert=1]").tooltip({
        position: {
            my: "left bottom-10",
            at: "left top"
        },
        tooltipClass: "warning-summary"
    });

    //// show status alert dialog
    //$("table.data a[data-status-alert=1]").click(function () {
    //    showDialog($("<div style='padding: 20px 10px' class='ui-dialog-content'>" + $(this).attr("data-status-alert-message") + "</div>"), "Employee Qualification Alert", { messageIcon: "warning" });
    //});


}