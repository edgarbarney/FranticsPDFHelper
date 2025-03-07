using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;

namespace Frantics_PDF_Helper.Windows
{
    /// <summary>
    /// Interaction logic for DialogueWindow.xaml
    /// </summary>
    public partial class DialogueWindow : Window
    {
		[Flags]
		public enum DialogueButton
		{
			None = 0,

			OK = 1,
			Yes = 2,
			No = 4,
			Cancel = 8,

			Default = OK | Cancel,

		}

		public enum DialogueType 
		{ 
			Normal,
			Input
		}

		public DialogueButton DialogueResult { get; private set; } = DialogueButton.None;
		public string InputText { get => inputTextBox.Text; set => inputTextBox.Text = value; }
		public string DescriptionText { get => descriptionTextBlock.Text; set => descriptionTextBlock.Text = value; }
		public DialogueButton Buttons { get; set; } = DialogueButton.Default;
		public DialogueType Type{ get; set; } = DialogueType.Normal;

        public DialogueWindow()
        {
            InitializeComponent();
        }

		public DialogueWindow(string title, string desc, string defaultInput, DialogueButton dialogueButtons, DialogueType dialogueType)
		{
			InitializeComponent();
			Title = title;
			DescriptionText = desc;
			InputText = defaultInput;
			Buttons = dialogueButtons;
			Type = dialogueType;
			if (dialogueType == DialogueType.Input)
			{
				inputTextBox.Visibility = Visibility.Visible;
			}
			else
			{
				inputTextBox.Visibility = Visibility.Collapsed;
			}

			buttonGrid.ColumnDefinitions.Clear();
			buttonGrid.Children.Clear();

			List<DialogueButton> setFlags = [];
			foreach (DialogueButton value in Enum.GetValues(typeof(DialogueButton)))
			{
				if (value != DialogueButton.None && value != DialogueButton.Default && dialogueButtons.HasFlag(value))
				{
					setFlags.Add(value);
				}
			}

			switch (setFlags.Count)
			{
				case 1:
					buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
					break;
				case 2:
					buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
					buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
					break;
				case 3:
					buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
					buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
					buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
					break;
			}

			for (int i = 0; i < setFlags.Count; i++)
			{
				var border = new Border();

				// First border: 0 0 5 0
				// Last border: 5 0 0 0
				// Middle border(s): 5 0 5 0
				if (i == 0)
				{
					border.Padding = new Thickness(0.0, 0.0, 5.0, 0.0);
				}
				else if (i == setFlags.Count - 1)
				{
					border.Padding = new Thickness(5.0, 0.0, 0.0, 0.0);
				}
				else
				{
					border.Padding = new Thickness(5.0, 0.0, 5.0, 0.0);
				}

				var button = new Button
				{
					Padding = new Thickness(10.0)
				};
				button.Click += Button_Click;

				buttonGrid.Children.Add(border);
				border.Child = button;

				Grid.SetColumn(border, i);
				switch (setFlags[i])
				{
					case DialogueButton.OK:
						button.Content = "OK";
						break;
					case DialogueButton.Yes:
						button.Content = "Yes";
						break;
					case DialogueButton.No:
						button.Content = "No";
						break;
					case DialogueButton.Cancel:
						button.Content = "Cancel";
						break;
				}
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			// Check which button was clicked
			if (sender is Button button)
			{
				DialogueResult = button.Content switch
				{
					"OK" => DialogueButton.OK,
					"Yes" => DialogueButton.Yes,
					"No" => DialogueButton.No,
					"Cancel" => DialogueButton.Cancel,
					_ => DialogueButton.Cancel,
				};
			}

			Close();
		}

		/// <summary>
		/// Shows a dialogue window with the specified title, description, input field, OK and Cancel buttons.
		/// </summary>
		/// <param name="title">Title of the dialogue window.</param>
		/// <param name="desc">Description of the dialogue window.</param>
		/// <param name="defaultInput">Default input placeholder of the dialogue window.</param>
		/// <returns>Returns input value if OK was clicked, null if Cancel was clicked or the window was closed somehow.</returns>
		public static string? ShowInputDialogue(string title, string desc, string defaultInput = "")
		{
			var dialogue = new DialogueWindow(title, desc, defaultInput, DialogueButton.Default, DialogueType.Input);
			dialogue.ShowDialog();

			if (dialogue.DialogueResult == DialogueButton.None || dialogue.DialogueResult == DialogueButton.Cancel)
			{
				return null;
			}

			return dialogue.InputText;
		}

		/// <summary>
		/// Shows a basic dialogue window with the specified title, description, OK and Cancel buttons.
		/// </summary>
		/// <param name="title">Title of the dialogue window.</param>
		/// <param name="desc">Description of the dialogue window.</param>
		/// <returns>Returns true if OK was clicked, false if Cancel was clicked or the window was closed somehow.</returns>
		public static bool ShowDialogue(string title, string desc)
		{
			var dialogue = new DialogueWindow(title, desc, "", DialogueButton.Default, DialogueType.Normal);
			dialogue.ShowDialog();

			return dialogue.DialogueResult == DialogueButton.OK;
		}

		/// <summary>
		/// Shows a dialogue window with the specified title, description, and some buttons.
		/// </summary>
		/// <param name="title">Title of the dialogue window.</param>
		/// <param name="desc">Description of the dialogue window.</param>
		/// <param name="dialogueButtons">
		///		What buttons will dialogue window make use of.
		/// </param>
		/// <returns>Returns clicked button if any got clicked, returns <see cref="DialogueButton.None"/> if the window was closed somehow.</returns>
		public static DialogueButton ShowDialogue(string title, string desc, DialogueButton dialogueButtons = DialogueButton.Default)
		{
			var dialogue = new DialogueWindow(title, desc, "", dialogueButtons, DialogueType.Normal);
			dialogue.ShowDialog();

			return dialogue.DialogueResult;
		}

		/// <summary>
		/// Shows a message box with the specified title, description and an OK button.
		/// </summary>
		/// <param name="title">Title of the dialogue window.</param>
		/// <param name="desc">Description of the dialogue window.</param>
		public static void ShowMessageBox(string title, string desc)
		{
			ShowDialogue(title, desc, DialogueButton.OK);
		}
	}
}
