using Android.Speech.Tts;
using Xamarin.Forms;
using System.Collections.Generic;
using Libmemo.Droid;
using System;
using Android.Runtime;

[assembly: Xamarin.Forms.Dependency(typeof(ITextToSpeechImplementation))]
namespace Libmemo.Droid {
    public class ITextToSpeechImplementation : Java.Lang.Object, ITextToSpeech, TextToSpeech.IOnInitListener {
        TextToSpeech speaker;
        string toSpeak;
        string toId;
        TTSListener listener;

        public event EventHandler<string> OnStart;
        public event EventHandler<string> OnStop;
        public event EventHandler<string> OnError;
        public event EventHandler<string> OnDone;

        public ITextToSpeechImplementation() {
            var ctx = Forms.Context; // useful for many Android SDK features
            speaker = new TextToSpeech(ctx, this);
            speaker.SetLanguage(new Java.Util.Locale("ru-RU"));

            listener = new TTSListener();
            listener.Start += (s, e) => { this.OnStart.Invoke(this, e); };
            listener.Done += (s, e) => { this.OnDone?.Invoke(this, e); };
            listener.Error += (s, e) => { this.OnError?.Invoke(this, e); };
            listener.Stop += (s, e) => { this.OnStop?.Invoke(this, e); };
            speaker.SetOnUtteranceProgressListener(listener);
        }

        public void Speak(string text, int id) {

            toSpeak = text;
            toId = id.ToString();


            speaker.Speak(toSpeak, QueueMode.Flush, null, toId);
        }

        public void Stop() {
            if (speaker != null && speaker.IsSpeaking)
                speaker.Stop();
        }

        public bool IsSpeaking() {
            if (speaker != null && speaker.IsSpeaking) return true;
            else return false;
        }

        #region IOnInitListener implementation
        public void OnInit(OperationResult status) {
            if (status.Equals(OperationResult.Success)) {
                speaker.Speak(toSpeak, QueueMode.Flush, null, toId);
            }
        }
        #endregion
    }

    class TTSListener : UtteranceProgressListener {
        public event EventHandler<string> Done;
        public override void OnDone(string utteranceId) {
            Done?.Invoke(this, utteranceId);
        }

        public event EventHandler<string> Error;
        public override void OnError(string utteranceId) {
            Error?.Invoke(this, utteranceId);
        }
        public override void OnError(string utteranceId, [GeneratedEnum] TextToSpeechError errorCode) {
            Error?.Invoke(this, utteranceId);
        }

        public event EventHandler<string> Start;
        public override void OnStart(string utteranceId) {
            Start?.Invoke(this, utteranceId);
        }

        public event EventHandler<string> Stop;
        public override void OnStop(string utteranceId, bool interrupted) {
            Stop?.Invoke(this, utteranceId);
        }
    }
}