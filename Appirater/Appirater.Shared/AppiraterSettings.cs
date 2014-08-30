using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;

namespace Windows.UI.Xaml
{
    public class AppiraterSettings
    {
        /// <summary>
        /// Place your Windows (Phone) Store app id here.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Your app's name.
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// This is the message your users will see once they've passed the day+launches threshold.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// This is the title of the message alert that users will see.
        /// </summary>
        public string MessageTitle { get; set; }

        /// <summary>
        /// The text of the button that rejects reviewing the app.
        /// </summary>
        public string CancelButton { get; set; }

        /// <summary>
        /// Text of button that will send user to app review page.
        /// </summary>
        public string RateButton { get; set; }

        /// <summary>
        /// Text for button to remind the user to review later.
        /// </summary>
        public string RateLaterButton { get; set; }

        /// <summary>
        /// Users will need to have the same version of your app installed for this many
        /// days before they will be prompted to rate it.
        /// </summary>
        public int DaysUntilPrompt { get; set; }

        /// <summary>
        /// An example of a 'use' would be if the user launched the app. Bringing the app
        /// into the foreground (on devices that support it) would also be considered
        /// a 'use'. You tell Appirater about these events using the two methods:
        /// [Appirater appLaunched:]
        /// [Appirater appEnteredForeground:]
        ///
        /// Users need to 'use' the same version of the app this many times before
        /// before they will be prompted to rate it.
        /// </summary>
        public int UsesUntilPrompt { get; set; }

        /// <summary>
        /// A significant event can be anything you want to be in your app. In a
        /// telephone app, a significant event might be placing or receiving a call.
        /// In a game, it might be beating a level or a boss. This is just another
        /// layer of filtering that can be used to make sure that only the most
        /// loyal of your users are being prompted to rate you on the app store.
        /// If you leave this at a value of -1, then this won't be a criteria
        /// used for rating. To tell Appirater that the user has performed
        /// a significant event, call the method:
        /// [Appirater userDidSignificantEvent:];
        /// </summary>
        public int SigEventsUntilPrompt { get; set; }

        /// <summary>
        /// Once the rating alert is presented to the user, they might select
        /// 'Remind me later'. This value specifies how long (in days) Appirater
        /// will wait before reminding them.
        /// </summary>
        public int TimeBeforeReminding;

        /// <summary>
        /// 'YES' will show the Appirater alert everytime. Useful for testing how your message
        /// looks and making sure the link to your app's review page works.
        /// </summary>
        public bool Debug { get; set; }

        //public AppiraterSettings(int appId)
        //    : this(appId, (NSString)NSBundle.MainBundle.InfoDictionary.ObjectForKey(new NSString("CFBundleName")), false)
        //{
        //}

        //public AppiraterSettings(int appId, bool debug)
        //    : this(appId, (NSString)NSBundle.MainBundle.InfoDictionary.ObjectForKey(new NSString("CFBundleName")), debug)
        //{
        //}

#if WINDOWS_APP
        public AppiraterSettings(string appName, bool debug)
            : this(Windows.ApplicationModel.Package.Current.Id.FamilyName, appName, debug)
        {

        }
#endif

#if WINDOWS_PHONE_APP
        public AppiraterSettings(string appName, bool debug)
            : this(GetAppId(), appName, debug)
        {

        }

        private static string GetAppId()
        {
            var def = XNamespace.Get("http://schemas.microsoft.com/appx/2010/manifest");
            var mp = XNamespace.Get("http://schemas.microsoft.com/appx/2014/phone/manifest");
            var appXml = XElement.Load("AppxManifest.xml");
            var appElement = (from manifestData in appXml.Descendants(mp + "PhoneIdentity") select manifestData).SingleOrDefault();

            if (appElement != null)
            {
                return appElement.Attribute("PhoneProductId").Value;
            }

            throw new ArgumentNullException("You must associate the app with the Store or manually provide the appId");
        }
#endif


        public AppiraterSettings(string appId, string appName, bool debug)
        {
            AppId = appId;
            AppName = appName;
            Message = string.Format(AppResources.rate_message, AppName);
            MessageTitle = string.Format(AppResources.rate_title, AppName);
            CancelButton = AppResources.rate_cancel;
            RateButton = string.Format(AppResources.rate, AppName);
            RateLaterButton = AppResources.rate_later;
            DaysUntilPrompt = 30;
            UsesUntilPrompt = 20;
            SigEventsUntilPrompt = -1;
            TimeBeforeReminding = 1;
            Debug = debug;
        }
    }
}
