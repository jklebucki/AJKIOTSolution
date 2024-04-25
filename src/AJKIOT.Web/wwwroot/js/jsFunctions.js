function downloadFileFromStream(filename, contentType, content) {
    var file = new Blob([content], { type: contentType });
    var a = document.createElement('a');
    var url = window.URL.createObjectURL(file);
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    setTimeout(function () {
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
    }, 0);
}