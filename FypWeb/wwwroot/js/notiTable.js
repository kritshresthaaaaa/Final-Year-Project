var dataTable;
$(document).ready(function () {
    loadDataTable();
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/FittingRoomEmployee/Home/GetAllNotifications' }, // Make sure this URL matches the updated action method that includes product counts.
        "columns": [
            { "data": "fromRoomName", "width": "15%" },
            { "data": "notiHeader", "width": "15%" },
            { "data": "notiBody", "width": "25%" },
            { "data": "isReadSt", "width": "25%" },
            { "data": "createdDate", "width": "25%" },      
        ]
    });
}


