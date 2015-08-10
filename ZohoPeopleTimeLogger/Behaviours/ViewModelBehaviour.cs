using System;
using System.Windows;
using System.Windows.Interactivity;
using Microsoft.Practices.ServiceLocation;

namespace ZohoPeopleTimeLogger.Behaviours
{
    public class ViewModelBehaviour : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty ViewModelTypeProperty = DependencyProperty.Register(
            "ViewModelType", typeof (Type), typeof (ViewModelBehaviour), new PropertyMetadata(default(Type)));

        public Type ViewModelType
        {
            get { return (Type) GetValue(ViewModelTypeProperty); }
            set { SetValue(ViewModelTypeProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObjectOnLoaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObjectOnLoaded;
        }

        private async void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            AssociatedObject.DataContext = ServiceLocator.Current.GetInstance(ViewModelType);

            var viewModel = AssociatedObject.DataContext as ViewModel.ViewModel;
            if (viewModel != null)
            {
                await viewModel.ViewReady();
            }
        }
    }
}