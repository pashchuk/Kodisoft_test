using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

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
