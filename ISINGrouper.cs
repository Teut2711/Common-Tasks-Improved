using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System;

using Serilog;
using Newtonsoft.Json.Linq;

namespace Common_Tasks
{
	public partial class ISINGrouper
	{
		private static readonly ILogger log = new LoggerConfiguration()
			   .MinimumLevel.Information()
			   .WriteTo.File("./ISIN_grouper.log")
			   .CreateLogger();
		public ISINGrouper(string folderPath, string saveFolderPath)
		{

			log.Information($"Starting ISINGrouper for folder '{folderPath}' and save folder '{saveFolderPath}'");


				Task.Run(() =>
				{
				

					var processStartInfo = new ProcessStartInfo
					{
						FileName = Path.Combine("./python-3.10.9-embed-amd64", "python.exe"),
						Arguments = $"{Path.Combine("ISIN_grouper", "__main__.py")} \"{folderPath}\" \"{saveFolderPath}\"",
						UseShellExecute = false,
						CreateNoWindow = true,
						RedirectStandardOutput = true,
						RedirectStandardError = true,
					};

					using (var process = new Process { StartInfo = processStartInfo })
					{
						ProgressBarWindow progressBarWindow = new();
						progressBarWindow.Visibility = System.Windows.Visibility.Visible;
						var progressBar = progressBarWindow.progressBar;
						var progressText = progressBarWindow.progressText;

						process.Start();

						process.BeginOutputReadLine();

						process.OutputDataReceived += (sender, args) =>
						{
							if (args.Data != null)
							{
								log.Information($"Received data: {args.Data}");

								JObject pythonIPCOutput = new JObject(args.Data);
								int TOTAL_READS = 0;
								int TOTAL_SAVES = 0;



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
											progressBar.Value = Convert.ToInt32(counter * 100 / TOTAL_READS);
											progressText.Text = $"Reading file {filename}";
										});
								}
								else if (pythonIPCOutput["type"].Equals("SAVED_FILE"))
								{
									string filename = pythonIPCOutput["filename"].ToString();
									int counter = int.Parse(pythonIPCOutput["counter"].ToString());
									progressBar.Dispatcher.Invoke(() =>
										{
											progressBar.Value = Convert.ToInt32(counter * 100 / TOTAL_SAVES);
											progressText.Text = $"Saving file {filename}";

										});
								}


							}
						};


						string errorOutput = process.StandardError.ReadToEnd();

						process.WaitForExit();

						if (process.ExitCode == 0)
						{
							// Do something with the output
							System.Windows.MessageBox.Show($"Successfully saved at: {saveFolderPath}", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
							log.Information("Successfully saved files");
						}
						else
						{
							// Do something with the error
							var errorText = errorOutput.ToString();

							System.Windows.MessageBox.Show($"Failed to save file: {errorText}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
							log.Error($"error:\n {errorOutput}");
						}

					}



				});

			}

		}
	
}
