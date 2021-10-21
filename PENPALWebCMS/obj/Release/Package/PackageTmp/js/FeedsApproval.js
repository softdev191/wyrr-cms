$(function () {

    LoadData();
});


function LoadData() {
    var flagbVisible = true;

    $('#grid').dataTable({

        "bDestroy": true,
        "bProcessing": false,
        "sAjaxSource": "GetFeeds",
        "aoColumns": [
                { "mDataProp": "User_feedID" },
                { "mDataProp": "UserID" },
                { "mDataProp": "UserName" },
                { "mDataProp": "Title" },
                { "mDataProp": "Description" },
                { "mDataProp": "created_at" },
                {
                    "mDataProp": "isApproved", "mRender": function (mDataProp, type, full) {
                        if (full.isApproved) {
                            return "Approved"
                        }
                        else {
                            return "Not Approved"
                        }

                    }
                },
        {
            "mData": null,
            "bSearchable": false,
            "bSortable": false,
            "sClass": "text-center",
            "mRender": function (data, type, full) {

                var linkButtons = '';
                if (!full.isApproved) {
                    linkButtons += '<a class="btn btn-infodelete btn-sm" onclick="return Approve(this);" style="width:79px" href="../FeedsApproval/ApproveFeed/' + full['User_feedID'] + '">Approve</a>';
                }
                else {
                    linkButtons += '<a class="btn btn-info btn-sm" onclick="return Disapprove(this);" style="width:79px" href="../FeedsApproval/DisApproveFeed/' + full['User_feedID'] + '">Disapprove</a>';
                }
                return linkButtons;
            }, "bVisible": flagbVisible
        }
        ]
        ,
        "fnRowCallback": function (nRow, aData, iDisplayIndex, iDisplayIndexFull) {
            if (aData.isApproved) {
                $(nRow).css('color', '#adadad');
            }
        }
    });
}

function OpenPopup(obj) {

    var url = obj.getAttribute("href");

    $.get(url, function (data) {
        if (data) {
            $('#partialContent').html(data);
            $('[data-popup="popup"]').fadeIn(350);
        }
        else {
            toastr.error('Error occurred');
        }

    });
    return false;
}


function ClosePopup() {
    $('[data-popup="popup"]').fadeOut(350);
    return false;
}

function UpdateSuccessful(data) {
    //    if (data.indexOf("field-validation-error") > -1) return;

    if (data.success) {
        $('[data-popup="popup"]').fadeOut(0);
        LoadData();
        toastr.success('Saved Successfully');

    }
}


function Disapprove(obj) {

    var url = obj.getAttribute("href");


    bootbox.confirm({
        size: "small",
        message: "Are you sure,you want to disapprove this feed?",
        callback: function (result) {
            if (result) {
                $.ajax({
                    url: url,
                    type: "post",
                    success: function (data) {
                        if (data.success) {
                            toastr.success(data.message);
                            LoadData();
                        }
                        else {
                            toastr.error(data.message);
                        }
                    }
                });
            }
        }
    });

            
    return false;
}

function Approve(obj) {

    var url = obj.getAttribute("href");

    

    bootbox.confirm({
        size: "small",
        message: "Are you sure,you want to approve this feed?",
        callback: function (result) {
            if (result) {
                $.ajax({
                    url: url,
                    type: "post",
                    success: function (data) {
                        if (data.success) {
                            toastr.success(data.message);
                            LoadData();
                        }
                        else {
                            toastr.error(data.message);
                        }
                    }
                });
            }
        }
    });


    
    return false;
}