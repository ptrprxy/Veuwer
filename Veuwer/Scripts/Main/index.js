var toupload;
var forward = '';

$(function () {
    toupload = [];

    $('#filestreams').change(function () {
        for (var i = 0; i < this.files.length; i++) {
            toupload.push(this.files[i]);
        }
        console.log(toupload);
        refreshFileList();
    });

    $('#fileupload').click(function () {
        $('uploaderror').css({ display: 'none' });
        $('.progress').css({ display: 'block' });
        $('#filestreams,#filestreams-label,#urlinput').attr("disabled", true);
        $('#uploaderror').html('');

        var filedivs = $('.fileentry');
        var ajaxlist = [];
        for (var i = 0; i < toupload.length; i++) {
            var formdata = new FormData();
            formdata.append('image', toupload[i]);
            console.log(toupload[i]);
            console.log(formdata);

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
            window.location.href = forward.substring(1);
        });
    });

    $('#urlinput').on('paste', function () {
        setTimeout(function () {
            var urlinput = $('#urlinput')[0].value;
            toupload.push(urlinput);
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
        $($('.progress .progress-bar')[i]).addClass('progress-bar-success');
        forward += ',' + e.newid;
    };
}

function createError(i) {
    return function (e) {
        $($('.progress .progress-bar')[i]).addClass('progress-bar-danger');
        $('#uploaderror').css({ display: 'block' });
        var errordiv = $('#uploaderror');
        if ('message' in e) {
            errordiv.html(errordiv.html() + '<br>' + e.message);
        } else {
            errordiv.html(errordiv.html() + '<br>' + 'There was an error while uploading.');
        }
        $('#filestreams,#filestreams-label,#urlinput').removeAttr('disabled');
    }
}

function refreshFileList() {
    var startEntry = '<div class="row fileentry"><div class="col-md-10">';
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

    if (toupload.length > 0) {
        $('#fileupload').removeAttr('disabled');
    } else {
        $('#fileupload').attr("disabled", true);
    }
}