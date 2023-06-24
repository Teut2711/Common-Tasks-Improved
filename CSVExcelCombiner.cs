using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;


namespace Common_Tasks
{
	public partial class CSVExcelCombiner
    {
        public CSVExcelCombiner(string folderPath, string saveAsFilePath)
        {
           
                Task.Run(() =>
                {
                    Process process = new();
                    process.StartInfo.FileName = Path.Combine("./python-3.10.9-embed-amd64", "python.exe");
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardError = true;
                    string columnsFilePath = Path.Combine("./spreadsheet_merger", "columns.txt");
                    string scriptArgs = $" {Path.Combine("spreadsheet_merger", "__main__.py")} \"{folderPath}\" \"{columnsFilePath}\" \"{saveAsFilePath}\"";
                    process.StartInfo.Arguments = scriptArgs;
                    process.Start();
                    string errorOutput = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        System.Windows.MessageBox.Show($"Successfully saved at: {saveAsFilePath}", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show($"Failed to save file: {errorOutput}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }


                });

            }

        }
    }




