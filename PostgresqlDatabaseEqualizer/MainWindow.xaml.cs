using PostgresqlDatabaseEqualizer.Helpers;
using PostgresqlDatabaseEqualizer.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace PostgresqlDatabaseEqualizer
{
  /// <summary>
  /// Interaction Logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow: Window
  {
    private bool _sourceConnectionOK;
    private bool _destinationConnectionOK;
    private const string _schemaFilename = "schemas.txt";
    private const string _sourceFilename1 = "sourceConnectionStringSchema1.txt";
    private const string _sourceFilename2 = "sourceConnectionStringSchema2.txt";
    private const string _sourceFilename3 = "sourceConnectionStringSchema3.txt";
    private const string _sourceFilename4 = "sourceConnectionStringSchema4.txt";

    private const string _targetFilename1 = "targetConnectionStringSchema1.txt";
    private const string _targetFilename2 = "targetConnectionStringSchema2.txt";
    private const string _targetFilename3 = "targetConnectionStringSchema3.txt";
    private const string _targetFilename4 = "targetConnectionStringSchema4.txt";

    private List<string> _allSchemas;
    private string _sourceConnectionString;
    private string _targetConnectionString;

    private readonly string _logFile = "log.txt";

    public MainWindow()
    {
      InitializeComponent();
      LogMessage($"Application is starting");
      CreateFileIfNotExist(_schemaFilename);

      CreateFileIfNotExist(_sourceFilename1);
      CreateFileIfNotExist(_sourceFilename2);
      CreateFileIfNotExist(_sourceFilename3);
      CreateFileIfNotExist(_sourceFilename4);

      CreateFileIfNotExist(_targetFilename1);
      CreateFileIfNotExist(_targetFilename2);
      CreateFileIfNotExist(_targetFilename3);
      CreateFileIfNotExist(_targetFilename4);

      FillFileIfEmpty(_schemaFilename);

      FillFileIfEmpty(_sourceFilename1);
      FillFileIfEmpty(_sourceFilename2);
      FillFileIfEmpty(_sourceFilename3);
      FillFileIfEmpty(_sourceFilename4);

      FillFileIfEmpty(_targetFilename1);
      FillFileIfEmpty(_targetFilename2);
      FillFileIfEmpty(_targetFilename3);
      FillFileIfEmpty(_targetFilename4);
      LoadConfigurationFileComboBox();
    }

    private void FillFileIfEmpty(string filename)
    {
      if (filename == _schemaFilename && File.ReadAllLines(filename).Length == 0)
      {
        var clearContent = string.Format("schema1{0}schema2{0}schema3{0}schema4", Environment.NewLine);
        var encryptedContent = EncryptionHelper.Encrypt(clearContent);
        File.WriteAllText(filename, encryptedContent);
        var allSchemasEncrypted = File.ReadAllLines(filename).ToList();
        _allSchemas = new List<string>();
        foreach (var oneSchema in allSchemasEncrypted)
        {
          _allSchemas.Add(EncryptionHelper.Decrypt(oneSchema));
        }
      }

      if (File.ReadAllLines(filename).Length == 0 && (filename == _sourceFilename1 || filename == _sourceFilename2 || filename == _sourceFilename3 || filename == _sourceFilename4))
      {
        // encrypt the content first before writing it to the file
        const string clearContent = "Host=localhost;Port=5432;Username=username;Password=password;Database=databaseName;";
        var encryptedContent = EncryptionHelper.Encrypt(clearContent);
        File.WriteAllText(filename, encryptedContent);
      }

      if (File.ReadAllLines(filename).Length == 0 && (filename == _targetFilename1 || filename == _targetFilename2 || filename == _targetFilename3 || filename == _targetFilename4))
      {
        const string clearContent = "Host=localhost;Port=5432;Username=username;Password=password;Database=databaseName;";
        var encryptedContent = EncryptionHelper.Encrypt(clearContent);
        File.WriteAllText(filename, encryptedContent);
      }
    }

    private void LoadConfigurationFileComboBox()
    {
      cboConfigurationFile.Items.Clear();
      cboConfigurationFile.Items.Add(_schemaFilename);
      cboConfigurationFile.Items.Add(_sourceFilename1);
      cboConfigurationFile.Items.Add(_sourceFilename2);
      cboConfigurationFile.Items.Add(_sourceFilename3);
      cboConfigurationFile.Items.Add(_sourceFilename4);
      cboConfigurationFile.Items.Add(_targetFilename1);
      cboConfigurationFile.Items.Add(_targetFilename2);
      cboConfigurationFile.Items.Add(_targetFilename3);
      cboConfigurationFile.Items.Add(_targetFilename4);
      cboConfigurationFile.SelectedIndex = 0;
      LoadFileContentIntoTextBox();
    }

    private void LoadFileContentIntoTextBox()
    {
      // create the file if it does not exist
      if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cboConfigurationFile.SelectedItem.ToString())))
      {
        File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cboConfigurationFile.SelectedItem.ToString())).Close();
      }
      // load the content of the file into the textbox
      // decrypt the content first before loading it into the textbox
      var decryptedContent = EncryptionHelper.Decrypt(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cboConfigurationFile.SelectedItem.ToString())));
      txtConfigurationFile.Text = decryptedContent;
    }

    private void ButtonConnectionTarget_Click(object sender, RoutedEventArgs e)
    {
      //SourceConnectionTextBox.IsEnabled = false;
      //SourceConnectionOKIcon.Foreground = Brushes.Green;
      //_sourceConnectionOK = true;
      //MessageBox.Show("Please enter a connection string", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private static void CreateFileIfNotExist(string fileName)
    {
      string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
      if (!File.Exists(filePath))
      {
        File.Create(filePath).Close();
      }
    }

    private void BtnSaveConfigurationFile_Click(object sender, RoutedEventArgs e)
    {
      // save the content of the textbox to the file by replacing the content
      // encrypt the content first before writing it to the file
      File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cboConfigurationFile.SelectedItem.ToString()), EncryptionHelper.Encrypt(txtConfigurationFile.Text));
      // reload the content of the file
      LoadFileContentIntoTextBox();
      MessageBox.Show("Configuration file saved", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void CboConfigurationFile_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
      LoadFileContentIntoTextBox();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      var settings = Settings.Default;

      // Save window state (Normal, Maximized)
      settings.WindowState = this.WindowState.ToString();

      // Save window size and position only if it's in Normal state
      if (this.WindowState == WindowState.Normal)
      {
        settings.WindowTop = Top;
        settings.WindowLeft = Left;
        settings.WindowHeight = Height;
        settings.WindowWidth = Width;
      }

      // Save the current screen's working area (for multi-monitor setups)
      var screen = System.Windows.SystemParameters.WorkArea;
      settings.LastScreenWidth = screen.Width;
      settings.LastScreenHeight = screen.Height;

      settings.Save();
      SaveLogs();
    }


    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      // Load window size and position if saved
      var settings = Settings.Default;

      if (settings.WindowWidth > 0 && settings.WindowHeight > 0)
      {
        Width = settings.WindowWidth;
        Height = settings.WindowHeight;
      }

      if (settings.WindowLeft >= 0 && settings.WindowTop >= 0)
      {
        Left = settings.WindowLeft;
        Top = settings.WindowTop;
      }

      // Get the current screen dimensions
      var currentScreen = SystemParameters.WorkArea;

      // Check if the saved screen matches the current screen
      bool isSameScreen = settings.LastScreenWidth == currentScreen.Width &&
                          settings.LastScreenHeight == currentScreen.Height;

      // If the screen setup changed, center the window on the primary screen
      if (!isSameScreen || Left < 0 || Top < 0 || Left + Width > currentScreen.Width || Top + Height > currentScreen.Height)
      {
        Left = (currentScreen.Width - Width) / 2;
        Top = (currentScreen.Height - Height) / 2;
      }

      // Restore the saved window state (Normal or Maximized)
      if (Enum.TryParse(settings.WindowState, out WindowState state))
      {
        WindowState = state;
      }

      LoadLogs();
    }

    private void LoadLogs()
    {
      txtLogs.Text = File.Exists(_logFile) ? File.ReadAllText(_logFile) : string.Empty;
    }

    private void SaveLogs()
    {
      try
      {
        File.WriteAllText(_logFile, txtLogs.Text);
      }
      catch (Exception exception)
      {
        MessageBox.Show($"Error saving logs: {exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void LogMessage(string message)
    {
      string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
      Dispatcher.Invoke(() =>
      {
        txtLogs.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
        txtLogs.ScrollToEnd();
        SaveLogs();
      });
    }
  }
}
