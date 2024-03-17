var dataTable;
$(document).ready(function () {
    loadDataTable();
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/discount/getalldiscounts' }, // Make sure this URL matches the updated action method that includes product counts.
        "columns": [
            {
                "data": null, // No data source for row numbers
                "width": "5%",
                "render": function (data, type, row, meta) {
                    // Use the DataTables meta.row for the row index
                    return meta.row + 1;
                }
            },
            { "data": "name", "width": "25%" },
            { "data": "percentage", "width": "25%" },
            { "data": "categoryName", "width": "25%" },
            { "data": "brandName", "width": "25%" },
            { "data": "skuCode", "width": "25%" },
            {
                "data": "startDate", "width": "25%", "render": function (data) {
                    var date = new Date(data);
                    return date.toLocaleDateString();
                }
            },
            {
                "data": "endDate", "width": "25%", "render": function (data) {
                    var date = new Date(data);
                    return date.toLocaleDateString();
                }
            },
            {
                data: { id: "id", isActive: "isActive" },
                "render": function (data) {

                    if (data.isActive) {
                        return `
<button onclick=ToggleStatus('${data.id}','${data.isActive}') class="w-[100px] flex justify-center focus:outline-none text-white bg-red-700 hover:bg-red-800 focus:ring-4 focus:ring-red-300 font-medium rounded-lg text-sm px-5 py-2.5 me-2 mb-2 dark:bg-red-600 dark:hover:bg-red-700 dark:focus:ring-red-900">
                            Deactivate
                        `

                    } else {
                        return `
<button onclick=ToggleStatus('${data.id}','${data.isActive}') class="w-[100px] focus:outline-none text-white bg-green-700 hover:bg-green-800 focus:ring-4 focus:ring-green-300 font-medium rounded-lg text-sm px-5 py-2.5 me-2 mb-2 dark:bg-green-600 dark:hover:bg-green-700 dark:focus:ring-green-900">
                            Activate
                        
                        `

                    }


                }, "width": "15%"
            },


            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="flex gap-x-2 justify-center">
                    
                         
                            <a onclick=Delete('/admin/discount/delete/${data}')  class=" w-[100px] focus:outline-none text-white bg-red-700 hover:bg-red-800 focus:ring-4 focus:ring-red-300 font-medium rounded-lg text-sm px-5 py-2.5 me-2 mb-2 dark:bg-red-600 dark:hover:bg-red-700 dark:focus:ring-red-900">
                                Delete
                            </a>
                        </div>
                    `;
                }, "width": "25%"
            }
        ]
    });
}
function ToggleStatus(id, currentStatus) {
    var isActive = currentStatus === 'true'; // Convert string to boolean
    $.ajax({
        type: "POST",
        url: '/admin/discount/togglestatus',
        contentType: "application/json",
        data: JSON.stringify({ id: id, isActive: !isActive }), // Correctly toggle the status
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            } else {
                toastr.error(data.message);
            }
        },
        error: function (error) {
            toastr.error("An error occurred.");
            console.error(error);
        }
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
                    dataTable.ajax.reload();

                    toastr.success(data.message);
                }
            });
        }
    });
}


