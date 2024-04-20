var dataTable;
$(document).ready(function () {
    loadDataTable();
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/Employee/Transaction/GetAllTransactions' }, // Make sure this URL matches the updated action method that includes product counts.
        "columns": [
            { "data": "id", "width": "10%" },
            { "data": "customerName", "width": "15%" },
            { "data": "customerEmail", "width": "15%" },
            { "data": "customerPhone", "width": "15%" },
            {
                "data": "orderDate",
                "width": "15%",
                "render": function (data, type, row) {
                    // Parse the ISO string to a Date object
                    var date = new Date(data);
                    // Format the date to a locale-specific string
                    return date.toLocaleDateString();
                }
            },
            {
                "data": "orderTotal",
                "width": "15%",
                "render": function (data, type, row) {
                    // Format the order total to display it as Rupees
                    return `Rs. ${data.toFixed(2)}`; // Assuming 'data' is a number; adjust if needed
                }
            },
            {
                "data": "id",
                "width": "10%",
                "render": function (data) {
                    return `
                        <div class="flex gap-x-2 justify-center">
                            <a href="/Employee/Transaction/Details/${data}" class="  w-[100px] focus:outline-none text-white bg-green-700 hover:bg-green-800 focus:ring-4 focus:ring-green-300 font-medium rounded-lg text-sm px-5 py-2.5 me-2 mb-2 dark:bg-green-600 dark:hover:bg-green-700 dark:focus:ring-green-900">
                                Details
                            </a>
                        </div>
                    `;
                }, "width": "25%"
            }
        ]
    });
}

