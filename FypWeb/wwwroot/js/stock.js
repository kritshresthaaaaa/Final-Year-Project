var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            url: '/admin/inventory/getall',
            dataSrc: '' // Assuming the API response directly contains the data array
        },
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "productName", "width": "20%" },
            { "data": "categoryName", "width": "15%" },
            { "data": "brandName", "width": "10%" },
            { "data": "size", "width": "10%" },
            { "data": "price", "width": "15%" },
            { "data": "stock", "width": "10%" },
            {
                "data": "stockStatus", // Use the stockStatus field from your API
                "title": "Status",
                "width": "15%",
                "render": function (data, type, row) {
                    // Customize display based on stockStatus value
                    var statusHtml = '<span style="padding: 5px 10px; border-radius: 5px; color: white;">';
                    if (data === "Low") {
                        statusHtml += '<span style="background-color: red;">Low Stock</span>';
                    } else {
                        statusHtml += '<span style="background-color: green;">In Stock</span>';
                    }
                    statusHtml += '</span>';
                    return statusHtml;
                }
            }
        ],
        "order": [[1, "asc"]] // Optionally, adjust the initial sorting
    });
}
