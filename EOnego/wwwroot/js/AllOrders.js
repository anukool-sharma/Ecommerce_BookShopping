var dataTable;
$(document).ready(function () {
    loadDataTable();
})
function loadDataTable() {
    dataTable = $('#AllOrder').DataTable({
        "ajax": {
            "url": "/Admin/AllOrders/GetAll"
        },
        "columns": [
            {
                "data": "orderDate", "width": "20%","class":"text-center",
                "render": function (data) {
                    return data.substring(0, 10);
                }
            },
            { "data": "applicationUser.userName", "width": "20%" ,"class":"text-center" },
            { "data": "orderStatus", "width": "20%","class":"text-center" },
            { "data": "orderTotal", "width": "20%","class":"text-center" },
            {
                "data": "id", "width": "15%","class":"text-center",
                "render": function (data) {
                    return `<a href="/Admin/AllOrders/Details/${data}" class="btn btn-success"><i class="fa-solid fa-circle-info"></i></a>`;
                }
            }
        ]
    })
}       