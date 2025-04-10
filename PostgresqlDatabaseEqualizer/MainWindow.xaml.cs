using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;
using Npgsql;
using PostgresqlDatabaseEqualizer.Helpers;
using PostgresqlDatabaseEqualizer.Models;
using PostgresqlDatabaseEqualizer.Properties;

namespace PostgresqlDatabaseEqualizer
{
  /// <summary>
  /// Interaction Logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow: Window
  {
    private readonly bool _sourceConnectionOK;
    private readonly bool _destinationConnectionOK;
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
    private readonly string _sourceConnectionString;
    private readonly string _targetConnectionString;

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

    private static List<string> GetListOfSchema()
    {
      var decryptedContent = EncryptionHelper.Decrypt(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _schemaFilename)));
      return decryptedContent
           .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
           .ToList();
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
      settings.WindowState = WindowState.ToString();

      // Save window size and position only if it's in Normal state
      if (WindowState == WindowState.Normal)
      {
        settings.WindowTop = Top;
        settings.WindowLeft = Left;
        settings.WindowHeight = Height;
        settings.WindowWidth = Width;
      }

      // Save the current screen's working area (for multi-monitor setups)
      var screen = SystemParameters.WorkArea;
      settings.LastScreenWidth = screen.Width;
      settings.LastScreenHeight = screen.Height;

