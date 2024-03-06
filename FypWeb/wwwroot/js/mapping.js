$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/mapping/getall' },
        "columns": [
            {
                "data": "skuCode.code",
                "width": "10%"
            },
            {
                "data": "products",
                "width": "10%",
                "render": function (data, type, row) {
                    // Simply return the count of products without any button or styling
                    return `${data.length}`;
                }
            },
            {
                "data": "mapping",
                "width": "10%",
                "render": function (data, type, row) {
                    // Ensure proper HTML structure for the anchor tag
                    // If expecting a query parameter
                    return `<a href="/Admin/Mapping/Map?sku=${encodeURIComponent(row.skuCode.code)}" class="inline-flex justify-center px-4 py-2 text-sm font-medium text-white bg-green-500 rounded-md hover:bg-green-700 focus:outline-none">Mapping Action</a>`;


                }
            }
        ]
    });
}
