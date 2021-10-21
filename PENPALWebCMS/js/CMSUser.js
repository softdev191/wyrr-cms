$(function () {

    LoadData();
});

function LoadData() {
    var flagbVisible = true;

    $('#grid').dataTable({

        "bDestroy": true,
        "bProcessing": false,
        "sAjaxSource": "GetCMSUsers",
        "aoColumns": [
                { "mDataProp": "UserName" },
                { "mDataProp": "EmailAddress" },
                { "mDataProp": "PhoneNumber" },
                {
                    "mDataProp": "IsDeleted", "mRender": function (mDataProp, type, full) {
                        if (full.IsDeleted) {
                            return "InActive"
                        }
                        else {
                            return "Active"
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

                linkButtons += '<a class="btn btn-info btn-sm" onclick="return OpenPopup(this);"  href="../CMSUser/ViewCMSUser/' + full['UserID'] + '">View</a> ';

                linkButtons += '<a class="btn btn-info btn-sm" onclick="return OpenPopup(this);"  href="../CMSUser/ManageCMSUser/' + full['UserID'] + '">Edit</a> ';


                if (full.IsDeleted) {
                    linkButtons += '<a class="btn btn-infodelete btn-sm" onclick="return Active(this);" style="width:79px" href="../CMSUser/Active/' + full['UserID'] + '">Activate</a>';
                }
                else {
                    linkButtons += '<a class="btn btn-info btn-sm" onclick="return DeActive(this);" style="width:79px" href="../CMSUser/DeActive/' + full['UserID'] + '">Deactivate</a>';
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

function UpdateResetPasswordSuccessful(data) {
    //    if (data.indexOf("field-validation-error") > -1) return;

    if (data.success) {
        $('[data-popup="popup"]').fadeOut(0);
       
        toastr.success(data.message);
        window.location.href = "../CMSUser/Index";
    }
}


function ClosePopup() {
    window.location.href = "../CMSUser/Index";
}

function ValidateData() {
   
    var inputOldPasswordVal = $('#Password').val();
    var inputNewPasswordVal = $('#NewPassword').val();
    var inputConfirmPasswordVal = $('#ConfirmPassword').val();

    if (inputOldPasswordVal != null && inputNewPasswordVal != null) {
        if (inputNewPasswordVal != inputConfirmPasswordVal) {

            toastr.error('New Password and confirm Password must be same');
            return false;
        }
        else if (inputOldPasswordVal == inputNewPasswordVal) {

            toastr.error('New Password should not be same as Old Password');
            return false;
        }
    }
}
