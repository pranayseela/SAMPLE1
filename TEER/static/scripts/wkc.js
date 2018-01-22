$.fn.center = function (parent) {
    if (parent) {
        parent = this.parent();
    } else {
        parent = window;
    }
    this.css({
        "position": "absolute",
        "top": ((($(parent).height() - this.outerHeight()) / 2) + $(parent).scrollTop() + "px"),
        "left": ((($(parent).width() - this.outerWidth()) / 2) + $(parent).scrollLeft() + "px")
    });
    return this;
}

$.fn.clearForm = function () {
    return this.each(function () {
        var type = this.type, tag = this.tagName.toLowerCase();
        if (tag == 'form')
            return $(':input', this).clearForm();
        if (type == 'text' || type == 'password' || tag == 'textarea' || type=='file')
            this.value = '';
        else if (type == 'checkbox' || type == 'radio')
            this.checked = false;
        else if (tag == 'select')
            this.selectedIndex = 0;
    });
};

$.fn.showProgressBar = function () {
    $("div.blockUIMessage").remove();

    var html = ""
        + "<div class='blockUIMessage'>"        
        + "<div class='animation'>&nbsp;</div>"
        + "<div class='message'>Loading, please wait...</div>"
        + "</div>";

    $(html).css({
        cursor: 'default',
        border: 'solid 1px #fff',
        padding: '15px',
        position: 'fixed'        
    }).appendTo(this);

    //$(html).appendTo(this);
    $(".blockUIMessage").center(false).show();
};

$.fn.hideProgressBar = function () {
    $(".blockUIMessage").hide();
};

$.fn.showSpinner = function () {
    $("div.blockUIMessage").remove();

    var html = ""
        + "<div class='loader'>"
        + "<div class='message'>Loading...</div>"      
        + "</div>";

    $(html).appendTo(this);

    //$(html).appendTo(this);
    $(".blockUIMessage").center(true).show();


};

$.fn.hideSpinner = function () {
    $(".blockUIMessage").hide();
};

$.fn.loadWithoutCache = function () {
    var elem = $(this);
    var func = arguments[1];
    $.ajax({
        url: arguments[0],
        cache: false,
        dataType: "html",
        success: function (data, textStatus, XMLHttpRequest) {
            elem.html(data);
            if (func != undefined) {
                func(elem, data, textStatus, XMLHttpRequest);
            }
        }
    });
    return elem;
}