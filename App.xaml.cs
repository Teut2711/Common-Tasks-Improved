using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Common_Tasks
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public static class GlobalVariables
	{
		public static bool DEBUG { get; set; }
	}

	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// Assign values to global variables
			GlobalVariables.DEBUG = true;

		}
	}
}
