$(function () {
    LoadView();
});


function LoadView() {
    $.get('GetSearchCriteria', function (data) {
        if (data) {
            $('#partialOrderView').html(data);
            $(function () {
                CallAllDatePickers();
            });
        }
        else {
            toastr.error('Error occurred');
        }

    });
}


function CallAllDatePickers() {
    $('#FromDate').datepicker({
        format: 'dd-mm-yyyy'
    }).on('changeDate', function (ev) {
        $('#FromDate').datepicker('hide');
    });
    $('#FromDate').click(function () {
        $(".dropdown-menu").hide();
        $('#FromDate').datepicker('show');
    });


    $('#ToDate').datepicker({
        format: 'dd-mm-yyyy'
    }).on('changeDate', function (ev) {
        $('#ToDate').datepicker('hide');
    });
    $('#ToDate').click(function () {
        $(".dropdown-menu").hide();
        $('#ToDate').datepicker('show');
    });



}


function LoadSearchResult(data) {
    searchData = data.aaData;
    $('#userSearchResult').show();

    table = $('#searchResult').dataTable({
        "bDestroy": true,
        "bProcessing": true,
        "oLanguage": {
            "sEmptyTable": "No user records found for this query."
        },
        "aaData": data.aaData,
        "aoColumns": [
           //{
           //    "mDataProp": "ID",
           //    "bSearchable": false,
           //    "bSortable": false,
           //    "mRender": function (data, type, full) {
           //        return "<input type='checkbox' name='id[]' value=" + full['ID'] + ">";
           //    },
           //},
           {
               "mDataProp": "UserName"
           },
                   {
                       "mDataProp": "EmailAddress"
                   },
                   //{
                   //    "mDataProp": "AccountNumber"
                   //},
                   {
                       "mDataProp": "PhoneNumber"
                   },

                   {
                       "mDataProp": "UniqueUserId"


                   },
                    {
                        "mDataProp": "DefaultCurrency"


                    },
                   //{
                   //    "mDataProp": "Date"
                   //},
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



        ],
        "aaSorting": [[1, 'desc']]

    });



}

function ValidateData() {
    var inputVal = $('#PhoneNumber').val();
    if (inputVal != "") {
        var pattern = /^\d{10}$/;
        if (!pattern.test(inputVal)) {
            toastr.error('Please enter valid phone number');
            return false;
        }
    }

    var inputdateVal = $('#FromDate').val();

    if (inputdateVal != "")
    {
        var dateReg = /^\d{2}([./-])\d{2}\1\d{4}$/;
        if (!dateReg.test(inputdateVal)) {
            toastr.error('Please enter valid from date');
            return false;
        }

    }
   
    var inputtodateVal = $('#ToDate').val();

    if (inputtodateVal != "") {
        var dateReg = /^\d{2}([./-])\d{2}\1\d{4}$/;
        if (!dateReg.test(inputtodateVal)) {
            toastr.error('Please enter valid to date');
            return false;
        }

    }

    var inputNameVal = $('#CustomerName').val();
    if (inputNameVal != "") {
        var pattern = /^[a-zA-Z ]*$/;;
        if (!pattern.test(inputNameVal)) {
            toastr.error('Please enter valid customer name');
            return false;
        }
    }
    
    var startDate = Date.parse($('#FromDate').val()),
       endDate = Date.parse($('#ToDate').val())

    if (startDate > endDate) {
        toastr.error('End Date should greater than Start Date');
        return false;
    }


}