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
    private string _schemaFilename;
    private List<string> _allSchemas;

    public MainWindow()
    {
      InitializeComponent();
      _schemaFilename = "schemas.txt";
      CheckFilePresence(_schemaFilename);
      LoadConfigurationFileComboBox();
    }

    private void LoadConfigurationFileComboBox()
    {
      cboConfigurationFile.Items.Clear();
      cboConfigurationFile.Items.Add(_schemaFilename);
    }

    private void ButtonConnectionTarget_Click(object sender, RoutedEventArgs e)
    {
      //SourceConnectionTextBox.IsEnabled = false;
      //SourceConnectionOKIcon.Foreground = Brushes.Green;
      //_sourceConnectionOK = true;
      //MessageBox.Show("Please enter a connection string", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void CheckFilePresence(string fileName)
    {
      string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
      if (File.Exists(filePath))
      {
        // read the content
        _allSchemas = File.ReadAllLines(filePath).ToList();
      }
      else
      {
        // create the file
        File.Create(filePath).Close();
        // write the content
        File.WriteAllText(filePath, "schema1");
        // add another schema
        File.AppendAllText(filePath, Environment.NewLine + "schema2");
        // add another schema
        File.AppendAllText(filePath, Environment.NewLine + "schema3");
        // add another schema
        File.AppendAllText(filePath, Environment.NewLine + "schema4");
        _allSchemas = File.ReadAllLines(filePath).ToList();
      }
    }

    private void BtnSaveConfigurationFile_Click(object sender, RoutedEventArgs e)
    {

    }
  }
}
