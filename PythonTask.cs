using Microsoft.Win32;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Serilog;
using Newtonsoft.Json.Linq;


namespace Common_Tasks
{
	public class PythonTask

	{
		private static readonly ILogger log = new LoggerConfiguration()
		   .MinimumLevel.Information()
		   .WriteTo.File("./ISIN_grouper.log")
		   .CreateLogger();

		private readonly string pythonScriptName;

		public PythonTask(string pythonScriptName) => this.pythonScriptName = pythonScriptName;

		public void Run()
		{

			if (inputSourcePath != null && saveSourcePath != null)
			{
				    
				Task.Run(() =>
				{
					ProcessStartInfo processStartInfo = new()
					{
						FileName = Path.Combine("./python-3.10.9-embed-amd64", "python.exe"),
						Arguments = $"{Path.Combine(pythonScriptName, "__main__.py")}\" \"{inputSourcePath}\" \"{saveSourcePath}\"",
						UseShellExecute = false,
						CreateNoWindow = true,
						RedirectStandardOutput = true,
						RedirectStandardError = true,
					};


					using var process = new Process { StartInfo = processStartInfo };
					ProgressBarWindow progressBarWindow = new()
					{
						Visibility = Visibility.Visible
					};
					var progressBar = progressBarWindow.progressBar;
					var progressText = progressBarWindow.progressText;

					process.Start();

					process.BeginOutputReadLine();

					process.OutputDataReceived += (sender, args) =>
					{
						if (args.Data != null)
						{
							log.Information($"Received data: {args.Data}");

							JObject? pythonIPCOutput = new(args.Data);
							int TOTAL_READS = 0;
							int TOTAL_SAVES = 0;
							if (pythonIPCOutput != null && pythonIPCOutput.HasValues)
							{
								if (pythonIPCOutput["type"].Equals("TOTAL_READS"))
								{
									TOTAL_READS = int.Parse(pythonIPCOutput["counts"].ToString());
								}
								else if (pythonIPCOutput["type"].Equals("TOTAL_SAVES"))
								{
									TOTAL_SAVES = int.Parse(pythonIPCOutput["counts"].ToString());


								}
								else if (pythonIPCOutput["type"].Equals("READ_FILE"))
								{
									string filename = pythonIPCOutput["filename"].ToString();
									int counter = int.Parse(pythonIPCOutput["counter"].ToString());
									progressBar.Dispatcher.Invoke(() =>
									{
										progressBar.Value = Convert.ToInt32(counter * 100.0 / TOTAL_READS);
										progressText.Text = $"Reading file {filename}";
									});
								}
								else if (pythonIPCOutput["type"].Equals("SAVE_FILE"))
								{
									string filename = pythonIPCOutput["filename"].ToString();
									int counter = int.Parse(pythonIPCOutput["counter"].ToString());
									progressBar.Dispatcher.Invoke(() =>
									{
										progressBar.Value = Convert.ToInt32(counter * 100.0 / TOTAL_SAVES);
										progressText.Text = $"Saving file {filename}";

									});
								}
								else if (pythonIPCOutput["type"].Equals("EXECUTION_COMPLETED"))
								{
									progressBar.Dispatcher.Invoke(() =>
									{
										progressBar.Value = 100;
										progressText.Text = $"Process Completed";

									});
								}


							}
						};

					};

					string errorOutput = process.StandardError.ReadToEnd();

					process.WaitForExit();

					if (process.ExitCode == 0)
					{
						// Do something with the output
						MessageBox.Show($"Successfully saved at: {saveSourcePath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
						log.Information("Successfully saved files");
					}
					else
					{
						// Do something with the error
						var errorText = errorOutput.ToString();

						MessageBox.Show($"Failed to save file: {errorText}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						log.Error($"error:\n {errorOutput}");
					}


				});



			}
		}

		public PythonTask? OpenFile(string filter = "All files (*.*)|*.*", string windowTitle = "Open file")
		{
			OpenFileDialog fileBrowserDialog = new()
			{
				ValidateNames = false,
				Multiselect = false,
				Title = windowTitle,
				CheckFileExists = false,
				Filter = filter
			};

			bool? result = fileBrowserDialog.ShowDialog();

			if (result == true)
			{
				inputSourcePath = Path.GetDirectoryName(fileBrowserDialog.FileName);
			}
			else
			{
				return null;

			}
			return this;

		}

		public PythonTask? OpenDirectory(string windowTitle = "Open Folder")
		{
			var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog
			{
				Description = windowTitle
			};

			// Display the folder selection dialog and get the result
			var result = folderBrowserDialog.ShowDialog();

			if (result == System.Windows.Forms.DialogResult.OK)
			{
				inputSourcePath = folderBrowserDialog.SelectedPath;
			}
			else
			{
				return null;
			}

			return this;

		}

		public PythonTask? SaveFile(string filter = "All files (*.*)|*.*", string windowTitle = "Save file")
		{
			OpenFileDialog fileBrowserDialog = new()
			{
				ValidateNames = false,
				Multiselect = false,
				Title = windowTitle,
				CheckFileExists = false,
				Filter = filter

			};

			// Display the folder selection dialog and get the result
			bool? result = fileBrowserDialog.ShowDialog();

			if (result == true)
			{
				saveSourcePath = Path.GetDirectoryName(fileBrowserDialog.FileName);
			}
			else
			{
				return null;

			}
			return this;

		}

		public PythonTask? SaveDirectory(string windowTitle = "Save folder")
		{
			var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog
			{
				Description = windowTitle
			};

			// Display the folder selection dialog and get the result
			var result = folderBrowserDialog.ShowDialog();

			if (result == System.Windows.Forms.DialogResult.OK)
			{
				saveSourcePath = folderBrowserDialog.SelectedPath;
			}
			else
			{
				return null;
			}

			return this;


		}

		private string? inputSourcePath;
		private string? saveSourcePath;

	}
}