      settings.Save();
      SaveLogs();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      LoadWindowSize();
      LoadLogs();
      LoadConnectionComboBoxes();
    }

    private void LoadWindowSize()
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
    }

    private void LoadConnectionComboBoxes()
    {
      // load the connection strings into the comboboxes of the connection tab
      var listOfItems = GetListOfSchema();
      SourceConnectionString.Items.Clear();
      TargetConnectionString.Items.Clear();
      foreach (var item in listOfItems)
      {
        SourceConnectionString.Items.Add(item);
        TargetConnectionString.Items.Add(item);
      }
    }

    private async void SourceConnectionString_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      string selectedValue = SourceConnectionString.SelectedItem.ToString();
      if (selectedValue == null)
      {
        return;
      }

      LogMessage($"La sélection de la source de la connection string est : {selectedValue}");
      await LoadSourceConnectionItems(selectedValue);
    }

    private async void TargetConnectionString_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      string selectedValue = TargetConnectionString.SelectedItem.ToString();
      if (selectedValue == null)
      {
        return;
      }

      LogMessage($"La sélection de la target de la connection string est : {selectedValue}");
      await LoadTargetConnectionItems(selectedValue);
    }

    private async Task LoadTargetConnectionItems(string theSelectedValue)
    {
      if (string.IsNullOrEmpty(theSelectedValue))
      {
        return;
      }

      // get connection string items from the selected item
      string filename = FindFilenameForSchema(theSelectedValue);
      var decryptedContent = EncryptionHelper.Decrypt(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)));
      var connectionStringItems = decryptedContent.Split(';').ToList();
      foreach (var item in connectionStringItems)
      {
        if (item.Contains("Host"))
        {
          TargetHost.Text = item.Split('=')[1];
        }
        if (item.Contains("Port"))
        {
          TargetPort.Text = item.Split('=')[1];
        }
        if (item.Contains("Username"))
        {
          TargetUsername.Text = item.Split('=')[1];
          TargetSchema.Text = item.Split('=')[1];
        }
        if (item.Contains("Password"))
        {
          TargetPassword.Password = item.Split('=')[1];
        }
        if (item.Contains("Database"))
        {
          TargetDatabaseName.Text = item.Split('=')[1];
        }
      }
    }

    private async Task<string> GetConnectionStringFromCombobox(string theSelectedValue)
    {
      if (string.IsNullOrEmpty(theSelectedValue))
      {
        return string.Empty;
      }

      // get connection string items from the selected item
      string filename = FindFilenameForSchema(theSelectedValue);
      var decryptedContent = EncryptionHelper.Decrypt(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)));
      var connectionStringItems = decryptedContent.Split(';').ToList();
      var result = new StringBuilder();
      foreach (var item in connectionStringItems)
      {
        if (item.Contains("Host"))
        {
          result.Append("Host=");
          result.Append(item.Split('=')[1]);
          result.Append(";");
        }
        if (item.Contains("Port"))
        {
          result.Append("Port=");
          result.Append(item.Split('=')[1]);
          result.Append(";");
        }
        if (item.Contains("Username"))
        {
          result.Append("Username=");
          result.Append(item.Split('=')[1]);
          result.Append(";");
        }
        if (item.Contains("Password"))
        {
          result.Append("Password=");
          result.Append(item.Split('=')[1]);
          result.Append(";");
        }
        if (item.Contains("Database"))
        {
          result.Append("Database=");
          result.Append(item.Split('=')[1]);
          result.Append(";");
        }
      }

      string finalConnectionString = result.ToString();
      return finalConnectionString;
    }

    private async Task LoadSourceConnectionItems(string theSelectedValue)
    {
      if (string.IsNullOrEmpty(theSelectedValue))
      {
        return;
      }

      // get connection string items from the selected item
      string filename = FindFilenameForSchema(theSelectedValue, false);
      var decryptedContent = EncryptionHelper.Decrypt(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)));
      var connectionStringItems = decryptedContent.Split(';').ToList();
      foreach (var item in connectionStringItems)
      {
        if (item.Contains("Host"))
        {
          SourceHost.Text = item.Split('=')[1];
        }
        if (item.Contains("Port"))
        {
          SourcePort.Text = item.Split('=')[1];
        }
        if (item.Contains("Username"))
        {
          SourceUsername.Text = item.Split('=')[1];
          SourceSchema.Text = item.Split('=')[1];
        }
        if (item.Contains("Password"))
        {
          SourcePassword.Password = item.Split('=')[1];
        }
        if (item.Contains("Database"))
        {
          SourceDatabaseName.Text = item.Split('=')[1];
        }
      }
    }

    private static string FindFilenameForSchema(string schemaName, bool targetFilename = true)
    {
      var fileContent = File.ReadAllLines(_schemaFilename);
      var fileContentDecrypted = EncryptionHelper.Decrypt(string.Join(Environment.NewLine, fileContent)).Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
      int schemaIndex = fileContentDecrypted.IndexOf(schemaName);

      var returnSchema = _targetFilename1.Replace("1", (schemaIndex + 1).ToString());
      if (!targetFilename)
      {
        returnSchema = returnSchema.Replace("target", "source");
      }

      return returnSchema;
    }

    private void LoadDatabaseSchemas()
    {
      try
      {
        // Charger les schémas depuis le fichier schemas.txt
        var encryptedContent = File.ReadAllText(_schemaFilename);
        var decryptedContent = EncryptionHelper.Decrypt(encryptedContent);
        var schemas = decryptedContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

        // Mettre à jour les ComboBox des schémas
        SourceConnectionString.Items.Clear();
        TargetConnectionString.Items.Clear();
        foreach (var schema in schemas)
        {
          SourceConnectionString.Items.Add(schema);
          TargetConnectionString.Items.Add(schema);
        }

        LogMessage($"Schémas chargés avec succès ({schemas.Count} schémas trouvés)");
      }
      catch (Exception exception)
      {
        LogMessage($"Erreur lors du chargement des schémas : {exception.Message}");
      }
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

    private async void ButtonConnectionSource_Click(object sender, RoutedEventArgs e)
    {
      NpgsqlConnection connection = null;
      try
      {
        ButtonConnectionSource.IsEnabled = false;
        ResetButtonColor(ButtonConnectionSource);
        LogMessage("Testing source PostgreSQL connection...");
        string selectedValue = SourceConnectionString.SelectedItem.ToString();
        connection = new NpgsqlConnection(await GetConnectionStringFromCombobox(selectedValue));
        await connection.OpenAsync();
        LogMessage("PostgreSQL connection successful! ");
        SetButtonSuccess(ButtonConnectionSource);
      }
      catch (Exception exception)
      {
        LogMessage($"PostgreSQL connection failed: {exception.Message} ");
        SetButtonError(ButtonConnectionSource);
      }
      finally
      {
        ButtonConnectionSource.IsEnabled = true;
        // close the connection
        if (connection?.State != System.Data.ConnectionState.Closed)
        {
          await connection.CloseAsync();
        }
      }
    }

    private string GetSourceConnectionString()
    {
      try
      {
        var server = string.Empty;
        var port = string.Empty;
        var database = string.Empty;
        var username = string.Empty;
        var password = string.Empty;
        Dispatcher.Invoke(() =>
        {
          server = SourceHost.Text;
          port = SourcePort.Text;
          database = SourceDatabaseName.Text;
          username = SourceUsername.Text;
          password = SourcePassword.Password;
        });

        var builder = new NpgsqlConnectionStringBuilder
        {
          Host = server,
          Port = int.Parse(port),
          Database = database,
          Username = username,
          Password = password
        };

        // Vérifier les champs obligatoires
        if (string.IsNullOrEmpty(server))
        {
          throw new ArgumentException("Source PostgreSQL host is required.");
        }

        if (string.IsNullOrEmpty(database))
        {
          throw new ArgumentException("Source PostgreSQL database is required.");
        }

        if (!int.TryParse(port, out int portNumber) || portNumber <= 0)
        {
          throw new ArgumentException("Invalid Source PostgreSQL port number.");
        }

        return builder.ConnectionString;
      }
      catch (Exception exception)
      {
        throw new ArgumentException($"Error building Source PostgreSQL connection string: {exception.Message}");
      }
    }

    private static void SetButtonSuccess(Button button)
    {
      button.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
    }

    private static void SetButtonError(Button button)
    {
      button.Background = new SolidColorBrush(Color.FromRgb(220, 53, 69));
    }

    private string GetSourcePostgresConnectionString()
    {
      try
      {
        var server = string.Empty;
        var port = string.Empty;
        var database = string.Empty;
        Dispatcher.Invoke(() =>
        {
          server = SourceHost.Text;
          port = SourcePort.Text;
          database = SourceDatabaseName.Text;
        });

        var builder = new NpgsqlConnectionStringBuilder();
        var credentials = LoadCredentials("to be replaced filename.txt");

        if (credentials == null || string.IsNullOrEmpty(credentials.Username) || string.IsNullOrEmpty(credentials.Password))
        {
          throw new ArgumentException("PostgreSQL credentials are not configured.");
        }

        builder.Host = server;
        builder.Port = int.Parse(port);
        builder.Database = database;
        builder.Username = credentials.Username;
        builder.Password = credentials.Password;

        // Vérifier les champs obligatoires
        if (string.IsNullOrEmpty(server))
        {
          throw new ArgumentException("Source PostgreSQL host is required.");
        }

        if (string.IsNullOrEmpty(database))
        {
          throw new ArgumentException("Source PostgreSQL database is required.");
        }

        if (!int.TryParse(port, out int portNumber) || portNumber <= 0)
        {
          throw new ArgumentException("Invalid Source PostgreSQL port number.");
        }

        return builder.ConnectionString;
      }
      catch (Exception exception)
      {
        throw new ArgumentException($"Error building Source PostgreSQL connection string: {exception.Message}");
      }
    }

    private static DbCredentials LoadCredentials(string filename)
    {
      try
      {
        if (File.Exists(filename))
        {
          var encrypted = File.ReadAllText(filename);
          var decrypted = EncryptionHelper.Decrypt(encrypted);
          if (!string.IsNullOrEmpty(decrypted))
          {
            return JsonConvert.DeserializeObject<DbCredentials>(decrypted);
          }
        }
        return null;
      }
      catch (Exception exception)
      {
        MessageBox.Show($"Error loading credentials from {filename}: {exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return null;
      }
    }

    private static void ResetButtonColor(Button button)
    {
      button.Background = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
    }

    private void ButtonConnectionTarget_Click(object sender, RoutedEventArgs e)
    {
      // Todo add code 
      MessageBox.Show("Target Connexion to be tested");
    }
  }
}
