using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
using PostgresqlDatabaseEqualizer.Views;

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
      ChangeButtonColor(ButtonConnectionSource, Color.FromRgb(0x00, 0x7A, 0xCC)); // blue
    }

    private static void ChangeButtonColor(Button button, Color color)
    {
      button.Background = new SolidColorBrush(color);
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
      ChangeButtonColor(ButtonConnectionTarget, Color.FromRgb(0x00, 0x7A, 0xCC)); // blue
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

    private async Task<string> GetConnectionStringFromCombobox(string theSelectedValue, bool isSource = true)
    {
      if (string.IsNullOrEmpty(theSelectedValue))
      {
        return string.Empty;
      }

      // get connection string items from the selected item
      string filename = FindFilenameForSchema(theSelectedValue, !isSource);
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
        else if (item.Contains("Port"))
        {
          result.Append("Port=");
          result.Append(item.Split('=')[1]);
          result.Append(";");
        }
        else if (item.Contains("Username"))
        {
          result.Append("Username=");
          result.Append(item.Split('=')[1]);
          result.Append(";");
        }
        else if (item.Contains("Password"))
        {
          result.Append("Password=");
          result.Append(item.Split('=')[1]);
          result.Append(";");
        }
        else if (item.Contains("Database"))
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
        else if (item.Contains("Port"))
        {
          SourcePort.Text = item.Split('=')[1];
        }
        else if (item.Contains("Username"))
        {
          SourceUsername.Text = item.Split('=')[1];
          SourceSchema.Text = item.Split('=')[1];
        }
        else if (item.Contains("Password"))
        {
          SourcePassword.Password = item.Split('=')[1];
        }
        else if (item.Contains("Database"))
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
        connection = new NpgsqlConnection(await GetConnectionStringFromCombobox(selectedValue, true));
        await connection.OpenAsync();
        LogMessage("PostgreSQL source connection successful! ");
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

    private string GetTargetConnectionString()
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
          server = TargetHost.Text;
          port = TargetPort.Text;
          database = TargetDatabaseName.Text;
          username = TargetUsername.Text;
          password = TargetPassword.Password;
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
          throw new ArgumentException("Target PostgreSQL host is required.");
        }

        if (string.IsNullOrEmpty(database))
        {
          throw new ArgumentException("Target PostgreSQL database is required.");
        }

        if (!int.TryParse(port, out int portNumber) || portNumber <= 0)
        {
          throw new ArgumentException("Invalid Target PostgreSQL port number.");
        }

        return builder.ConnectionString;
      }
      catch (Exception exception)
      {
        throw new ArgumentException($"Error building Target PostgreSQL connection string: {exception.Message}");
      }
    }

    private static void SetButtonSuccess(Button button)
    {
      button.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69)); // green
    }

    private static void SetButtonError(Button button)
    {
      button.Background = new SolidColorBrush(Color.FromRgb(220, 53, 69));
    }

    private static void ResetButtonColor(Button button)
    {
      button.Background = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
    }

    private async void ButtonConnectionTarget_Click(object sender, RoutedEventArgs e)
    {
      NpgsqlConnection connection = null;
      try
      {
        ButtonConnectionTarget.IsEnabled = false;
        ResetButtonColor(ButtonConnectionTarget);
        LogMessage("Testing target PostgreSQL connection...");
        string selectedValue = TargetConnectionString.SelectedItem.ToString();
        connection = new NpgsqlConnection(await GetConnectionStringFromCombobox(selectedValue, false));
        await connection.OpenAsync();
        LogMessage("PostgreSQL target connection successful ");
        SetButtonSuccess(ButtonConnectionTarget);
      }
      catch (Exception exception)
      {
        LogMessage($"PostgreSQL target connection failed: {exception.Message} ");
        SetButtonError(ButtonConnectionTarget);
      }
      finally
      {
        ButtonConnectionTarget.IsEnabled = true;
        // close the connection
        if (connection?.State != System.Data.ConnectionState.Closed)
        {
          await connection.CloseAsync();
        }
      }
    }

    private async void BtnMigrateTables_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        List<TableInfo> selectedSourceTables = null;
        await Dispatcher.InvokeAsync(() =>
        {
          selectedSourceTables = listSourceTables.ItemsSource.Cast<TableInfo>().Where(t => t.IsSelected).ToList();
        });

        if (selectedSourceTables.Count == 0)
        {
          MessageBox.Show("Please select at least one table from the source database to migrate.", "No tables selected", MessageBoxButton.OK, MessageBoxImage.Hand);
          return;
        }

        // Réorganiser les tables en fonction des dépendances
        var orderedTables = await Task.Run(() => GetTablesInDependencyOrder(selectedSourceTables));

        // Afficher l'ordre de migration prévu
        var migrationOrder = string.Join("\n", orderedTables.Select((t, i) => $"{i + 1}. {t.TableName}"));
        var result = MessageBox.Show(
            $"Les tables seront migrées dans l'ordre suivant :\n\n{migrationOrder}\n\nVoulez-vous continuer ?",
            "Ordre de migration",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information
        );

        if (result != MessageBoxResult.Yes)
        {
          return;
        }

        var loadingWindow = new LoadingWindow(this) { Title = "Copying Data ..." };
        loadingWindow.Show();

        try
        {
          foreach (var table in orderedTables)
          {
            try
            {
              await Dispatcher.InvokeAsync(() =>
              {
                loadingWindow.lblStatus.Content = $"Copying table {table.TableName}...";
              });

              await Dispatcher.InvokeAsync(() => MigrateTable(table, TargetSchema.Text));
              LogMessage($"Successfully copied table {table.TableName}");
            }
            catch (Exception exception)
            {
              LogMessage($"Error copying table {table.TableName}: {exception.Message}");
              await Dispatcher.InvokeAsync(() =>
              {
                MessageBox.Show($"Error copying table {table.TableName}: {exception.Message}",
                              "Copy Error", MessageBoxButton.OK, MessageBoxImage.Error);
              });
            }
          }

          await Dispatcher.InvokeAsync(() =>
          {
            MessageBox.Show("Copy completed. Check the log for details.",
                      "Copy Complete", MessageBoxButton.OK, MessageBoxImage.Information);
          });
        }
        finally
        {
          await Dispatcher.InvokeAsync(() =>
          {
            loadingWindow.Close();
          });
        }
      }
      catch (Exception exception)
      {
        LogMessage($"Copy error: {exception.Message}");
        await Dispatcher.InvokeAsync(() =>
        {
          MessageBox.Show($"Copy error: {exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        });
      }
    }

    private List<TableInfo> GetTablesInDependencyOrder(List<TableInfo> tables)
    {
      var orderedTables = new List<TableInfo>();
      var dependencies = new Dictionary<string, HashSet<string>>();
      var processedTables = new HashSet<string>();
      string schema = string.Empty;

      // Get schema in a thread-safe manner
      Application.Current.Dispatcher.Invoke(() =>
      {
        schema = TargetSchema.Text;
      });

      NpgsqlConnection sourceConnection = null;
      NpgsqlConnection targetConnection = null;

      try
      {
        sourceConnection = new NpgsqlConnection(GetSourceConnectionString());
        targetConnection = new NpgsqlConnection(GetTargetConnectionString());

        sourceConnection.Open();
        targetConnection.Open();

        // Get dependencies for each table
        foreach (var table in tables)
        {
          using (var cmd = targetConnection.CreateCommand())
          {
            cmd.CommandTimeout = 0;
            // First, get table list
            cmd.CommandText = @"
                WITH RECURSIVE fk_tree AS (
                    -- Requête de base : obtenir toutes les dépendances directes
                    SELECT DISTINCT
                        tc.table_name as dependent_table,
                        ccu.table_name as referenced_table,
                        tc.constraint_name,
                        1 as level
                    FROM information_schema.table_constraints tc
                    JOIN information_schema.constraint_column_usage ccu 
                        ON tc.constraint_name = ccu.constraint_name
                        AND ccu.table_schema = tc.table_schema
                    WHERE tc.constraint_type = 'FOREIGN KEY'
                        AND tc.table_schema = @schema
                        AND tc.table_name = @tablename

                    UNION ALL

                    -- Partie récursive : obtenir les dépendances indirectes
                    SELECT DISTINCT
                        t.dependent_table,
                        ccu.table_name,
                        tc.constraint_name,
                        t.level + 1
                    FROM fk_tree t
                    JOIN information_schema.table_constraints tc
                        ON tc.table_name = t.referenced_table
                        AND tc.constraint_type = 'FOREIGN KEY'
                        AND tc.table_schema = @schema
                    JOIN information_schema.constraint_column_usage ccu 
                        ON tc.constraint_name = ccu.constraint_name
                        AND ccu.table_schema = tc.table_schema
                )
                SELECT DISTINCT referenced_table, level
                FROM fk_tree
                ORDER BY level DESC";

            cmd.Parameters.AddWithValue("tablename", table.TableName.ToLower());
            cmd.Parameters.AddWithValue("schema", schema);

            dependencies[table.TableName] = new HashSet<string>();
            using (var reader = cmd.ExecuteReader())
            {
              while (reader.Read())
              {
                var referencedTable = reader.GetString(0).ToUpper();
                dependencies[table.TableName].Add(referencedTable);
                LogMessage($"Found dependency: {table.TableName} -> {referencedTable} (level {reader.GetInt32(1)})");
              }
            }
          }
        }
      }
      catch (Exception exception)
      {
        LogMessage($"Error getting dependencies: {exception.Message}");
        throw;
      }
      finally
      {
        sourceConnection?.Dispose();
        targetConnection?.Dispose();
      }

      // Fonction récursive pour ajouter les tables dans le bon ordre
      void ProcessTable(string tableName, HashSet<string> processingStack = null)
      {
        if (processedTables.Contains(tableName))
        {
          return;
        }

        processingStack = processingStack ?? new HashSet<string>();

        // Detect circular dependencies
        if (processingStack.Contains(tableName))
        {
          LogMessage($"Warning: Circular dependency detected for table {tableName}");
          return;
        }

        processingStack.Add(tableName);

        if (dependencies.ContainsKey(tableName))
        {
          foreach (var dep in dependencies[tableName])
          {
            if (tables.Any(t => t.TableName.Equals(dep, StringComparison.OrdinalIgnoreCase)))
            {
              ProcessTable(dep, processingStack);
            }
            else
            {
              // Si une table dépendante n'est pas sélectionnée, afficher un avertissement
              LogMessage($"Warning: Table {tableName} depends on {dep} but {dep} is not selected for migration");
            }
          }
        }

        processingStack.Remove(tableName);
        processedTables.Add(tableName);

        var tableInfo = tables.First(t => t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        if (!orderedTables.Contains(tableInfo))
        {
          orderedTables.Add(tableInfo);
          LogMessage($"Added {tableInfo.TableName} to migration queue");
        }
      }

      // Add all the tables in order
      foreach (var table in tables)
      {
        ProcessTable(table.TableName);
      }

      // Display the final migration order
      var migrationOrder = string.Join(" -> ", orderedTables.Select(t => t.TableName));
      LogMessage($"Migration order: {migrationOrder}");

      return orderedTables;
    }

    private void MigrateTable(TableInfo targetTable, string selectedSchema)
    {
      LoadingWindow loadingWindow = null;
      string schema = selectedSchema;
      NpgsqlConnection sourceConnection = null;
      NpgsqlConnection targetConnection = null;

      try
      {
        sourceConnection = new NpgsqlConnection(GetSourceConnectionString());
        targetConnection = new NpgsqlConnection(GetTargetConnectionString());

        sourceConnection.Open();
        targetConnection.Open();

        // Désactiver temporairement les contraintes de clé étrangère
        using (var disableCmd = new NpgsqlCommand($@"
            SELECT tc.constraint_name
            FROM information_schema.table_constraints tc
            WHERE tc.constraint_type = 'FOREIGN KEY'
            AND tc.table_schema = @schema
            AND tc.table_name = @tablename", targetConnection))
        {
          disableCmd.Parameters.AddWithValue("schema", schema);
          disableCmd.Parameters.AddWithValue("tablename", targetTable.TableName.ToLower());

          var constraints = new List<string>();
          using (var reader = disableCmd.ExecuteReader())
          {
            while (reader.Read())
            {
              constraints.Add(reader.GetString(0));
            }
          }

          // Désactiver chaque contrainte de clé étrangère
          foreach (var constraint in constraints)
          {
            using (var alterCmd = new NpgsqlCommand($"ALTER TABLE {schema}.{targetTable.TableName.ToLower()} ALTER CONSTRAINT \"{constraint}\" DEFERRABLE INITIALLY DEFERRED", targetConnection))
            {
              alterCmd.ExecuteNonQuery();
            }
          }
        }

        // Vider la table cible
        using (var truncateCmd = new NpgsqlCommand($"TRUNCATE TABLE {schema}.{targetTable.TableName.ToLower()} CASCADE", targetConnection))
        {
          truncateCmd.ExecuteNonQuery();
        }

        // Réinitialiser la séquence si elle existe
        using (var cmd = new NpgsqlCommand(@"
            SELECT pg_get_serial_sequence(@schema || '.' || @tablename, column_name) as sequence_name
            FROM information_schema.columns 
            WHERE table_schema = @schema 
            AND table_name = @tablename 
            AND column_default LIKE 'nextval%'", targetConnection))
        {
          cmd.Parameters.AddWithValue("schema", schema);
          cmd.Parameters.AddWithValue("tablename", targetTable.TableName.ToLower());

          using (var reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              string sequenceName = reader.GetString(0);
              if (!string.IsNullOrEmpty(sequenceName))
              {
                using (var seqCmd = new NpgsqlCommand($"ALTER SEQUENCE {sequenceName} RESTART WITH 1", targetConnection))
                {
                  seqCmd.ExecuteNonQuery();
                }
              }
            }
          }
        }

        // Récupérer la structure de la table
        var columnsQuery = $@"SELECT column_name, data_type, data_length, data_precision, data_scale, nullable, column_id
                            FROM all_tab_columns 
                            WHERE table_name = '{targetTable.TableName}' 
                            ORDER BY column_id";

        var columns = new List<ColumnInfo>();
        var processedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = new NpgsqlCommand(columnsQuery, sourceConnection))
        {
          using (var reader = cmd.ExecuteReader())
          {
            while (reader.Read())
            {
              var columnName = reader.GetString(0);
              if (!processedColumns.Contains(columnName))
              {
                processedColumns.Add(columnName);
                columns.Add(new ColumnInfo
                {
                  ColumnName = columnName,
                  DataType = reader.GetString(1),
                  Length = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                  Precision = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                  Scale = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                  IsNullable = reader.GetString(5) == "Y"
                });
              }
              else
              {
                var columnId = reader.GetInt32(6);
                LogMessage($"Warning: Duplicate column {columnName} found in table {targetTable.TableName} at position {columnId}");

                // Get the table's constraints to understand the relationships
                using (var constraintCmd = new NpgsqlCommand($@"
                    SELECT a.constraint_name, a.constraint_type, a.r_constraint_name,
                           b.column_name, b.position
                    FROM all_constraints a
                    JOIN all_cons_columns b ON a.constraint_name = b.constraint_name
                    WHERE a.table_name = '{targetTable.TableName}'
                    AND b.column_name = '{columnName}'", sourceConnection))
                {
                  using (var constraintReader = constraintCmd.ExecuteReader())
                  {
                    while (constraintReader.Read())
                    {
                      var constraintInfo = $"Constraint: {constraintReader.GetString(0)}, " +
                                         $"Type: {constraintReader.GetString(1)}, " +
                                         $"Position: {constraintReader.GetInt32(4)}";
                      LogMessage($"Related constraint for duplicate column {columnName}: {constraintInfo}");
                    }
                  }
                }
              }
            }
          }
        }

        // Log table structure information
        LogMessage($"Table {targetTable.TableName} structure:");
        foreach (var col in columns)
        {
          LogMessage($"Column: {col.ColumnName}, Type: {col.DataType}, Nullable: {col.IsNullable}");
        }

        // Construire la requête de sélection
        var columnList = string.Join(", ", columns.Select(c => c.ColumnName));
        var selectQuery = $"SELECT {columnList} FROM {targetTable.TableName}";

        // Construire la requête d'insertion
        var columnListPg = string.Join(", ", columns.Select(c => c.ColumnName.ToLower()));
        var paramList = string.Join(", ", columns.Select(c => $"@{c.ColumnName.ToLower()}"));
        var insertQuery = $"INSERT INTO {schema}.{targetTable.TableName.ToLower()} ({columnListPg}) VALUES ({paramList})";

        // Si la table a une auto-référence, on doit d'abord insérer avec NULL pour la référence
        bool hasAutoReference = false;
        string autoReferenceColumn = null;
        using (var cmd = new NpgsqlCommand(@"
          SELECT DISTINCT
            kcu.column_name
          FROM information_schema.table_constraints tc
          JOIN information_schema.key_column_usage kcu 
              ON tc.constraint_name = kcu.constraint_name
              AND kcu.table_schema = tc.table_schema
          WHERE tc.constraint_type = 'FOREIGN KEY'
              AND tc.table_schema = @schema
              AND tc.table_name = @tablename", targetConnection))
        {
          cmd.Parameters.AddWithValue("schema", schema);
          cmd.Parameters.AddWithValue("tablename", targetTable.TableName.ToLower());

          using (var reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              hasAutoReference = true;
              autoReferenceColumn = reader.GetString(0).ToUpper();
              LogMessage($"Table {targetTable.TableName} has auto-reference on column {autoReferenceColumn}");
            }
          }
        }

        if (hasAutoReference)
        {
          insertQuery = $@"INSERT INTO {schema}.{targetTable.TableName.ToLower()} ({columnListPg}) 
                         VALUES ({string.Join(", ", columns.Select(c =>
                             c.ColumnName.Equals(autoReferenceColumn, StringComparison.OrdinalIgnoreCase)
                             ? "NULL"
                             : $"@{c.ColumnName.ToLower()}"))})"
                         ;
        }

        // Lire les données source
        using (var selectCmd = new NpgsqlCommand(selectQuery, sourceConnection))
        using (var reader = selectCmd.ExecuteReader())
        {
          var insertedRows = new List<Dictionary<string, object>>();

          while (reader.Read())
          {
            var rowData = new Dictionary<string, object>();
            using (var insertCmd = new NpgsqlCommand(insertQuery, targetConnection))
            {
              foreach (var column in columns)
              {
                var value = reader[column.ColumnName];
                if (value == DBNull.Value)
                {
                  insertCmd.Parameters.AddWithValue($"@{column.ColumnName.ToLower()}", DBNull.Value);
                }
                else
                {
                  insertCmd.Parameters.AddWithValue($"@{column.ColumnName.ToLower()}", value);
                }
                rowData[column.ColumnName] = value;
              }

              try
              {
                insertCmd.ExecuteNonQuery();
                insertedRows.Add(rowData);
              }
              catch (Npgsql.PostgresException pgEx) when (pgEx.SqlState == "23503") // Foreign key violation
              {
                var details = $"Foreign key violation in table {targetTable.TableName}:\n";
                details += $"Error: {pgEx.Message}\n";
                details += $"Detail: {pgEx.Detail}\n";
                details += "This usually means the referenced record in the parent table doesn't exist.";
                LogMessage(details);

                // Log the data that caused the violation
                var rowDataStr = string.Join(", ", rowData.Select(kv => $"{kv.Key}={kv.Value}"));
                LogMessage($"Row data that caused violation: {rowDataStr}");

                throw;
              }
              catch (Exception exception)
              {
                LogMessage($"Error inserting row in {targetTable.TableName}: {exception.Message}");
                throw;
              }
            }
          }

          // Si la table a une auto-référence, mettre à jour les références maintenant
          if (hasAutoReference && insertedRows.Any())
          {
            LogMessage($"Updating self-references for table {targetTable.TableName}");
            foreach (var row in insertedRows)
            {
              if (row[autoReferenceColumn] != DBNull.Value)
              {
                var updateQuery = $@"
                  UPDATE {schema}.{targetTable.TableName.ToLower()}
                  SET {autoReferenceColumn.ToLower()} = @parentid
                  WHERE ";

                // Trouver la clé primaire
                using (var cmd = new NpgsqlCommand(@"
                  SELECT DISTINCT kcu.column_name
                  FROM information_schema.table_constraints tc
                  JOIN information_schema.key_column_usage kcu 
                      ON tc.constraint_name = kcu.constraint_name
                      AND kcu.table_schema = tc.table_schema
                  WHERE tc.constraint_type = 'PRIMARY KEY'
                      AND tc.table_schema = @schema
                      AND tc.table_name = @tablename", targetConnection))
                {
                  cmd.Parameters.AddWithValue("schema", schema);
                  cmd.Parameters.AddWithValue("tablename", targetTable.TableName.ToLower());

                  using (var pkReader = cmd.ExecuteReader())
                  {
                    if (pkReader.Read())
                    {
                      var pkColumn = pkReader.GetString(0).ToUpper();
                      updateQuery += $"{pkColumn.ToLower()} = @id";

                      using (var updateCmd = new NpgsqlCommand(updateQuery, targetConnection))
                      {
                        updateCmd.Parameters.AddWithValue("@parentid", row[autoReferenceColumn]);
                        updateCmd.Parameters.AddWithValue("@id", row[pkColumn]);
                        updateCmd.ExecuteNonQuery();
                      }
                    }
                  }
                }
              }
            }
          }
        }

        // Réactiver les contraintes de clé étrangère
        using (var enableCmd = new NpgsqlCommand($@"
            SELECT tc.constraint_name
            FROM information_schema.table_constraints tc
            WHERE tc.constraint_type = 'FOREIGN KEY'
            AND tc.table_schema = @schema
            AND tc.table_name = @tablename", targetConnection))
        {
          enableCmd.Parameters.AddWithValue("schema", schema);
          enableCmd.Parameters.AddWithValue("tablename", targetTable.TableName.ToLower());

          var constraints = new List<string>();
          using (var reader = enableCmd.ExecuteReader())
          {
            while (reader.Read())
            {
              constraints.Add(reader.GetString(0));
            }
          }

          // Réactiver chaque contrainte de clé étrangère
          foreach (var constraint in constraints)
          {
            using (var alterCmd = new NpgsqlCommand($"ALTER TABLE {schema}.{targetTable.TableName.ToLower()} VALIDATE CONSTRAINT \"{constraint}\"", targetConnection))
            {
              alterCmd.ExecuteNonQuery();
            }
          }
        }

        // Réactiver les triggers utilisateur
        using (var enableTriggersCmd = new NpgsqlCommand($"ALTER TABLE {schema}.{targetTable.TableName.ToLower()} ENABLE TRIGGER USER", targetConnection))
        {
          enableTriggersCmd.ExecuteNonQuery();
        }
      }
      catch (Exception exception)
      {
        // S'assurer de réactiver les contraintes même en cas d'erreur
        if (targetConnection != null && targetConnection.State == ConnectionState.Open)
        {
          try
          {
            using (var enableCmd = new NpgsqlCommand($@"
                    SELECT tc.constraint_name
                    FROM information_schema.table_constraints tc
                    WHERE tc.constraint_type = 'FOREIGN KEY'
                    AND tc.table_schema = @schema
                    AND tc.table_name = @tablename", targetConnection))
            {
              enableCmd.Parameters.AddWithValue("schema", schema);
              enableCmd.Parameters.AddWithValue("tablename", targetTable.TableName.ToLower());

              var constraints = new List<string>();
              using (var reader = enableCmd.ExecuteReader())
              {
                while (reader.Read())
                {
                  constraints.Add(reader.GetString(0));
                }
              }

              // Reactivate each foreign key constraint
              foreach (var constraint in constraints)
              {
                using (var alterCmd = new NpgsqlCommand($"ALTER TABLE {schema}.{targetTable.TableName.ToLower()} VALIDATE CONSTRAINT \"{constraint}\"", targetConnection))
                {
                  alterCmd.ExecuteNonQuery();
                }
              }
            }

            // Enable user triggers
            using (var enableTriggersCmd = new NpgsqlCommand($"ALTER TABLE {schema}.{targetTable.TableName.ToLower()} ENABLE TRIGGER USER", targetConnection))
            {
              enableTriggersCmd.ExecuteNonQuery();
            }
          }
          catch
          {
            // Ignore errors when reactivating triggers
          }
        }

        LogMessage($"Error copying table {targetTable.TableName}: {exception.Message}");
        throw;
      }
      finally
      {
        if (sourceConnection != null)
        {
          if (sourceConnection.State == ConnectionState.Open)
          {
            sourceConnection.Close();
          }

          sourceConnection.Dispose();
        }

        if (targetConnection != null)
        {
          if (targetConnection.State == ConnectionState.Open)
          {
            targetConnection.Close();
          }

          targetConnection.Dispose();
        }
      }
    }

    private async void ButtonLoadSourceTables_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        var loadingWindow = new LoadingWindow(this);
        loadingWindow.Show();

        var tables = new List<TableInfo>();
        using (var connection = new NpgsqlConnection(GetSourceConnectionString()))
        {
          await connection.OpenAsync();

          using (var cmd = connection.CreateCommand())
          {
            cmd.CommandTimeout = 0;
            // First, get table list
            cmd.CommandText = @"
              SELECT tablename 
              FROM pg_catalog.pg_tables 
              WHERE schemaname = @schema 
              ORDER BY tablename";

            cmd.Parameters.AddWithValue("schema", SourceSchema.Text);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
              while (await reader.ReadAsync())
              {
                tables.Add(new TableInfo { TableName = reader.GetString(0), RowCount = 0 });
              }
            }

            // then count table lines for each table
            foreach (var table in tables)
            {
              cmd.CommandText = $"SELECT COUNT(*) FROM {SourceSchema.Text}.\"{table.TableName}\"";
              table.RowCount = Convert.ToInt64(await cmd.ExecuteScalarAsync());
            }
          }

          listSourceTables.ItemsSource = tables;
          LogMessage($"Successfully loaded {tables.Count} PostgreSQL tables ");
          SetButtonSuccess(buttonLoadSourceTables);

          CompareTables();
        }

        loadingWindow.Close();
      }
      catch (Exception exception)
      {
        LogMessage($"Failed to load PostgreSQL tables: {exception.Message}");
        SetButtonError(buttonLoadSourceTables);
      }
    }

    private void CompareTables()
    {
      if (listSourceTables.ItemsSource == null || listTargetTables.ItemsSource == null)
      {
        return;
      }

      var sourceTables = listSourceTables.ItemsSource.Cast<TableInfo>();
      var targetTables = listTargetTables.ItemsSource.Cast<TableInfo>();

      // Init colors
      foreach (var table in sourceTables.Concat(targetTables))
      {
        table.Background = null;
      }

      // Compare tables
      foreach (var oracleTable in sourceTables)
      {
        var postgresTable = targetTables.FirstOrDefault(t => t.TableName.Equals(oracleTable.TableName, StringComparison.OrdinalIgnoreCase));
        if (postgresTable != null)
        {
          var color = oracleTable.RowCount == postgresTable.RowCount
            ? new SolidColorBrush(Colors.LightGreen)
            : new SolidColorBrush(Colors.LightPink);

          oracleTable.Background = color;
          postgresTable.Background = color;
        }
      }
    }

    private async void ButtonLoadTargetTables_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        var loadingWindow = new LoadingWindow(this);
        loadingWindow.Show();

        var tables = new List<TableInfo>();
        using (var connection = new NpgsqlConnection(GetTargetConnectionString()))
        {
          await connection.OpenAsync();

          using (var cmd = connection.CreateCommand())
          {
            cmd.CommandTimeout = 0;
            // First, get table list
            cmd.CommandText = @"
              SELECT tablename 
              FROM pg_catalog.pg_tables 
              WHERE schemaname = @schema 
              ORDER BY tablename";

            cmd.Parameters.AddWithValue("schema", TargetSchema.Text);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
              while (await reader.ReadAsync())
              {
                tables.Add(new TableInfo { TableName = reader.GetString(0), RowCount = 0 });
              }
            }

            // then count table lines for each table
            foreach (var table in tables)
            {
              cmd.CommandText = $"SELECT COUNT(*) FROM {TargetSchema.Text}.\"{table.TableName}\"";
              table.RowCount = Convert.ToInt64(await cmd.ExecuteScalarAsync());
            }
          }

          listTargetTables.ItemsSource = tables;
          LogMessage($"Successfully loaded {tables.Count} PostgreSQL tables ");
          SetButtonSuccess(buttonLoadTargetTables);

          CompareTables();
        }

        loadingWindow.Close();
      }
      catch (Exception exception)
      {
        LogMessage($"Failed to load PostgreSQL tables: {exception.Message}");
        SetButtonError(buttonLoadTargetTables);
      }
    }
  }
}
