(function ($) {
    $.fn.ikrNotificationSetup = function (options) {
        var defaultSettings = $.extend({
            BeforeSeenColor: "#2E467C",
            AfterSeenColor: "#ccc"
        }, options);
        var parentId = $(this).attr("id");
        if ($.trim(parentId) != "" && parentId.length > 0) {
            $("#" + parentId).append("<div class='ikrNoti_Counter'></div>" +
                "<div class='ikrNoti_Button'><i class='bx bxs-bell' style='color:#ffffff'></i></div>" +
                "<div class='ikrNotifications'>" +
                "<h3>Notifications (<span class='notiCounterOnHead'>0</span>)</h3>" +
                "<div class='ikrNotificationItems'>" +
                "</div>" +
                "<div class='ikrSeeAll'><a href='#'>See All</a></div>" +
                "</div>");

            $('#' + parentId + ' .ikrNoti_Counter')
                .css({ opacity: 0 })
                .text(0)
                .css({ top: '-10px' })
                .animate({ top: '-2px', opacity: 1 }, 500);

            $('#' + parentId + ' .ikrNoti_Button').click(function () {
                $('#' + parentId + ' .ikrNotifications').fadeToggle('fast', 'linear', function () {
                    /*if ($('#' + parentId + ' .ikrNotifications').is(':hidden')) {
                        $('#' + parentId + ' .ikrNoti_Button').css('background-color', defaultSettings.AfterSeenColor);
                    }
                    else $('#' + parentId + ' .ikrNoti_Button').css('background-color', defaultSettings.BeforeSeenColor);*/
                });
                $('#' + parentId + ' .ikrNoti_Counter').fadeOut('slow');
                return false;
            });
            $(document).click(function () {
                $('#' + parentId + ' .ikrNotifications').hide();
                /* if ($('#' + parentId + ' .ikrNoti_Counter').is(':hidden')) {
                     $('#' + parentId + ' .ikrNoti_Button').css('background-color', defaultSettings.AfterSeenColor);
                 }*/
            });
            $('#' + parentId + ' .ikrNotifications').click(function () {
                return false;
            });

            $("#" + parentId).css({
                position: "relative"
            });
        }
    };
    $.fn.ikrNotificationCount = function (options) {
        var defaultSettings = $.extend({
            NotificationList: [],
            NotiFromPropName: "",
            ListTitlePropName: "",
            ListBodyPropName: "",
            ControllerName: "Notification",
            ActionName: "AllNotifications",
            Area: "Employee"
        }, options);
        var parentId = $(this).attr("id");
        if ($.trim(parentId) != "" && parentId.length > 0) {
            $("#" + parentId + " .ikrNotifications .ikrSeeAll").click(function (e) {
                e.preventDefault(); // Prevent the default action to ensure smooth redirection.
                // Correct the URL by removing the extra '/' and properly concatenating the Area
                var url = '/' + defaultSettings.Area + '/' + defaultSettings.ControllerName + '/' + defaultSettings.ActionName;
                window.open(url, '_blank'); // Opens the constructed URL in a new tab.
            });

            var totalUnReadNoti = defaultSettings.NotificationList.filter(x => x.isRead == false).length;
            $('#' + parentId + ' .ikrNoti_Counter').text(totalUnReadNoti);
            $('#' + parentId + ' .notiCounterOnHead').text(totalUnReadNoti);
            if (defaultSettings.NotificationList.length > 0) {
                $.map(defaultSettings.NotificationList, function (item) {
                    var className = item.isRead ? "" : " ikrSingleNotiDivUnReadColor";
                    var sNotiFromPropName = $.trim(defaultSettings.NotiFromPropName) == "" ? "" : item[ikrLowerFirstLetter(defaultSettings.NotiFromPropName)];
                    $("#" + parentId + " .ikrNotificationItems").append("<div class='ikrSingleNotiDiv" + className + "' notiId=" + item.notiId + ">" +
                        "<h4 class='ikrNotiFromPropName'>" + sNotiFromPropName + "</h4>" +
                        "<h5 class='ikrNotificationTitle'>" + item[ikrLowerFirstLetter(defaultSettings.ListTitlePropName)] + "</h5>" +
                        "<div class='ikrNotificationBody'>" + item[ikrLowerFirstLetter(defaultSettings.ListBodyPropName)] + "</div>" +
                        "<div class='ikrNofiCreatedDate'>" + item.createdDateSt + "</div>" +
                        "</div>");
                    $("#" + parentId + " .ikrNotificationItems .ikrSingleNotiDiv[notiId=" + item.notiId + "]").click(function () {
                        var url = item.url; // Get the URL from the notification item

                        // Verify if URL is available
                        if (url) {
                            var productId = url.split('/').pop(); // Extract the product ID from the URL
                            console.log('Product ID:', productId); // Log the product ID to verify it's correct

                            // Verify if productId is available
                            if (productId) {
                                var redirectUrl = '/Employee/Product/Details/' + productId;
                                console.log('Navigating to:', redirectUrl); // Log the URL to verify it's correct

                                // Mark the notification as unread by updating the isRead property
                                item.isRead = false;

                                // Update the notification counter
                                var totalUnReadNoti = defaultSettings.NotificationList.filter(x => x.isRead == false).length;
                                $('#' + parentId + ' .ikrNoti_Counter').text(totalUnReadNoti);
                                $('#' + parentId + ' .notiCounterOnHead').text(totalUnReadNoti);

                                // Navigate to the product details page
                                window.location.href = redirectUrl;
                            } else {
                                console.error('Product ID is not available in the URL:', url);
                                // You can display an error message or perform any other action here
                            }
                        } else {
                            console.error('URL is not available in the notification item:', item);
                            // You can display an error message or perform any other action here
                        }
                    });





                });
            }
        }
    };
}(jQuery));

function ikrLowerFirstLetter(value) {
    return value.charAt(0).toLowerCase() + value.slice(1);
}
