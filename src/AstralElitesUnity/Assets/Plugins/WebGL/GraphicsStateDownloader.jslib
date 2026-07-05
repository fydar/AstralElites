mergeInto(LibraryManager.library, {
    GSC_DownloadBytes: function (filenamePtr, dataPtr, length) {
        var filename = UTF8ToString(filenamePtr);
        var bytes = new Uint8Array(HEAPU8.buffer, dataPtr, length);
        var blob = new Blob([bytes], { type: 'application/octet-stream' });
        var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }
});
