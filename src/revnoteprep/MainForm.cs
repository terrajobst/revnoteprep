using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using ReviewNotesPreparationTool.Properties;

namespace ReviewNotesPreparationTool
{
    internal sealed partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            Font = SystemFonts.MessageBoxFont;

            var settings = GetDefaultSettings();
            SetSettings(settings);
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.FileName = outputFileTextBox.Text;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                    outputFileTextBox.Text = dlg.FileName;
            }
        }

        private async void generateButton_Click(object sender, EventArgs e)
        {
            var settings = GetSettings();

            if (!ConfirmOverwrite(settings.OutputFileName))
                return;

            try
            {
                await ReviewWriter.WriteReviewAsync(settings);
            }
            catch (Exception ex)
            {
                var text = string.Format(Resources.MainForm_ErrorWritingReviewNotes_Text, ex.Message);
                MessageBox.Show(text, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Process.Start(settings.OutputFileName);
            Close();
        }

        private static ReviewSettings GetDefaultSettings()
        {
            var fileName = string.Format(Resources.MainForm_GetDefaultSettings_FileNameFormat, DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

            var settings = new ReviewSettings();
            settings.Organization = @"dotnet";
            settings.Repository = @"corefx";
            settings.Label = @"needs-api-review";
            settings.OutputFileName = path;
            return settings;
        }

        private ReviewSettings GetSettings()
        {
            var settings = new ReviewSettings();
            settings.Organization = organizationTextBox.Text;
            settings.Repository = repositoryTextBox.Text;
            settings.Label = labelTextBox.Text;
            settings.OutputFileName = outputFileTextBox.Text;
            return settings;
        }

        private void SetSettings(ReviewSettings settings)
        {
            organizationTextBox.Text = settings.Organization;
            repositoryTextBox.Text = settings.Repository;
            labelTextBox.Text = settings.Label;
            outputFileTextBox.Text = settings.OutputFileName;
        }

        private static bool ConfirmOverwrite(string outputFileName)
        {
            if (!File.Exists(outputFileName))
                return true;

            var text = string.Format(Resources.MainForm_ConfirmOverwrite_Text, outputFileName, Environment.NewLine);
            var title = Resources.MainForm_ConfirmOverwrite_Title;
            var button = MessageBox.Show(text, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            return button == DialogResult.Yes;
        }
    }
}