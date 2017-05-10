﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace Libmemo {
    public partial class App : Application {

        #region Services
        private static PersonRepository _database;
        public static PersonRepository Database {
            get {
                if (_database == null) {
                    _database = new PersonRepository();
                }
                return _database;
            }
        }

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

        public App() {
            InitializeComponent();
            TK.CustomMap.Api.Google.GmsDirection.Init("AIzaSyCFwd7VMckhN6zZdbmCfGO0WXvJyyqh1OA");


            MainPage = new NavigationPage(new MainPage());
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
