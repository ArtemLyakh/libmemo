using System;

namespace Libmemo {
    public interface ITextToSpeech {
        void Speak(string text, int id);
        void Stop();
        bool IsSpeaking();
        event EventHandler<string> OnStart;
        event EventHandler<string> OnStop;
        event EventHandler<string> OnError;
        event EventHandler<string> OnDone;
    }
}