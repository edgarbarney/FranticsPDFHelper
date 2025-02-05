using System;
using System.Windows;

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
			Cancel = 2,

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
			
		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			DialogueResult = DialogueButton.OK;
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			DialogueResult = DialogueButton.None;
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
		/// Shows a basic dialogue window with the specified title, description, input field, OK and Cancel buttons.
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
		/// Shows a dialogue window with the specified title, description, input field, and some buttons.
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
	}
}
