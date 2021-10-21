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
    $('#TransactionFromDate').datepicker({
        format: 'dd-mm-yyyy'
    }).on('changeDate', function (ev) {
        $('#TransactionFromDate').datepicker('hide');
    });
    $('#TransactionFromDate').click(function () {
        "@ViewBag.Message"
        $(".dropdown-menu").hide();
        $('#TransactionFromDate').datepicker('show');
    });


    $('#TransactionToDate').datepicker({
        format: 'dd-mm-yyyy'
    }).on('changeDate', function (ev) {
        $('#TransactionToDate').datepicker('hide');
    });
    $('#TransactionToDate').click(function () {
        $(".dropdown-menu").hide();
        $('#TransactionToDate').datepicker('show');
    });



}

function LoadSearchResult(data) {
    searchData = data.aaData;
    $('#transactionSearchResult').show();

    table = $('#searchResult').dataTable({
        "bDestroy": true,
        "bProcessing": true,
        "oLanguage": {
            "sEmptyTable": "No transaction records found for this query."
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
               "mDataProp": "IsTxnVerified", "mRender": function (mDataProp, type, full) {
                   if (full.IsTxnVerified) {
                       return "Success"
                   }
                   else {
                       return "Failed"
                   }

               }
           },

           {
                           "mDataProp": "SenderName"
                       },
           {
                       "mDataProp": "ReceiverName"
                   },
           {
                       "mDataProp": "SenderAccountNumber"
                   },
           {
                       "mDataProp": "RecieverAccountNumber"
                   },
           {
                       "mDataProp": "TotalAmount"


                   },
           {
                       "mDataProp": "Date"
                   },
        ],
        "fnRowCallback": function (nRow, aData, iDisplayIndex, iDisplayIndexFull) {
            if (!aData.IsTxnVerified) {
                $('td', nRow).css('background-color', '#ffeaea');
            }
            else
            {
                $('td', nRow).css('background-color', '#ddffdd');
            }
        },

        "aaSorting": [[1, 'desc']]

    });



}


function ValidateData() {
   
    var inputdateVal = $('#TransactionFromDate').val();
   
    if (inputdateVal != "") {
        var dateReg = /^\d{2}([./-])\d{2}\1\d{4}$/;
        if (!dateReg.test(inputdateVal)) {
            toastr.error('Please enter valid from date');
            return false;
        }

    }

    var inputtodateVal = $('#TransactionToDate').val();

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


    var startDate = Date.parse($('#TransactionFromDate').val()),
       endDate = Date.parse($('#TransactionToDate').val())

    if (startDate > endDate) {
        toastr.error('End Date should greater than Start Date');
        return false;
    }


}


