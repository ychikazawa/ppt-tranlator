using Microsoft.Win32;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Interop.PowerPoint;
using System.IO;


namespace CS48_WPF_TranslatePPT
{
    public partial class MainWindow : Window
    {
        string filePath;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SelectPptButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select a PowerPoint File",
                Filter = "PowerPoint files (*.ppt;*.pptx)|*.ppt;*.pptx|All files (*.*)|*.*",
                InitialDirectory = @"C:\"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
                SelectPPTButton.Content = filePath;
            }
        }

        private async void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            string sourceLanguage = SourceLanguageTextBox.Text;
            string targetLanguage = TargetLanguageTextBox.Text;
            string apiKey = ApiKeyTextBox.Text;
            string region = RegionTextBox.Text;

            if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(sourceLanguage) ||
                string.IsNullOrWhiteSpace(targetLanguage) || string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(region))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            try
            {
                ProgressBar.Value = 0;
                NotificationLabel.Content = "Now translating...";

                var progress = new Progress<int>(value => ProgressBar.Value = value);
                var translatedFilePath = await Task.Run(()=> TranslatePpt(filePath, sourceLanguage, targetLanguage, apiKey, region, progress));

                ProgressBar.Value = 100;
                NotificationLabel.Content = $"Completed translation: {translatedFilePath}";
            }
            catch (Exception ex)
            {
                NotificationLabel.Content = "Error occured.";
                MessageBox.Show($"Error occured: {ex.Message}");
            }
        }

        private string TranslatePpt(string filePath, string sourceLanguage, string targetLanguage, string apiKey, string region, IProgress<int> progress)
        {
            PowerPoint.Application pptApplication = new PowerPoint.Application();
            PowerPoint.Presentation presentation = pptApplication.Presentations.Open(filePath);

            int totalShapes = presentation.Slides.Cast<Slide>().Sum(slide => slide.Shapes.Count);
            int processedShapes = 0;

            foreach (PowerPoint.Slide slide in presentation.Slides)
            {
                foreach (PowerPoint.Shape shape in slide.Shapes)
                {
                    if (shape.HasTextFrame == MsoTriState.msoTrue)
                    {
                        string originalText = shape.TextFrame.TextRange.Text;
                        string translatedText = AzureTextTranslator.TranslateText(apiKey, region, originalText, sourceLanguage, targetLanguage);
                        shape.TextFrame.TextRange.Text = translatedText;
                    }

                    processedShapes++;
                    int progressValue = (int)((double)processedShapes / totalShapes * 100);
                    progress.Report(progressValue);
                }
            }

            string savedFilePath = Path.Combine(Path.GetDirectoryName(filePath), "translated_presentation.pptx");
            presentation.SaveAs(savedFilePath);
            
            try
            {
                presentation.Close();
                pptApplication.Quit();
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Failed to close PPT file: {ex.Message}");
            }

            return savedFilePath;
        }
    }
}