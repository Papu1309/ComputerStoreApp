using System.Windows;

namespace ComputerStoreApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show($"Произошла ошибка: {args.Exception.Message}\n\n" +
                              $"Пожалуйста, обратитесь к администратору.",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
        }
    }
}