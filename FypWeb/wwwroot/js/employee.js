var dataTable;
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
                data: { id: "id", lockoutEnd: "lockoutEnd" },
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();
                    if (lockout > today) {
                        return `
                       <div class="flex gap-x-2">                      
                            <a onclick=LockUnlock('${data.id}') class="w-[100px] focus:outline-none text-white bg-red-700 hover:bg-red-800 focus:ring-4 focus:ring-red-300 font-medium rounded-lg text-sm px-5 py-2.5 me-2 mb-2 dark:bg-red-600 dark:hover:bg-red-700 dark:focus:ring-red-900">
                            Lock
                            </a>
                                <a href="/admin/employee/RoleManagement?userId=${data.id}" class="focus:outline-none text-white bg-red-700 hover:bg-red-800 focus:ring-4 focus:ring-red-300 font-medium rounded-lg text-sm px-5 py-2.5 me-2 mb-2 dark:bg-red-600 dark:hover:bg-red-700 dark:focus:ring-red-900" href="/admin/employee/delete?id=${data}">
                            Permission
                            </a>
                         
                        </div > `
                    }
                    else {
                        return `
                       <div class="flex gap-x-2">                      
                            <a onclick=LockUnlock('${data.id}') class=" w-[100px] focus:outline-none text-white bg-green-700 hover:bg-green-800 focus:ring-4 focus:ring-green-300 font-medium rounded-lg text-sm px-5 py-2.5 me-2 mb-2 dark:bg-green-600 dark:hover:bg-green-700 dark:focus:ring-green-900">
                            Unlock
                            </a>
                                <a href="/admin/employee/RoleManagement?userId=${data.id}" class="focus:outline-none text-white bg-green-700 hover:bg-green-800 focus:ring-4 focus:ring-green-300 font-medium rounded-lg text-sm px-5 py-2.5 me-2 mb-2 dark:bg-green-600 dark:hover:bg-green-700 dark:focus:ring-green-900" href="/admin/employee/delete?id=${data}">
                            Permission
                            </a>
                        
                        </div > `                            
                            
 
                    }


                    ;
                }, "width": "35%"
            }

        ]
    });
}

function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: '/admin/employee/lockunlock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
            else {
                toastr.error(data.message);
            }
        }
    });
}
