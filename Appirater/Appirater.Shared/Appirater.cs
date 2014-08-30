﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;

namespace Windows.UI.Xaml
{
    public class Appirater
    {
        const string SELECTOR_INCREMENT_AND_RATE = "incrementAndRate:";
        const string SELECTOR_INCREMENT_EVENT_AND_RATE = "incrementSignificantEventAndRate:";
        const string FIRST_USE_DATE = "kAppiraterFirstUseDate";
        const string USE_COUNT = "kAppiraterUseCount";
        const string SIGNIFICANT_EVENT_COUNT = "kAppiraterSignificantEventCount";
        const string CURRENT_VERSION = "kAppiraterCurrentVersion";
        const string RATED_CURRENT_VERSION = "kAppiraterRatedCurrentVersion";
        const string DECLINED_TO_RATE = "kAppiraterDeclinedToRate";
        const string REMINDER_REQUEST_DATE = "kAppiraterReminderRequestDate";
        const string TEMPLATE_REVIEW_URL_W81 = "ms-windows-store:REVIEW?PFN={0}";
        const string TEMPLATE_REVIEW_URL_WP81 = "ms-windows-store:reviewapp?appid={0}";
        readonly AppiraterSettings settings;
        private MessageDialog ratingAlert;

        //public Appirater(int appId)
        //    : this(new AppiraterSettings(appId))
        //{
        //}

        //public Appirater(int appId, bool debug)
        //    : this(new AppiraterSettings(appId, debug))
        //{
        //}

#if WINDOWS_APP
        public Appirater(string appName, bool debug)
            : this(new AppiraterSettings(Windows.ApplicationModel.Package.Current.Id.FamilyName, appName, debug))
        {

        }
#endif

        public Appirater(AppiraterSettings settings)
        {
            this.settings = settings;
            //NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.WillResignActiveNotification, OnAppWillResignActive);
        }

        public MessageDialog RatingAlert { get { return ratingAlert; } }

        /*
             * Returns current app version
             */
        public string CurrentVersion
        {
            get
            {
                return Package.Current.Id.Version.ToString();
            }
        }

        /*
             DEPRECATED: While still functional, it's better to use
             appLaunched:(BOOL)canPromptForRating instead.

             Calls [Appirater appLaunched:YES]. See appLaunched: for details of functionality.
             */
        public void AppLaunched()
        {
            AppLaunched(true);
        }

        /*
             Tells Appirater that the app has launched, and on devices that do NOT
             support multitasking, the 'uses' count will be incremented. You should
             call this method at the end of your application delegate's
             application:didFinishLaunchingWithOptions: method.
		 
             If the app has been used enough to be rated (and enough significant events),
             you can suppress the rating alert
             by passing NO for canPromptForRating. The rating alert will simply be postponed
             until it is called again with YES for canPromptForRating. The rating alert
             can also be triggered by appEnteredForeground: and userDidSignificantEvent:
             (as long as you pass YES for canPromptForRating in those methods).
             */
        public void AppLaunched(bool canPromptForRating)
        {
            IncrementUseCount();
            if (canPromptForRating && RatingConditionsHaveBeenMet() && ConnectedToNetwork())
                ShowRatingAlert();
        }

        /*
             Tells Appirater that the app was brought to the foreground on multitasking
             devices. You should call this method from the application delegate's
             applicationWillEnterForeground: method.
		 
             If the app has been used enough to be rated (and enough significant events),
             you can suppress the rating alert
             by passing NO for canPromptForRating. The rating alert will simply be postponed
             until it is called again with YES for canPromptForRating. The rating alert
             can also be triggered by appLaunched: and userDidSignificantEvent:
             (as long as you pass YES for canPromptForRating in those methods).
             */
        public void AppEnteredForeground(bool canPromptForRating)
        {
            IncrementUseCount();
            if (canPromptForRating && RatingConditionsHaveBeenMet() && ConnectedToNetwork())
                ShowRatingAlert();
        }

