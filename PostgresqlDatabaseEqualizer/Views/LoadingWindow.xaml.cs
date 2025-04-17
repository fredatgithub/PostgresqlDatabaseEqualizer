using System.Windows;

namespace PostgresqlDatabaseEqualizer.Views
{
  /// <summary>
  /// Logique d'interaction pour LoadingWindow.xaml
  /// </summary>
  public partial class LoadingWindow: Window
  {
    public LoadingWindow()
    {
      InitializeComponent();
    }

    public LoadingWindow(Window owner)
    {
      InitializeComponent();
      Owner = owner;

      // Center the window vs the main window
      Loaded += (s, e) =>
      {
        Left = owner.Left + (owner.Width - Width) / 2;
        Top = owner.Top + (owner.Height - Height) / 2;
      };
    }
  }
}
