var toupload;
var forward;
var uploadfail = false;

$(function () {
    toupload = [];

    $('#filestreams').change(function () {
        for (var i = 0; i < this.files.length; i++) {
            toupload.push(this.files[i]);
        }

        refreshFileList();
        $('#filestreams').replaceWith($('#filestreams').val('').clone(true));
    });

    $('#fileupload').click(function () {
        forward = '/i/';
        $('#uploaderror,.removeitem').css({ display: 'none' });
        $('.progress').css({ display: 'block' });
        $('#filestreams,#filestreams-label,#urlinput').attr("disabled", true);
        $('#uploaderror').html('');
        uploadfail = false;

        var filedivs = $('.fileentry');
        var ajaxlist = [];
        for (var i = 0; i < toupload.length; i++) {
            var formdata = new FormData();
            formdata.append('image', toupload[i]);

            var index = i;
            ajaxlist.push($.ajax({
                url: '/Home/Upload',
                type: 'POST',
                xhr: createProg(i),
                success: createSuccess(i),
                error: createError(i),
                data: formdata,
                cache: false,
                contentType: false,
                processData: false,
                dataType: 'json'
            }));
        }

        $.when.apply($, ajaxlist).then(function () {
            if (!uploadfail) {
                window.location.href = forward.substring(0, forward.length - 1);
            }
        });
    });

    $('#urlinput').on('paste', function () {
        setTimeout(function () {
            var urlinput = $('#urlinput')[0];
            toupload.push(urlinput.value);
            urlinput.value = '';
            refreshFileList();
        }, 100);
    });
});

function createProg(i) {
    return function () {
        var myXhr = $.ajaxSettings.xhr();
        if (myXhr.upload) {
            myXhr.upload.addEventListener('progress', function (e) {
                if (e.lengthComputable) {
                    $($('.progress .progress-bar')[i]).attr({ 'aria-valuenow': e.loaded, 'aria-valuemax': e.total });
                    var compPer = (e.loaded / e.total * 100).toString() + '%';
                    $($('.progress .progress-bar')[i]).css({ width: compPer });
                    $($('.progress .progress-bar .sr-only')[i]).html(compPer + ' Complete');
                }
            }, false);
        }
        return myXhr;
    };
}

function createSuccess(i) {
    return function (e) {
        if (e.status == 'success') {
            $($('.progress .progress-bar')[i]).addClass('progress-bar-success');
            forward += e.message + ',';
        } else {
            $($('.progress .progress-bar')[i]).addClass('progress-bar-danger');
            $('#uploaderror').css({ display: 'block' });
            $('.removeitem').css({ display: 'inline' });
            $('#filestreams,#filestreams-label,#urlinput').removeAttr('disabled');
            uploadfail = true;

            var errordiv = $('#uploaderror');
            errordiv.html(errordiv.html() + '<br>' + e.message);
        }
    };
}

function createError(i) {
    return function (e) {
        $($('.progress .progress-bar')[i]).addClass('progress-bar-danger');
        $('#uploaderror').css({ display: 'block' });
        $('.removeitem').css({ display: 'inline' });
        $('#filestreams,#filestreams-label,#urlinput').removeAttr('disabled');
        uploadfail = true;

        var errordiv = $('#uploaderror');
        errordiv.html(errordiv.html() + '<br>' + 'There was an error while uploading.');
    }
}

function refreshFileList() {
    if (toupload.length === 0) {
        $('#filelist').html('');
        return;
    }

    var startEntry = '<div class="row fileentry"><div class="col-md-1"><a href="#" class="removeitem">X</a></div><div class="col-md-9">';
    var endEntry = '<div class="progress"><div class="progress-bar" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width:0%"><span class="sr-only">0% Complete</span></div></div></div></div>';
    var filestreams = $('#filestreams')[0];

    $('#filelist').html(startEntry + $.map(toupload, function (x) {
        if (typeof x === 'string') {
            var url = x.replace(/^.*[\\\/]/, '');
            if (url === '')
                url = x;
            return url;
        } else {
            return x.name;
        }
    }).join(endEntry + startEntry) + endEntry);

    var remlinks = $('.removeitem');
    for (var i = 0; i < remlinks.length; i++) {
        $(remlinks[i]).click(createRemove(i));
    }

    if (toupload.length > 0) {
        $('#fileupload').removeAttr('disabled');
    } else {
        $('#fileupload').attr("disabled", true);
    }
}

function createRemove(i) {
    return function () {
        toupload.splice(i, 1);
        refreshFileList();
    }
}