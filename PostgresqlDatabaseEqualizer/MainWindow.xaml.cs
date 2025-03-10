using System;
using System.Collections.Generic;
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
    private const string _sourceFilename = "sourceConnectionString.txt";
    private const string _targetFilename = "targetConnectionString.txt";
    private List<string> _allSchemas;
    private string _sourceConnectionString;
    private string _targetConnectionString;

    public MainWindow()
    {
      InitializeComponent();
      CreateFileIfNotExist(_schemaFilename);
      CreateFileIfNotExist(_sourceFilename);
      CreateFileIfNotExist(_targetFilename);
      FillFileIfEmpty(_schemaFilename);
      FillFileIfEmpty(_sourceFilename);
      FillFileIfEmpty(_targetFilename);
      LoadConfigurationFileComboBox();
    }

    private void FillFileIfEmpty(string filename)
    {
      if (filename == _schemaFilename && File.ReadAllLines(filename).Length == 0)
      {
        // write the content
        File.WriteAllText(filename, "schema1");
        // add another schema
        File.AppendAllText(filename, Environment.NewLine + "schema2");
        // add another schema
        File.AppendAllText(filename, Environment.NewLine + "schema3");
        // add another schema
        File.AppendAllText(filename, Environment.NewLine + "schema4");
        _allSchemas = File.ReadAllLines(filename).ToList();
      }

      if (filename == _sourceFilename && File.ReadAllLines(filename).Length == 0)
      {
        // write the content
        // encrypt the content first before writing it to the file
        // TODO : encrypt the content
        File.WriteAllText(filename, "Host=localhost;Port=5432;Username=username;Password=password;Database=databaseName;");
      }

      if (filename == _targetFilename && File.ReadAllLines(filename).Length == 0)
      {
        // write the content
        File.WriteAllText(filename, "Host=localhost;Port=5432;Username=username;Password=password;Database=databaseName;");
      }
    }

    private void LoadConfigurationFileComboBox()
    {
      cboConfigurationFile.Items.Clear();
      cboConfigurationFile.Items.Add(_schemaFilename);
      cboConfigurationFile.Items.Add(_sourceFilename);
      cboConfigurationFile.Items.Add(_targetFilename);
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
      txtConfigurationFile.Text = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cboConfigurationFile.SelectedItem.ToString()));
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
        // create the file
        File.Create(filePath).Close();

      }
    }

    private void BtnSaveConfigurationFile_Click(object sender, RoutedEventArgs e)
    {
      // save the content of the textbox to the file by replacing the content
      File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cboConfigurationFile.SelectedItem.ToString()), txtConfigurationFile.Text);
      // reload the content of the file
      LoadFileContentIntoTextBox();
      MessageBox.Show("Configuration file saved", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void CboConfigurationFile_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
      LoadFileContentIntoTextBox();
    }
  }
}
