using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    public class EntrySelectedBehavior : Behavior<Entry> {

        public static readonly BindableProperty SelectedCommandProperty = BindableProperty.Create(
            nameof(SelectedCommand),
            typeof(ICommand),
            typeof(EntrySelectedBehavior));

        public ICommand SelectedCommand {
            get { return (ICommand)GetValue(SelectedCommandProperty); }
            set { SetValue(SelectedCommandProperty, value); }
        }

        public Entry AssociatedObject { get; private set; }

        protected override void OnAttachedTo(Entry bindable) {
            base.OnAttachedTo(bindable);

            AssociatedObject = bindable;
            bindable.Completed += OnEntryCompleted;
            bindable.BindingContextChanged += OnBindingContextChanged;
        }

        protected override void OnDetachingFrom(Entry bindable) {
            base.OnDetachingFrom(bindable);

            AssociatedObject = null;
            bindable.Completed -= OnEntryCompleted;
            bindable.BindingContextChanged -= OnBindingContextChanged;
        }

        private void OnBindingContextChanged(object sender, EventArgs e) {
            OnBindingContextChanged();
        }
        protected override void OnBindingContextChanged() {
            base.OnBindingContextChanged();
            BindingContext = AssociatedObject.BindingContext;
        }

        private void OnEntryCompleted(object sender, EventArgs e) {
            if (SelectedCommand == null) return;

            if (SelectedCommand.CanExecute(null)) {
                SelectedCommand.Execute(null);
            }
        }
    }
}
