using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo {
    class EntryChangedBehavior : Behavior<Entry> {

        public static readonly BindableProperty ChangedCommandProperty = BindableProperty.Create(
            nameof(ChangedCommand),
            typeof(ICommand),
            typeof(EntryChangedBehavior));

        public ICommand ChangedCommand {
            get { return (ICommand)GetValue(ChangedCommandProperty); }
            set { SetValue(ChangedCommandProperty, value); }
        }

        public Entry AssociatedObject { get; private set; }

        protected override void OnAttachedTo(Entry bindable) {
            base.OnAttachedTo(bindable);

            AssociatedObject = bindable;
            bindable.TextChanged += OnEntryTextChanged;
            bindable.BindingContextChanged += OnBindingContextChanged;
        }

        protected override void OnDetachingFrom(Entry bindable) {
            base.OnDetachingFrom(bindable);

            AssociatedObject = null;
            bindable.TextChanged -= OnEntryTextChanged;
            bindable.BindingContextChanged -= OnBindingContextChanged;
        }

        private void OnBindingContextChanged(object sender, EventArgs e) {
            OnBindingContextChanged();
        }
        protected override void OnBindingContextChanged() {
            base.OnBindingContextChanged();
            BindingContext = AssociatedObject.BindingContext;
        }

        private void OnEntryTextChanged(object sender, EventArgs e) {
            if (ChangedCommand == null) return;

            if (ChangedCommand.CanExecute(null)) {
                ChangedCommand.Execute(null);
            }
        }



    }
}
