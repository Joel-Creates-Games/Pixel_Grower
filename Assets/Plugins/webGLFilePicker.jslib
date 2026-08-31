mergeInto(LibraryManager.library, {
    OpenWebGlFilePicker: function (extensionsPtr, gameObjectNamePtr, callbackMethodNamePtr) {
        var extensions = UTF8ToString(extensionsPtr);
        var gameObjectName = UTF8ToString(gameObjectNamePtr);
        var callbackMethodName = UTF8ToString(callbackMethodNamePtr);

        var fileInput = document.getElementById('unity-webgl-file-input');
        if (!fileInput) {
            fileInput = document.createElement('input');
            fileInput.id = 'unity-webgl-file-input';
            fileInput.type = 'file';
            fileInput.style.display = 'none';
            document.body.appendChild(fileInput);
        }

        fileInput.accept = extensions;
        fileInput.onchange = function (event) {
            var file = event.target.files[0];
            if (file) {
                var url = URL.createObjectURL(file);
                SendMessage(gameObjectName, callbackMethodName, url);
            }
            fileInput.value = '';
        };

        fileInput.click();
    }
});