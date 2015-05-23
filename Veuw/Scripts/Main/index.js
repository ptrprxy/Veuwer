$(function () {
    $('#filepicker').change(function () {
        $('#filelist').html($.map(this.files, function (x) {
            return x.name;
        }).join("<br>"));
        if (this.files.length > 0) {
            $('#fileupload').removeAttr('disabled');
        } else {
            $('#fileupload').attr("disabled", true);
        }
    });
});