function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": '/FittingRoomEmployee/Home/GetAllNotifications',
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
                    return `<button class="mark-read w-[200px] focus:outline-none text-white bg-green-700 hover:bg-green-800 focus:ring-4 focus:ring-green-300 font-medium rounded-lg text-sm px-5 py-2.5 me-2 mb-2 dark:bg-green-600 dark:hover:bg-green-700 dark:focus:ring-green-800" data-id="${data}">Mark as Read</button>`;
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

    // Correctly attach the event handler using delegation
    $('#tblData tbody').on('click', '.mark-read', function () {
        var id = $(this).data('id'); // Get the ID of the notification

        // AJAX call to server to update the isRead status
        $.ajax({
            url: '/FittingRoomEmployee/Home/SetNotificationRead',
            method: 'POST',
            data: { id: id }, // Ensure your server-side action is expecting an `id` parameter
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
