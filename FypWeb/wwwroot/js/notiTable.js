function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": '/Employee/Notification/GetAllNotifications',
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "createdDate",
                "width": "20%",
                "render": function (data, type, row) {
                    // Assuming `data` is a date string, convert it to a Date object first
                    var date = new Date(data);
                    // Then format it using `toLocaleString`
                    return date.toLocaleString();
                }
            },
            { "data": "fromRoomName", "width": "10%" },
            { "data": "notiHeader", "width": "15%" },
            { "data": "notiBody", "width": "25%" },
            { "data": "isReadSt", "width": "10%" },
           
            {
                "data": "notiId",
                "render": function (data, type, row) {
                    // Check if the notification is read or not
                    var buttonClass = "bg-green-700 hover:bg-green-800 dark:bg-green-600 dark:hover:bg-green-700"; // Default class for read notifications
                    var buttonText = "Read"; // Default text for read notifications
                    if (row.isReadSt === "NO") { // Assuming isReadSt returns "Not Read" for unread notifications
                        buttonClass = "bg-red-700 hover:bg-red-800 dark:bg-red-600 dark:hover:bg-red-700"; // Change class for unread notifications
                        buttonText = "Mark as Read"; // Change text for unread notifications
                    }
                    return `<button class="mark-read w-[200px] focus:outline-none text-white ${buttonClass} focus:ring-4 focus:ring-green-300 font-medium rounded-lg text-sm px-5 py-2.5 me-2 mb-2 dark:focus:ring-green-800" data-id="${data}">${buttonText}</button>`;
                },
                "width": "20%"
            }

        ],
        "columnDefs": [
            { "orderable": false, "targets": 5 } // Disables ordering on the 'Mark as Read' button column
        ]
    });
}

$(document).ready(function () {
    loadDataTable();   
    $('#tblData tbody').on('click', '.mark-read', function () {
        var id = $(this).data('id'); 

        $.ajax({
            url: '/Employee/Notification/SetNotificationRead',
            method: 'POST',
            data: { id: id }, 
            success: function (response) {
                if (response.message == "Already") {
                    dataTable.ajax.reload(null, false);
                    toastr.success("Notification already read.");

                }
                else if (response.success) {
                    dataTable.ajax.reload(null, false);
                    toastr.success("Notification read successfully.");
                }

                else {
                    dataTable.ajax.reload(null, false);
                    toastr.error("Error marking as read.");
                }
            },
            error: function () {
                toastr.error("Error marking as read.");
            }
        });
    });
});
