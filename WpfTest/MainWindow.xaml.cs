using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TasksLibrary;

namespace WpfTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private BackgroundWorker _worker;
		public MainWindow()
		{
			InitializeComponent();
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			TextBox.Clear();
			if (clone.IsChecked ?? false)
			{
				_worker = new BackgroundWorker() {WorkerReportsProgress = true};
				var progress = new ProgressWindow {Owner = this};
				_worker.DoWork += (s, eventarg) => eventarg.Result = TestClone(_worker);
				_worker.RunWorkerCompleted += (s, eventarg) =>
				{
					TextBox.Text += (string) eventarg.Result;
					progress.Close();
				};
				_worker.ProgressChanged +=
					(s, ev) => progress.Dispatcher.Invoke(() => progress.progressBar.Value = ev.ProgressPercentage);
				progress.Show();
				_worker.RunWorkerAsync();
			}
			if (@continue.IsChecked ?? false)
				TestContinueWith();
			if (@lock.IsChecked ?? false)
				TestMutexLock();
			if (@using.IsChecked ?? false)
				TestUsingMutexLock();
			if (random.IsChecked ?? false)
				TestRandomGenerator();
			if (assembly.IsChecked ?? false)
				TestAssemblyCollection();
		}

	}
}
