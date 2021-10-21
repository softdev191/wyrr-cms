$(function () {

    LoadData();
});


function LoadData() {
    var flagbVisible = true;

    $('#grid').dataTable({

        "bDestroy": true,
        "bProcessing": false,
        "sAjaxSource": "GetUsers",
        "aoColumns": [
                { "mDataProp": "UserName" },
                { "mDataProp": "EmailAddress" },
                { "mDataProp": "PhoneNumber" },
                { "mDataProp": "PublicKey" },
                //{ "mDataProp": "UniqueUserId" },
               // { "mDataProp": "DefaultCurrency" },
                {
                    "mDataProp": "IsSocialLogin", "mRender": function (mDataProp, type, full) {
                        if (full.IsSocialLogin) {
                            return "Yes"
                        }
                        else {
                            return "No"
                        }

                    }
                },
                { "mDataProp": "LastLoginDateTime" },
                {
                    "mDataProp": "IsActive", "mRender": function (mDataProp, type, full) {
                        if (full.IsActive) {
                            return "Active"
                        }
                        else {
                            return "InActive"
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

                linkButtons += '<a class="btn btn-info btn-sm" onclick="return OpenPopup(this);"  href="../Admin/ViewUser/' + full['UserID'] + '">View</a> ';

                linkButtons += '<a class="btn btn-info btn-sm" onclick="return OpenPopup(this);"  href="../Admin/ManageUser/' + full['UserID'] + '">Edit</a> ';
                

                if (full.IsDeleted) {
                    linkButtons += '<a class="btn btn-infodelete btn-sm" onclick="return Active(this);" style="width:79px" href="../Admin/Active/' + full['UserID'] + '">Activate</a>';
                }
                else {
                    linkButtons += '<a class="btn btn-info btn-sm" onclick="return DeActive(this);" style="width:79px" href="../Admin/DeActive/' + full['UserID'] + '">Deactivate</a>';
                }
                return linkButtons;
            }, "bVisible": flagbVisible
        }
        ],
        "fnRowCallback": function (nRow, aData, iDisplayIndex, iDisplayIndexFull) {
            if (aData.IsDeleted) {
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


function DeActive(obj) {

    var url = obj.getAttribute("href");


    bootbox.confirm({
        size: "small",
        message: "Are you sure,you want to deactivate this user?",
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

function Active(obj) {

    var url = obj.getAttribute("href");

    

    bootbox.confirm({
        size: "small",
        message: "Are you sure,you want to activate this user?",
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