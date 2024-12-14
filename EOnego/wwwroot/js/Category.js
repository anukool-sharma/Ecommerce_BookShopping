var TbData;
$(document).ready(function () {
    loadDataTable();
})
function loadDataTable() {
    TbData = $('#tblData').DataTable({
        "ajax": {
        "url":"/Admin/Category/GetAll"
        },

        lengthMenu:[3, 6, 9],

        "columns": [

        // Edit Action Button
            {
                "data": "id", "width":"20%",
                "render": function (data) {
                    return `<div class="text-center">
                     <a href="/Admin/Category/upsert/${data}"class="btn btn-success"><i class="fas fa-edit"></i> </a>
                    </div>`;
                }
            },
            // For Name 
            { "data": "name", "width": "60%", "class": "text-center" },

            // For Delete
            {
                "data": "id", "class":"text-center",
                "render": function (data) {
                    return `<a class="btn btn-danger" onclick=Delete("/Admin/Category/Delete/${data}")><i class="fas fa-trash-alt"></i></a>`

                }
            }
        ]
    })
}
function Delete(url) {
    //alert(url);
    swal({
        "title": "Want to delete (..)",
        "text": "Good Go for Delete",
        "icon": "error",
        "buttons": true,
         "dangerMode":true
    }).then((WillDelete) => {
        if (WillDelete) {
            $.ajax({
                url: url,
                type: "delete",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        TbData.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
                })
        }
    })
}
