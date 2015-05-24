$(function () {
    $('#filepicker').change(function () {
        var startEntry = '<div class="row fileentry"><div class="col-md-10">';
        var endEntry = '</div></div>';

        $('#filelist').html(startEntry + $.map(this.files, function (x) {
            return x.name;
        }).join(endEntry + startEntry) + endEntry);

        if (this.files.length > 0) {
            $('#fileupload').removeAttr('disabled');
        } else {
            $('#fileupload').attr("disabled", true);
        }
    });

    $('#fileupload').click(function () {
        $('#uploaderror').css({ display: 'none' });
        $('#uploadprog').css({ display: 'block' });
        $('#fileupload,#filepicker-label').attr("disabled", true);
        var formData = new FormData($('form')[0]);
        $.ajax({
            url: '/Home/Upload',
            type: 'POST',
            xhr: function () {
                var myXhr = $.ajaxSettings.xhr();
                if (myXhr.upload) {
                    myXhr.upload.addEventListener('progress', fileprog, false);
                }
                return myXhr;
            },
            //beforeSend: beforeSendHandler,
            success: filesuccess,
            error: fileerror,
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            dataType: 'json'
        });
    });
});

function fileprog(e) {
    if (e.lengthComputable) {
        $('#uploadprog .progress-bar').attr({ 'aria-valuenow': e.loaded, 'aria-valuemax': e.total });
        var compPer = (e.loaded / e.total * 100).toString() + '%';
        $('#uploadprog .progress-bar').css({ width: compPer });
        $('#uploadprog .progress-bar .sr-only').html(compPer + ' Complete');
    }
}

function filesuccess(e) {
    $('#uploadprog .progress-bar').addClass('progress-bar-success');
    window.location.href = e.message;
}

function fileerror(e) {
    $('#uploadprog').css({ display: 'none' });
    $('#uploaderror').css({ display: 'block' });
    if ('message' in e) {
        $('#uploaderror').html(e.message);
    } else {
        $('#uploaderror').html('There was an error while uploading.');
    }
    $('#fileupload,#filepicker-label').removeAttr('disabled');
}
