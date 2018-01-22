if (typeof jQuery === 'undefined') {
    throw new Error('jQuery is required')
}

$(document).ready(function () {
    $("div.menu-container ul").addClass("menu");
    $("div.menu-container").css('display', 'block');

    $("ul.menu li ul li:has(ul)").find("a:first").append(" &raquo; ");
});
