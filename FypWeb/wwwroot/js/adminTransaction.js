var dataTable;
$(document).ready(function () {
    loadDataTable();
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/Admin/Transaction/GetAllTransactions' },
        "columns": [
            { "data": "id", "width": "10%" },
            { "data": "customerName", "width": "15%" },
            { "data": "customerEmail", "width": "15%" },
            { "data": "customerPhone", "width": "15%" },
            { "data": "applicationUserId", "width":"10%" }, // Hide applicationUserId column
            { "data": "employeeEmail", "width": "15%" },
            {
                "data": "orderDate",
                "width": "10%", // Adjusted to 10%
                "render": function (data, type, row) {
                    var date = new Date(data);
                    return date.toLocaleDateString();
                }
            },
            {
                "data": "orderTotal",
                "width": "10%", // Adjusted to 10%
                "render": function (data, type, row) {
                    return `Rs. ${data.toFixed(2)}`;
                }
            },
            {
                "data": "id",
                "width": "15%", // Adjusted to 15%
                "render": function (data) {
                    return `
                        <div class="flex gap-x-2 justify-center">
                            <a href="/Admin/Transaction/Details/${data}" class="w-[100px] focus:outline-none text-white bg-green-700 hover:bg-green-800 focus:ring-4 focus:ring-green-300 font-medium rounded-lg text-sm px-5 py-2.5 me-2 mb-2 dark:bg-green-600 dark:hover:bg-green-700 dark:focus:ring-green-900">
                                Details
                            </a>
                        </div>
                    `;
                }
            }
        ]
    });
}
