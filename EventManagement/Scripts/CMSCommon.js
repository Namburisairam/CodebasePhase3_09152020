

function imageFormatCheck() {

    var ext = arguments[0].split('.').pop();
    var btn = arguments[1];

    if (ext == "" || ext == "jpg" || ext == "jpeg" || ext == "png" || ext == "bmp") {
        $(btn).removeAttr("disabled");
    }
    else {
        alert("Only JPG, JPEG, PNG and BMP formattted files are allowed.");
        $(btn).prop("disabled", true);
    }
}

function isNumberKey(evt) {
    var charCode = (evt.which) ? evt.which : event.keyCode
    if (charCode > 31 && (charCode < 48 || charCode > 57))
        return false;
    return true;
}    