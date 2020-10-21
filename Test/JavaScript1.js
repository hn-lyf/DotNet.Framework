/// <reference path="jquery-1.7.1.min.js" />
var qcxt = {
    video: {
        checkPluginInstall: function (type) {
            var e = false;
            if (type == "dh") {
                //大华摄像头
                if (window.ActiveXObject || "ActiveXObject" in window) {
                    try {
                        new ActiveXObject("WebActiveEXE.Plugin.1");
                        e = true;
                    }
                    catch (n) {
                        e = false;
                    }
                }
                else {
                    for (var r = 0, s = navigator.mimeTypes.length; s > r; r++) {
                        if (navigator.mimeTypes[r].type.toLowerCase() == "application/media-plugin-version-3.1.0.2") {
                            e = true;
                        }
                    }
                }
            }
            return e;
        },
        /**
         * 显示视频
         * @param {any} elementId 要显示的视频div id
         * @param {any} width 宽度
         * @param {any} height 高度
         * @param {any} data  deviceConfig 字段的值
         */
        show: function (elementId, width, height, data) {
            if (typeof (data) == "string") {
                data = JSON.parse(data);
            }
            var recordType = data.recordType;//dh 大华，hk=海康威视
            var pluginVideoId = "dhVideo" + new Date().getTime();
            switch (recordType) {
                case "dh"://大华
                    if (qcxt.video.checkPluginInstall(recordType)) {
                        if ($(elementId).find("[data-channel]").length == 0) {
                            if (window.ActiveXObject || "ActiveXObject" in window) {
                                $(elementId).html('<object classid="CLSID:7F9063B6-E081-49DB-9FEC-D72422F2727F" codebase="webrec.cab"  width="' + width + '" height="' + height + '" data-type="dh" data-channel="' + data.channel + '" id="' + pluginVideoId + '"></object>');
                            } else {
                                $(elementId).html('<object type="application/media-plugin-version-3.1.0.2" width="' + width + '" height="' + height + '" data-type="dh" data-channel="' + data.channel + '" id="' + pluginVideoId + '"></object>');
                            }
                            var pluginObject = document.getElementById(pluginVideoId);
                            var ret = pluginObject.LoginDeviceEx(data.recordIP, data.recordPort, data.username || "admin", data.password || "admin123", data.protocol || 0);
                            pluginObject.ProtocolPluginWithWebCall('{"Protocol":"EnablePreviewDBClickFullSreen","Params":{"Enable":true}}');//双击放大
                            pluginObject.SetModuleMode(1);
                            pluginObject.ConnectRealVideo(data.channel - 1, 1);
                        } else {
                            var pluginObject = document.getElementById($(elementId).find("[data-channel]").attr("id"));
                            pluginObject.ConnectRealVideo(data.channel - 1, 1);
                        }

                    } else {
                        $(elementId).html('<div class="nop"  style="background-color: #000;text-align:color: #FFF;min-height: 50px;center;width:' + width + 'px;height:' + height + 'px;line-height:' + height + 'px;" id="' + pluginVideoId + '"><a type="application/octet-stream" style="color:#a6aab2;" href="/video/dh/webplugin.exe">请安装控件包</a></div>');
                    }
                    break;
                case "hk"://海康

                    break;
            }
        }, dis: function (elementId) {
            var id = $(elementId).find("object")[0];
            if ($(id).data("type") == "dh") {
                id.DisConnectRealVideo(parseInt($(id).data("channel")) - 1);
            }
        }
    }
};