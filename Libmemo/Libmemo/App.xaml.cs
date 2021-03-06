﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace Libmemo {

    public partial class App : Application {

        #region Services
        private static TextToSpeechSpeaker _textToSpeech;
        public static TextToSpeechSpeaker TextToSpeech {
            get {
                if (_textToSpeech == null) {
                    _textToSpeech = new TextToSpeechSpeaker();
                }
                return _textToSpeech;
            }
        }

        private static ToastNotificator _toastNotificator;
        public static ToastNotificator ToastNotificator {
            get {
                if (_toastNotificator == null) {
                    _toastNotificator = new ToastNotificator();
                }
                return _toastNotificator;
            }
        }
        #endregion

        #region Pages

        public static void SetShowMenu(bool show = true) {
            (Application.Current.MainPage as MainPage).IsPresented = show;
        }

        public static void InitMenu() {
            ((Application.Current.MainPage as MainPage).Master as MenuPage).SetMenuPage();
        }

        public static MainPage GlobalPage {
            get { return (MainPage)Application.Current.MainPage; }
        }

        #endregion

        public App() {
            InitializeComponent();
            TK.CustomMap.Api.Google.GmsDirection.Init("AIzaSyCFwd7VMckhN6zZdbmCfGO0WXvJyyqh1OA");


            MainPage = new MainPage();
        }

        protected override void OnStart() {

        }

        protected override void OnSleep() {
            App.TextToSpeech.Stop();
        }

        protected override void OnResume() {
            App.TextToSpeech.Stop();
        }
    }
}
