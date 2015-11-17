using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ReactiveBinding
{
    [MarkupExtensionReturnType(typeof(BindingExpression))]
    public class ReactiveBinding : MarkupExtension
    {
        [ConstructorArgument("path")]
        public PropertyPath Path { get; set; }

        public ReactiveBinding() { }

        public ReactiveBinding(PropertyPath path)
        {
            Path = Path;
        }

        sealed class Proxy : INotifyPropertyChanged
        {
            string _Value;
            public string Value
            {
                get { return _Value; }
                set
                {

                    _Value = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private FrameworkElement _frameworkElement;
        private IDisposable _subscription;
        private DependencyProperty _target;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var pvt = serviceProvider as IProvideValueTarget;
            if (pvt == null)
            {
                return null;
            }

            _frameworkElement = pvt.TargetObject as FrameworkElement;
            if (_frameworkElement == null)
            {
                return this;
            }

            _target = pvt.TargetProperty as DependencyProperty;
            if (_target == null)
            {
                return this;
            }

            _frameworkElement.DataContextChanged += FrameworkElement_DataContextChanged;

            var proxy = new Proxy();
            var binding = new Binding()
            {
                Source = proxy,
                Path = new PropertyPath("Value")
            };

            // Make sure we don't leak subscriptions
            _frameworkElement.Unloaded += (e, v) => _subscription.Dispose();

            return binding.ProvideValue(serviceProvider);
        }

        private void FrameworkElement_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var propValue = GetProperty(_frameworkElement.DataContext, Path.Path);
                var obs = propValue as IObservable<object>;
                _subscription = obs.ObserveOnDispatcher()
                                   .Subscribe(n => _frameworkElement.SetValue(_target, n));
                _frameworkElement.DataContextChanged -= FrameworkElement_DataContextChanged;
            }
        }

        private static object GetProperty(object context, string propPath)
        {
            var propValue = propPath
                .Split('.')
                .Aggregate(context, (value, name)
                    => value.GetType()
                        .GetProperty(name)
                        .GetValue(value, null));
            return propValue;
        }

    }
}