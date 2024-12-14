console.log("Jai Shree Raam");
var dataTable;
$(document).ready(function () {
    loadTable();
})
function loadTable() {
    var orderId = $('#dOrder').val();
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/AllOrders/orderAll?id=" + orderId
        },
        "columns": [
            { "data": "product.title", "width": "15%" },
            { "data": "product.author", "width": "15%" },
            { "data": "product.description", "width": "15%" },
            { "data": "count", "width": "15%" },
            { "data": "price", "width": "15%" },
           // {"data":"orderHeader.orderTotal","width":"15%"}
            {
                "data": "null", "width": "10%",
                "render": function (data, type, row) {
                    return (row.price * row.count);
                }
            }
        ],
        "drawCallback": function () {
            var total = 0;
            this.api().rows().every(function () {
                var data = this.data();
                total += data.price * data.count;
            });
            $('#tblData tfoot td').text(total);
        }
        })
}