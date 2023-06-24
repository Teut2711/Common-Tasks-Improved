using Microsoft.Win32;
using System.IO;

namespace Common_Tasks
{
	public partial class FolderSelector
	{
		private readonly string folderPath = string.Empty;

		public FolderSelector()
		{
			// Create a new instance of the OpenFileDialog class
			OpenFileDialog folderBrowserDialog = new OpenFileDialog();

			folderBrowserDialog.CheckFileExists = false;
			folderBrowserDialog.ValidateNames = false;
			folderBrowserDialog.FileName = "Folder Selection";
			folderBrowserDialog.Filter = "Folders|no_files_allowed";

			// Display the folder selection dialog and get the result
			bool? result = folderBrowserDialog.ShowDialog();

			if (result == true)
			{
				folderPath = Path.GetDirectoryName(folderBrowserDialog.FileName);
			}
		}

		public string GetPath()
		{
			return folderPath;
		}
	}
}
