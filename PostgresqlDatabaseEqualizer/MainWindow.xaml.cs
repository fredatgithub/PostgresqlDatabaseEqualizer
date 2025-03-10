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
    private List<string> _allSchemas;

    public MainWindow()
    {
      InitializeComponent();
      CheckFilePresence(_schemaFilename);
      LoadConfigurationFileComboBox();
    }

    private void LoadConfigurationFileComboBox()
    {
      cboConfigurationFile.Items.Clear();
      cboConfigurationFile.Items.Add(_schemaFilename);
      cboConfigurationFile.SelectedIndex = 0;
      LoadFileContentIntoTextBox();
    }

    private void LoadFileContentIntoTextBox()
    {
      txtConfigurationFile.Text = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cboConfigurationFile.SelectedItem.ToString()));
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
      // save the content of the textbox to the file by replacing the content

      File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cboConfigurationFile.SelectedItem.ToString()), txtConfigurationFile.Text);
      // reload the content of the file
      LoadFileContentIntoTextBox();
      MessageBox.Show("Configuration file saved", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
    }
  }
}
