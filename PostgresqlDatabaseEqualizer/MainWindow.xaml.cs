using System;
using System.IO; 
using System.Windows;

namespace PostgresqlDatabaseEqualizer
{
  /// <summary>
  /// Logique d'interaction pour MainWindow.xaml
  /// </summary>
  public partial class MainWindow: Window
  {
    private bool _sourceConnectionOK;
    private bool _destinationConnectionOK;

    public MainWindow()
    {
      InitializeComponent();
      CheckFilePresence("schemas.txt");
    }

    private void ButtonConnectionTarget_Click(object sender, RoutedEventArgs e)
    {
      //SourceConnectionTextBox.IsEnabled = false;
      //SourceConnectionOKIcon.Foreground = Brushes.Green;
      //_sourceConnectionOK = true;
      //MessageBox.Show("Please enter a connection string", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private static void CheckFilePresence(string fileName)
    {
      string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
      if (File.Exists(filePath))
      {
        MessageBox.Show($"{fileName} est présent.");
      }
      else
      {
        MessageBox.Show($"{fileName} est manquant.");
      }
    }
  }
}
