var dataTable;
$(document).ready(function () {
    loadDataTable();
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/brand/getall' }, // Make sure this URL matches the updated action method that includes product counts.
        "columns": [
            { "data": "brandID", "width": "15%" },
            { "data": "brandName", "width": "25%" },
            { "data": "productCount", "width": "25%" },

            {
                "data": "brandID",
                "render": function (data) {
                    return `
                        <div class="flex gap-x-2 justify-center">
                            <a href="/Admin/Brand/Edit/${data}" class="  w-[100px] focus:outline-none text-white bg-green-700 hover:bg-green-800 focus:ring-4 focus:ring-green-300 font-medium rounded-lg text-sm px-5 py-2.5 me-2 mb-2 dark:bg-green-600 dark:hover:bg-green-700 dark:focus:ring-green-900">
                                Edit
                            </a>
                            &nbsp;
                            <a onclick=Delete('/admin/brand/delete/${data}')  class=" w-[100px] focus:outline-none text-white bg-red-700 hover:bg-red-800 focus:ring-4 focus:ring-red-300 font-medium rounded-lg text-sm px-5 py-2.5 me-2 mb-2 dark:bg-red-600 dark:hover:bg-red-700 dark:focus:ring-red-900">
                                Delete
                            </a>
                        </div>
                    `;
                }, "width": "25%"
            }
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        // Reload the data table upon successful deletion
                        dataTable.ajax.reload();
                        // Show a success message using toastr
                        toastr.success(data.message);
                    } else {
                        // Show an error message if deletion failed
                        toastr.error(data.message);
                    }
                },
                error: function () {
                    // Show an error message if the AJAX request fails
                    toastr.error("An error occurred while processing the request.");
                }
            });
        }
    });
}
