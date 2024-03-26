$(document).ready(function () {
    $("#noti_Container").ikrNotificationSetup();
    $.ajax({
        type: "GET",
        dataType: "json",
        url: "/Employee/Notification/GetAllNotifications",
        success: function (data) {
            console.log(data);
  

            $("#noti_Container").ikrNotificationCount({
                NotificationList: data.data,
                NotiFromPropName: "FromRoomName",
                ListTitlePropName: "NotiHeader",
                ListBodyPropName: "NotiBody",
                ControllerName: "Notification",
                ActionName: "AllNotifications",
                Area: "Employee"
            });

        },
        error: function (error) {
            console.log(error);
        }
    });
});
