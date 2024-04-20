var dataTable;
$(document).ready(function () {
    loadDataTable();
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/employee/product/getall' }, // Make sure this URL matches the updated action method that includes product counts.
        // DataTable column definition
        "columns": [
            { "data": "id", "width": "15%" },
            { "data": "name", "width": "25%" },
            { "data": "category", "width": "25%" },
            {
                "data": null,
                "render": function (data, type, row) {
                    // Conditionally display discounted price if there's an active discount
                    if (row.isActiveDiscount) {
                        return `<del>Rs. ${row.price}</del> <strong>${row.discountedPrice}</strong>`;
                    } else {
                        return `Rs. ${row.price}`;
                    }
                },
                "width": "25%"
            },
            // If you still want a separate column for discountedPrice, you can hide it or remove this column definition
            {
                "data": "discountedPrice",
                "visible": false, // Hides the discountedPrice column, remove this line if you want to keep the column
                "width": "25%"
            },
            {
                "data": "id",
                "render": function (data) {
                    return `
                <div class="flex gap-x-2 justify-center">
              
                    <a href="/Employee/Product/Details/${data}" class="w-[100px] text-white bg-blue-700 hover:bg-blue-800 font-medium rounded-lg text-sm px-5 py-2.5 me-2 mb-2">
                        Details
                    </a>
                </div>
            `;
                },
                "width": "25%"
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
                    dataTable.ajax.reload();
                    if (data.success) {
                        toastr.success(data.message);
                    } else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}


