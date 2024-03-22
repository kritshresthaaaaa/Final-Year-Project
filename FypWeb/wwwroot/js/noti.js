$(document).ready(function () {
    $("#noti_Container").ikrNotificationSetup();
    $.ajax({
        type: "GET",
        dataType: "json",
        url: "/FittingRoomEmployee/Home/GetAllNotifications",
        success: function (data) {
            console.log(data);
  

            $("#noti_Container").ikrNotificationCount({
                NotificationList: data.data,
                NotiFromPropName: "FromUserName",
                ListTitlePropName: "NotiHeader",
                ListBodyPropName: "NotiBody",
                ControllerName: "Notification",
                ActionName: "AllNotifications",
                Area: "FittingRoomEmployee"
            });

        },
        error: function (error) {
            console.log(error);
        }
    });
});
