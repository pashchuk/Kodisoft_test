using System.Windows;
using System.Windows.Input;

namespace WpfTest
{
	/// <summary>
	/// Interaction logic for ProgressWindow.xaml
	/// </summary>
	public partial class ProgressWindow : Window
	{
		private Point _prevPoint;
		public ProgressWindow()
		{
			InitializeComponent();
		}

		private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
				this.DragMove();
		}
	}
}
