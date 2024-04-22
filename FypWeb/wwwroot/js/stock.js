var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            url: '/admin/inventory/getall',
            // Assuming the API response directly contains the data array
        },
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "productName", "width": "20%" },
            { "data": "categoryName", "width": "15%" },
            { "data": "brandName", "width": "10%" },
            { "data": "size", "width": "10%" },
            {
                "data": "price",
                "width": "15%",
                "render": function (data, type, row) {
                    return 'Rs. ' + data; // Concatenate "Rs. " with the price data
                }
            },
            { "data": "stock", "width": "10%" },
            {
                "data": "stockStatus", // Use the stockStatus field from your API
                "title": "Status",
                "width": "15%",
                "render": function (data, type, row) {
                    // Customize display based on stockStatus value
                    var statusHtml = '<span class="py-1 px-2 rounded-md text-white">';
                    if (data === "Low") {
                        statusHtml += '<span class="bg-red-500 rounded-md p-1">Low Stock</span>';
                    } else {
                        statusHtml += '<span class="bg-green-500 p-1 rounded-md">In Stock</span>';
                    }

                    statusHtml += '</span>';
                    return statusHtml;
                }
            }
        ],
        "order": [[1, "asc"]] // Optionally, adjust the initial sorting
    });
}
