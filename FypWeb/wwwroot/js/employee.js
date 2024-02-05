$(document).ready(function () {
    loadDataTable();
});
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/employee/getall' },
        "columns": [
            { data: 'id', "width": "25%" },
            { data: 'fullName', "width": "25%" },
            { data: 'phoneNumber', "width": "25%" },
            { data: 'email', "width": "25%" },
            { data: 'gender', "width": "25%" },
            { data: 'role', "width": "25%" },
            { data: 'registrationDate', "width": "25%" },
            {
                data: 'id',
                render: function (data) {
                    return `<div class="flex gap-x-2">
                                <a asp-action="Details" asp-route-id="@item.Id" class="flex items-center">Details</a>
                                &nbsp;
                                <a href="/admin/employee/edit?id=${data}">
                                    Edit
                                </a>
                                &nbsp;
                                <a href="/admin/employee/delete?id=${data}" class="flex items-center">Delete</a>
                            </div>`;
                }, "width": "35%"
            }

        ]
    });
}