        /*
             Tells Appirater that the user performed a significant event. A significant
             event is whatever you want it to be. If you're app is used to make VoIP
             calls, then you might want to call this method whenever the user places
             a call. If it's a game, you might want to call this whenever the user
             beats a level boss.
		 
             If the user has performed enough significant events and used the app enough,
             you can suppress the rating alert by passing NO for canPromptForRating. The
             rating alert will simply be postponed until it is called again with YES for
             canPromptForRating. The rating alert can also be triggered by appLaunched:
             and appEnteredForeground: (as long as you pass YES for canPromptForRating
             in those methods).
             */
        public void UserDidSignificantEvent(bool canPromptForRating)
        {
            IncrementSignificantEventCount();
            if (canPromptForRating && RatingConditionsHaveBeenMet() && ConnectedToNetwork())
                ShowRatingAlert();
        }

        /*
             Tells Appirater to open the App Store page where the user can specify a
             rating for the app. Also records the fact that this has happened, so the
             user won't be prompted again to rate the app.

             The only case where you should call this directly is if your app has an
             explicit "Rate this app" command somewhere.  In all other cases, don't worry
             about calling this -- instead, just call the other functions listed above,
             and let Appirater handle the bookkeeping of deciding when to ask the user
             whether to rate the app.
             */
        public void RateApp()
        {
#if WINDOWS_APP
            var reviewURL = string.Format(TEMPLATE_REVIEW_URL_W81, settings.AppId);
#elif WINDOWS_PHONE_APP
            var reviewURL = string.Format(TEMPLATE_REVIEW_URL_WP81, settings.AppId);
#endif


            AddOrUpdateValue(RATED_CURRENT_VERSION, true);

            Launcher.LaunchUriAsync(new Uri(reviewURL));
        }

        /*
             * Restarts tracking
             */
        public void Restart()
        {
            AddOrUpdateValue(CURRENT_VERSION, Package.Current.Id.Version.ToString());
            AddOrUpdateValue(FIRST_USE_DATE, DateTime.Now);
            AddOrUpdateValue(USE_COUNT, 1);
            AddOrUpdateValue(SIGNIFICANT_EVENT_COUNT, 0);
            AddOrUpdateValue(RATED_CURRENT_VERSION, false);
            AddOrUpdateValue(DECLINED_TO_RATE, false);
            AddOrUpdateValue(REMINDER_REQUEST_DATE, DateTime.MinValue);
        }

        bool ConnectedToNetwork()
        {
            return true;
        }

        async void ShowRatingAlert()
        {
#if WINDOWS_APP
            var alertView = new MessageDialog(settings.Message, settings.MessageTitle);
            alertView.Commands.Add(new UICommand(settings.RateButton) { Id = 0 });
            alertView.Commands.Add(new UICommand(settings.RateLaterButton) { Id = 1 });
            alertView.Commands.Add(new UICommand(settings.CancelButton) { Id = 2 });
#else
            var alertView = new MessageDialog(settings.Message, settings.MessageTitle.ToUpper());
            alertView.Commands.Add(new UICommand(settings.RateButton.ToLower()) { Id = 0 });
            alertView.Commands.Add(new UICommand(settings.RateLaterButton.ToLower()) { Id = 1 });
#endif

            ratingAlert = alertView;
            var result = await alertView.ShowAsync();
            if (result != null)
            {
                var buttonId = (int)result.Id;
                switch (buttonId)
                {
                    case 0:
                        RateApp();
                        break;
                    case 1:
                        AddOrUpdateValue(REMINDER_REQUEST_DATE, DateTime.Now.ToString());
                        AddOrUpdateValue(USE_COUNT, 0);
                        break;
                    case 2:
                        AddOrUpdateValue(DECLINED_TO_RATE, true);
                        break;
                }
            }
        }

