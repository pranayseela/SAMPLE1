$(document).ready(docReady);

var vm = null;

function addRemove(data, event) {
    var $this = $(event.target);

    var tr = $this.closest("tr.data-row");
    tr.addClass("selected-row");

    var deleteFlag = false;

    $("<div style='padding: 20px 10px'>Do you want to remove this qualification?</div>")
        .dialog({
            width: 500,
            modal: true,
            draggable: true,
            closeOnEscape: true,
            title: "Confirm",
            resizable: false,
            stack: true,
            dialogClass: "master-dialog warning-dialog",
            buttons: {
                "Delete": function () {
                    deleteFlag = true;
                    $(this).dialog("close");
                },
                "Cancel": function () {
                    tr.removeClass("selected-row");
                    $(this).dialog("close");
                }
            },
            close: function () {
                if (deleteFlag === true) {
                    // no animation needed because user is being asked 'Are you sure?'
                    //$this.closest("div.row").animate({
                    //    height: "0px"
                    //}, 500, "easeOutExpo", function () {
                    //    vm.removeSelected(data);
                    //});

                    vm.removeSelected(data);
                }

                $("table.data tr").removeClass("selected-row");
                $(this).dialog("destroy");
            }
        });
}

function docReady() {
    var timeoutHandle = 0;
    var searchTerm;

    $popup = $("#popup");
    $posQualifContainer = $("#posQualifContainer");
    $posQualifIds = $("#PositionQualificationIds");
    $posQualifs = $("#PositionQualifications");
    //$txtSearchTerm = $("#txtSearchTerm");

    // set the view model
    vm = new PositionQualifViewModel();
    vm.setSelectedItems(JSON.parse($posQualifs.val()));

    ko.applyBindings(vm, $posQualifContainer[0]);
    ko.applyBindings(vm, $popup[0]);

    //var oldSearchterm = null;
    //$txtSearchTerm.keyup(function (e) {
    //    searchTerm = $txtSearchTerm.val();

    //    if (searchTerm == oldSearchterm) {
    //        return;
    //    }

    //    // clear prior timeout
    //    clearTimeout(timeoutHandle);

    //    timeoutHandle = setTimeout(function () {
    //        // apply filter only if text hasn't changed
    //        if ($txtSearchTerm.val() == searchTerm) {
    //            oldSearchterm = searchTerm;
    //            vm.applyFilter(searchTerm);
    //        }
    //    }, 200);
    //});

    $popup.on("keydown", "input[data-search-field]", function (e) {
        if (e.keyCode == KEYCODE.ENTER) {
            vm.applyFilter();
        }
    });

    $("#btnUpdate").click(function (e) {
        // before submitting the form store the selected ids as a comma-separated list
        $posQualifIds.val(_.map(vm.selectedItems(), function (item) {
            return item.id();
        }).join(","));
    });

    $("#btnAddPosQualif").click(function () {
        $popup.dialog({
            width: 820,
            position: { my: "center top", at: "center top+150", of: $("div.page") },
            modal: true,
            draggable: true,
            closeOnEscape: true,
            title: "Add / Remove Position Qualifications",
            resizable: false,
            stack: true,
            dialogClass: "info-dialog qualif-dialog",
            buttons: {
                "Done": function () {
                    $(this).dialog("close");
                }
            },
            close: function () {
                $(this).dialog("destroy");
            }
        });
    });
}

var PositionQualifViewModel = function () {
    var self = this;

    // filter options
    self.qualificationId = ko.observable();
    self.qualificationName = ko.observable();
    self.preAward = ko.observable();

    self.items = ko.observableArray();
    self.selectedItems = ko.observableArray();
    self.filtering = ko.observable();

    self.minCharCount = 3;
    self.filterValid = ko.observable(false);

    self.setItems = function (items) {
        self.items.removeAll();

        $.each(items, function (index, item) {
            // get the object if it exists in the selectedItems collection
            var vm = _.find(self.selectedItems(), function (o) {
                return o.id() === item.qualificationId;
            });

            // object does not exist. create a viewmodel instance and add to collection
            if (!vm) {
                vm = new QualifViewModel();

                vm.id(item.qualificationId);
                vm.name(item.qualificationName);
                vm.activeStr(item.activeStr);
                vm.preAwardStr(item.preAwardStr);
                vm.validityDesc(item.validityDesc);
                vm.selected(false);
            }

            self.items.push(vm);
        });
    }

    self.setSelectedItems = function (items) {
        self.selectedItems.removeAll();

        $.each(items, function (index, item) {
            // do not add if the item already exists in the selectedItems collection
            var obj = _.find(self.selectedItems(), function (o) {
                return o.id() === item.qualificationId;
            });
            if (obj) {
                return;
            }

            // create a viewmodel instance and add to collection
            var vm = new QualifViewModel();

            vm.id(item.qualificationId);
            vm.name(item.qualificationName);
            vm.activeStr(item.activeStr);
            vm.preAwardStr(item.preAwardStr);
            vm.validityDesc(item.validityDesc);
            vm.selected(true);

            self.selectedItems.push(vm);
        });
    }

    self.oncheck = function (item) {
        if (item.selected() === true) {
            self.addSelected(item);
        }
        else {
            self.removeSelected(item);
        }

        // allow default action to proceed
        return true;
    }

    // note: this function takes a view model instance
    self.addSelected = function (item) {
        // get the object with the id and add it to the selectedItems collection

        item.selected(true);
        self.selectedItems.push(item);
    }

    // note: this function takes a view model instance
    self.removeSelected = function (item) {
        item.selected(false);

        // get the object with the id and remove from the selectedItems collection
        var obj = _.find(self.selectedItems(), function (o) {
            return o.id() === item.id();
        });

        obj.selected(false);
        self.selectedItems.remove(obj);
    }

    self.applyFilter = function () {
        self.items.removeAll();

        var id = self.qualificationId() || "";
        var name = self.qualificationName() || "";
        var preAward = self.preAward() || "";

        if ((id.trim().length + name.trim().length) < self.minCharCount) {
            self.filterValid(false);
            return;
        }

        self.filterValid(true);
        self.filtering(true);

        // call after a timeout to let the progress image animation show
        setTimeout(function () {

            $.get(buildUrl("/Qualification/FindQualifications"),
                {
                    qualificationId: id,
                    qualificationName: name,
                    preAward: preAward
                })
                .done(function (data) {
                    self.setItems(data.items);
                }).fail(function () {
                    // show error message
                    showDialog(Msg.ERROR_OCCURRED_ON_SERVER, "Error", { messageIcon: "warning" });
                }).always(function () {
                    self.filtering(false);
                });

        }, 500);
    }

    self.clear = function () {
        // clear filter options
        self.qualificationId("");
        self.qualificationName("");
        self.preAward("");

        self.items.removeAll();
        self.filterValid(false);
    }
}

var QualifViewModel = function () {
    var self = this;

    self.id = ko.observable();
    self.name = ko.observable();

    self.activeStr = ko.observable();
    self.preAwardStr = ko.observable();
            
    self.validityDesc = ko.observable();
    self.selected = ko.observable();
}
