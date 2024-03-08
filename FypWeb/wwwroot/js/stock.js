var dataTable;
$(document).ready(function () {
    loadDataTable();
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/inventory/getall' }, // Make sure this URL matches the updated action method that includes product counts.
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "productName", "width": "20%" },
            { "data": "categoryName", "width": "15%" },
            { "data": "brandName", "width": "10%" },
            { "data": "size", "width": "10%" },
            { "data": "price", "width": "15%" },
            { "data": "stock", "width": "10%" },
            {
                "data": "stock", // Use the stock data for determining the status.
                "title": "Status",
                "width": "15%", // Adjust the width as needed
                "render": function (data, type, row) {
                    // Check the stock value and return a button with the corresponding style
                    return parseInt(data, 10) <= 1
                        ? '<button type="button" style="color: white; background-color: red; border-radius: 5px; padding: 5px 10px;">Low Stock</button>'
                        : '<button type="button" style="color: white; background-color: green; border-radius: 5px; padding: 5px 10px;">In Stock</button>';
                }
            }

        ]
    });
}