        bool RatingConditionsHaveBeenMet()
        {
            if (settings.Debug)
                return true;

            DateTime dateOfFirstLaunch = DateTime.Parse(GetValueOrDefault<string>(FIRST_USE_DATE, DateTime.Now.ToString()));
            TimeSpan timeSinceFirstLaunch = DateTime.Now.Subtract(dateOfFirstLaunch);
            TimeSpan timeUntilRate = new TimeSpan(settings.DaysUntilPrompt, 0, 0, 0);
            if (timeSinceFirstLaunch < timeUntilRate)
                return false;

            // check if the app has been used enough
            int useCount = GetValueOrDefault<int>(USE_COUNT, 0);
            if (useCount < settings.UsesUntilPrompt)
                return false;

            // check if the user has done enough significant events
            int sigEventCount = GetValueOrDefault<int>(SIGNIFICANT_EVENT_COUNT, 0);
            if (sigEventCount < settings.SigEventsUntilPrompt)
                return false;

            // has the user previously declined to rate this version of the app?
            if (GetValueOrDefault<bool>(DECLINED_TO_RATE, false))
                return false;

            // has the user already rated the app?
            if (GetValueOrDefault<bool>(RATED_CURRENT_VERSION, false))
                return false;

            // if the user wanted to be reminded later, has enough time passed?
            DateTime reminderRequestDate = DateTime.Parse(GetValueOrDefault<string>(REMINDER_REQUEST_DATE, DateTime.MinValue.ToString()));
            TimeSpan timeSinceReminderRequest = DateTime.Now.Subtract(reminderRequestDate);
            TimeSpan timeUntilReminder = new TimeSpan(settings.TimeBeforeReminding, 0, 0, 0);
            if (timeSinceReminderRequest < timeUntilReminder)
                return false;

            return true;
        }

        void IncrementUseCount()
        {
            // get the app's version
            string version = CurrentVersion;

            // get the version number that we've been tracking
            var userDefaults = ApplicationData.Current.LocalSettings;
            var trackingVersion = (string)userDefaults.Values[CURRENT_VERSION];
            if (string.IsNullOrEmpty(trackingVersion))
            {
                trackingVersion = version;
                AddOrUpdateValue(CURRENT_VERSION, version);
            }

            if (settings.Debug)
                Debug.WriteLine("APPIRATER Tracking version: {0}", trackingVersion);

            if (trackingVersion == version)
            {
                // check if the first use date has been set. if not, set it.
                if (!userDefaults.Values.ContainsKey(FIRST_USE_DATE))
                {
                    AddOrUpdateValue(FIRST_USE_DATE, DateTime.Now.ToString());
                }

                // increment the use count
                var useCount = GetValueOrDefault<int>(USE_COUNT, 0);
                useCount++;
                AddOrUpdateValue(USE_COUNT, useCount++);

                if (settings.Debug)
                    Debug.WriteLine("APPIRATER Use count: {0}", useCount);
            }
            else
                Restart();
        }

        void IncrementSignificantEventCount()
        {
            // get the app's version
            var version = CurrentVersion;

            // get the version number that we've been tracking
            var userDefaults = ApplicationData.Current.LocalSettings;
            var trackingVersion = (string)userDefaults.Values[CURRENT_VERSION];
            if (string.IsNullOrEmpty(trackingVersion))
            {
                trackingVersion = version;
                AddOrUpdateValue(CURRENT_VERSION, version);
            }

            if (settings.Debug)
                Debug.WriteLine("APPIRATER Tracking version: {0}", trackingVersion);

            if (trackingVersion == version)
            {
                // check if the first use date has been set. if not, set it.
                if (!userDefaults.Values.ContainsKey(FIRST_USE_DATE))
                {
                    AddOrUpdateValue(FIRST_USE_DATE, DateTime.Now.ToString());
                }

                // increment the significant event count
                var sigEventCount = GetValueOrDefault<int>(SIGNIFICANT_EVENT_COUNT, 0);
                sigEventCount++;
                AddOrUpdateValue(SIGNIFICANT_EVENT_COUNT, sigEventCount++);

                if (settings.Debug)
                    Debug.WriteLine("APPIRATER Significant event count: {0}", sigEventCount);
            }
            else
                Restart();
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool AddOrUpdateValue(string key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                // If the value has changed
                if (ApplicationData.Current.LocalSettings.Values[key] != value)
                {
                    // Store the new value
                    ApplicationData.Current.LocalSettings.Values[key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                ApplicationData.Current.LocalSettings.Values.Add(key, value);
                valueChanged = true;
            }

            return valueChanged;
        }


        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="valueType"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private valueType GetValueOrDefault<valueType>(string key, valueType defaultValue)
        {
            valueType value;

            // If the key exists, retrieve the value.
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                value = (valueType)ApplicationData.Current.LocalSettings.Values[key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }

            return value;
        }
    }
}
