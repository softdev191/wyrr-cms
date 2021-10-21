$(function () {

    LoadData();
});


function LoadData() {
    var flagbVisible = true;

    $('#grid').dataTable({

        "bDestroy": true,
        "bProcessing": true,
        "sAjaxSource": "GetTransactionDetails",
        "aoColumns": [
               
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
                { "mDataProp": "SenderName" },                
                            
                { "mDataProp": "ReceiverName" },
                { "mDataProp": "Amount" },
                { "mDataProp": "SenderAccountNumber" },
                { "mDataProp": "ReceiverAccountNumber" },
                {
                    "mDataProp": "TransactionDate"
                   
                } ,  
        ],
        "fnRowCallback": function (nRow, aData, iDisplayIndex, iDisplayIndexFull) {
            if (!aData.IsTxnVerified) {
               $('td', nRow).css('background-color', '#ffeaea');
            }
            else
            {
                $('td', nRow).css('background-color', '#ddffdd');
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

//To Change date format into human readable format
function formattedDate(javascriptDate) {
    if (javascriptDate != null) {
        var pattern = /Date\(([^)]+)\)/;
        var results = pattern.exec(javascriptDate);
        var dt = new Date(parseFloat(results[1]));

        var fullYear = dt.getFullYear().toString();
        var month = (dt.getMonth() + 1).toString();
        var day = dt.getDate().toString();

        if (month.toString().length == 1)
            month = "0" + month;

        if (day.toString().length == 1)
            day = "0" + day;

        //javascriptDate = dt.getDate() + "/" + (dt.getMonth() + 1) + "/" + dt.getFullYear();
        javascriptDate = day + "-" + month + "-" + fullYear;

    }
    else {
        javascriptDate = "";
    }
    return "<div class='date'>" + javascriptDate + "<div>"
}


function ClosePopup() {
    $('[data-popup="popup"]').fadeOut(350);
    return false;
}

