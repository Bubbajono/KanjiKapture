using KanjiKapture.Models;
using KanjiKapture.Utilities;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace KanjiKapture
{
    public partial class MainPage : ContentPage
    {
        JObject? CompoundJson;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not Entry entry) return;

            if (string.IsNullOrEmpty(entry.Text))
            {
                entry.BackgroundColor = Colors.Transparent;
                return;
            }

            if (TextUtility.ContainsNonKanji(entry.Text))
            {
                entry.BackgroundColor = Colors.Red;
            }
            else
            {
                entry.BackgroundColor = Colors.LightGreen;
            }
        }

        private void OnEntryCompleted(object sender, EventArgs e)
        {
            if (sender is not Entry entry) return;

            if (string.IsNullOrEmpty(entry.Text))
            {
                KanjiSearchNotificationLabel.Text = string.Empty;
                return;
            }

            if (TextUtility.ContainsNonKanji(entry.Text))
            {
                KanjiSearchNotificationLabel.Text = (entry.Text.Length > 1) 
                    ? "These are not valid characters" 
                    : "This is not a valid character";
                entry.BackgroundColor = Colors.Red;
            }
            else
            {
                KanjiSearchNotificationLabel.Text = (entry.Text.Length > 1)
                    ? "These are valid characters"
                    : "This is a valid character";
                entry.BackgroundColor = Colors.LightGreen;
            }
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            var search = KanjiSearchEntry.Text;

            if (search.Length == 0) return;

            if (search.Length > 1)
            {
                await DisplayAlert("No Results", "Unable to search for compounds with compounds - please submit a single character", "OK");
                return;
            }

            if (TextUtility.ContainsNonKanji(search)) return;

            // Disable the button to prevent spamming
            SearchBtn.IsEnabled = false;

            CompoundJson = await TextUtility.GetKanjiCompounds(search);
            var compounds = TextUtility.GetCompoundList(search, CompoundJson);

            SemanticScreenReader.Announce(SearchBtn.Text);
            
            if (compounds is null || compounds.Length == 0)
            {
                await DisplayAlert("No Results", "No compounds found for the entered Kanji.", "OK");
                SearchBtn.IsEnabled = true;
                return;
            }
            else
            {
                await DisplayAlert($"{compounds.Length} results found", "", "OK");

                //Refresh workaround
                CompoundsListView.ItemsSource = null;
                CompoundsListView.ItemsSource = compounds;
                CompoundsListView.BindingContext = this;

                SearchBtn.IsEnabled = true;
            }
        }

        private void OnCompoundSelected(object sender, SelectedItemChangedEventArgs e)
        {            
            if (e.SelectedItem != null)
            {
                // Handle the selected item
                var selectedItem = e.SelectedItem.ToString();

                if (selectedItem is null) return;

                var compoundDetail = TextUtility.GetCompoundDetail(CompoundJson, selectedItem);

                if (compoundDetail != null)
                {
                    // Update UI elements and control visibility based on data
                    SlugLabel.Text = $"Slug: {compoundDetail.Slug}";
                    SlugLabel.IsVisible = !string.IsNullOrEmpty(compoundDetail.Slug);

                    IsCommonLabel.Text = $"Common: {compoundDetail.IsCommon}";
                    IsCommonLabel.IsVisible = true; // Always visible, adjust if needed

                    JlptLevelsLabel.Text = compoundDetail.JlptLevels.Count != 0
                        ? $"JLPT Levels: {string.Join(", ", compoundDetail.JlptLevels)}"
                        : string.Empty;
                    JlptLevelsLabel.IsVisible = !string.IsNullOrEmpty(JlptLevelsLabel.Text);

                    JapaneseWordsLabel.Text = compoundDetail.Japanese.Count != 0
                        ? $"Japanese Words: {string.Join(", ", compoundDetail.Japanese.Select(j => j.Word))}"
                        : string.Empty;
                    JapaneseWordsLabel.IsVisible = !string.IsNullOrEmpty(JapaneseWordsLabel.Text);

                    SensesLabel.Text = compoundDetail.Senses.Count != 0
                        ? $"Senses: {string.Join("; ", compoundDetail.Senses.Select(s => string.Join(", ", s.EnglishDefinitions)))}"
                        : string.Empty;
                    SensesLabel.IsVisible = !string.IsNullOrEmpty(SensesLabel.Text);

                    AttributionLabel.Text = compoundDetail.Attribution != null
                        ? $"Attribution: JMDict={compoundDetail.Attribution.Jmdict}, JMnedict={compoundDetail.Attribution.Jmnedict}, DBpedia={compoundDetail.Attribution.Dbpedia}"
                        : string.Empty;
                    AttributionLabel.IsVisible = !string.IsNullOrEmpty(AttributionLabel.Text);

                    // Show the detail view
                    CompoundDetailView.IsVisible = true;
                    CompoundDetailViewBorder.IsVisible = true;
                }
                else
                {
                    // Hide the detail view if no data is available
                    CompoundDetailView.IsVisible = false;
                    CompoundDetailViewBorder.IsVisible = false;
                }
            }
        }
    }

}
