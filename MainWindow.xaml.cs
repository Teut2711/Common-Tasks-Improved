using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Common_Tasks
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	/// 
	
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}
		private void Edit_Columns(object sender, RoutedEventArgs e)
		{
			string extra_path ="";
			if (GlobalVariables.DEBUG)
				 extra_path = "../../../";
			else
				 extra_path = "";

			string columnsFilePath = Path.Combine(AppContext.BaseDirectory,extra_path, "sheet_merger", "columns.txt");
			Application.Current.Dispatcher.Invoke(() =>
			{
				sheet_merger_button.IsEnabled = false;
			});

			if (!File.Exists(columnsFilePath))
			{
				using (File.Create(columnsFilePath)) { }

			}
			Process process = new();
			process.StartInfo.FileName = "notepad.exe";
			process.StartInfo.Arguments = columnsFilePath;
			process.EnableRaisingEvents = true;
			process.Exited += (s, args) =>
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					sheet_merger_button.IsEnabled = true;
				});
			};

			process.Start();


		}


		private void Group_ISIN(object sender, RoutedEventArgs e)
		{

			try
			{
				PythonTask task = new("isin_grouper");
				task.OpenDirectory()?.SaveFile()?.Run();

			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error executing script: {ex.Message}");
			}
		}



		private void Merge_Spreadsheets(object sender, RoutedEventArgs e)
		{	
			try
			{
				PythonTask task = new("sheet_merger");
				task.OpenDirectory()?.SaveFile()?.Run();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error executing script: {ex.Message}");
			}
		}

		private void Substitute_Text(object sender, RoutedEventArgs e)
		{
			try
			{			
					PythonTask task = new("text_substituter");
					task.OpenDirectory()?.SaveFile()?.Run();
				
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error executing script: {ex.Message}");
			}
		}

	}
}
