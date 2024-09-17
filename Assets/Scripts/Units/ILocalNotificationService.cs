using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NotificationSamples;

namespace com.F4A.MobileThird
{
	public partial interface ILocalNotificationService
	{
		void Initialize(IList<Data.LocalNotification> localNotificationData, GameNotificationsManager notificationsManager);
		void SetupAllNotifications(MonoBehaviour behaviour);
		void CancelAllNotifications();
		bool AuthorizedNotificationPermission();
		void AddNotification(Data.LocalNotification localNotification);
		void RequestAuthorization(Action completed);

	}
}
