$(document).bind("ready.user-roles-groups", function () {
    $popup = $("#popup");
    $allMcGroups = $("#allMcGroups");

    setupTooltip();

    $allMcGroups.on("change", "input:checkbox", function () {
        var $this = $(this);
        var $tr = $this.closest("tr.data-row");

        if ($this.prop("checked") === true) {
            $tr.addClass("selected-row");
        }
        else {
            $tr.removeClass("selected-row");
        }
    });

    // show the 'Select Group' link if checkbox is checked
    $(":checkbox[name=Roles]").on("change", function () {
        var $this = $(this);
        var $tr = $this.closest("tr.data-row");
        var $div = $tr.find("div[data-group-container]");

        if ($this.prop("checked") === true) {
            // uncheck all other checkboxes. they are mutually exclusive
           // $(":checkbox[name=Roles]:not([id=" + $this.attr("id") + "])").prop("checked", false).trigger("change");

            $div.removeClass("hidden");
        }
        else {
            $div.addClass("hidden");
        }
    });

    // remove group from role
    $("div[data-group-container]").on("click", "a[data-remove=1]", function () {
        var $li = $(this).closest("li");
        var $ul = $li.parent();

        // remove <ul> if only one <li> remains
        if ($ul.children().length == 1) {
            $ul.remove();
        }
        else {
            $li.remove();
        }
    });

    // show groups popup
    $("#roles a[data-edit=1]").click(function () {
        var $tr = $(this).closest("tr.data-row");

        var roleId = $tr.attr("data-role-id");
        var roleName = $tr.attr("data-role-name");

        // uncheck all mc groups and only check the ones for this role
        $allMcGroups.find("input:checkbox").prop("checked", false);
        $allMcGroups.find("tr").removeClass("selected-row");
        // select the groups in role
        $tr.find("input:hidden[name^=mc_group_id]").each(function () {
            var $chk = $("input:checkbox[data-group-id=" + $(this).val() + "]");
            // check the checkbox and highlight the row
            $chk.prop("checked", true).closest("tr").addClass("selected-row");
        });

        $popup.dialog({
            width: 600,
            position: { my: "center top", at: "center top+150", of: $("div.page") },
            modal: true,
            draggable: true,
            closeOnEscape: true,
            title: "Select Groups for " + roleName,
            resizable: false,
            stack: true,
            dialogClass: "info-dialog popup-dialog",
            buttons: {
                "Done": function () {
                    $(this).dialog("close");

                    var $div = $tr.find("div[data-group-container]");
                    $div.find("ul.group").remove();

                    // get all checked elements
                    var $chkItems = $allMcGroups.find(":checked");

                    if ($chkItems.length !== 0) {
                        var $ul = $("<ul class='group'></ul>");
                        $div.append($ul);

                        $.each($chkItems, function () {
                            var $this = $(this);
                            var groupId = $this.attr("data-group-id");
                            var groupName = $this.attr("data-group-name");
                            var mcList = $this.attr("data-mc-list");

                            var $li = $("<li></li>");

                            // management center list
                            var $a = $("<a></a>")
                                            .attr("data-mc-list", "1")
                                            .attr("title", mcList)
                                            .addClass("icon-16 icon-info")
                            $li.append($a);

                            // remove icon
                            $li.append(
                                $("<a></a>")
                                    .attr("data-remove", "1")
                                    .addClass("icon-16 icon-delete")
                                );

                            // javascript trick to escape html
                            var htmlEncoded = $("<div><div>").text(groupName).html();

                            $li.append($("<span></span>").html(htmlEncoded));
                            $li.append($("<input type='hidden'></input>").attr("name", "mc_group_id_" + roleName).val(groupId));
                            $ul.append($li);

                            setupTooltip($a);
                        });
                    }
                },
                "Cancel": function () {
                    $(this).dialog("close");
                }
            },
            open: function () {
                // highlight selected row
                $tr.addClass("selected-row");
            },
            close: function () {
                // remove selected row css
                $tr.removeClass("selected-row");

                $(this).dialog("destroy");
            }
        });
    });

    function setupTooltip($a) {
        // setup tooltip
        if (typeof ($a) == "undefined") {
            $a = $("a[data-mc-list=1]");
        }

        $a.tooltip({
            position: {
                my: "left bottom-10",
                at: "left top"
            },
            tooltipClass: "tooltip-summary"
        });
    }
});