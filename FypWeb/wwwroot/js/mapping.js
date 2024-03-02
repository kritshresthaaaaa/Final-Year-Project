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
                    var dropdownHtml = `<div class="relative inline-block text-left">
                                            <div>
                                                <button class="inline-flex justify-center w-full px-4 py-2 text-sm font-medium text-white bg-blue-500 rounded-md hover:bg-blue-700 focus:outline-none" type="button" onclick="toggleDropdown(event, 'dropdown${row.skuCode.code}')">
                                                    View All (${data.length})
                                                </button>
                                            </div>
                                            <div id="dropdown${row.skuCode.code}" class="hidden origin-top-right absolute right-0 mt-2 w-56 rounded-md shadow-lg bg-white ring-1 ring-black ring-opacity-5">
                                                <div class="py-1">`;

                    data.forEach(product => {
                        dropdownHtml += `<a href="#" class="text-gray-700 block px-4 py-2 text-sm hover:bg-gray-100">${product.name}</a>`;
                    });

                    dropdownHtml += `    </div>
                                      </div>
                                    </div>`;
                    return dropdownHtml;
                }
            },
            {
                "data": "mapping",
                "width": "10%",
                "render": function (data, type, row) {
                    // Updated button to be green using Tailwind CSS classes
                    return `<a  <a href="/Admin/Mapping/Map/${row.skuCode.code}" class="inline-flex justify-center  px-4 py-2 text-sm font-medium text-white bg-green-500 rounded-md hover:bg-green-700 focus:outline-none" >Mapping Action</a>`;
                }
            }
        ]
    });
}

function toggleDropdown(event, dropdownId) {
    const dropdownMenu = document.getElementById(dropdownId);
    if (dropdownMenu.classList.contains('hidden')) {
        dropdownMenu.classList.remove('hidden');
        dropdownMenu.classList.add('block');
    } else {
        dropdownMenu.classList.add('hidden');
        dropdownMenu.classList.remove('block');
    }
    event.stopPropagation();
}

window.onclick = function (event) {
    if (!event.target.matches('.inline-flex')) {
        var dropdowns = document.getElementsByClassName("origin-top-right");
        var i;
        for (i = 0; i < dropdowns.length; i++) {
            var openDropdown = dropdowns[i];
            if (!openDropdown.classList.contains('hidden')) {
                openDropdown.classList.add('hidden');
            }
        }
    }
};
