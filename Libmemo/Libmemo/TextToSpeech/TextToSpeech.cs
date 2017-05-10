using System;
using Xamarin.Forms;

namespace Libmemo {
    public class TextToSpeechSpeaker {
        public TextToSpeechSpeaker() {
            var native = DependencyService.Get<ITextToSpeech>();
            native.OnStart += (s, e) => { this.OnStart?.Invoke(this, e); };
            native.OnDone += (s, e) => { this.OnEnd?.Invoke(this, e); };
            native.OnStop += (s, e) => { this.OnEnd?.Invoke(this, e); };
            native.OnError += (s, e) => { this.OnEnd?.Invoke(this, e); };
        }

        public event EventHandler<string> OnEnd;
        public event EventHandler<string> OnStart;

        public void Speak(string str, int id) {
            DependencyService.Get<ITextToSpeech>().Speak(str, id);
        }


        public void Stop() {
            DependencyService.Get<ITextToSpeech>().Stop();
        }

        public bool IsSpeaking() {
            return DependencyService.Get<ITextToSpeech>().IsSpeaking();
        }

    }
}