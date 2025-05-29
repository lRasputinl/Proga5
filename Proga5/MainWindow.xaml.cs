using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace TextEditor
{
    public partial class MainWindow : Window
    {
        private string currentFilePath = null;

        private bool isBold = false;
        private bool isItalic = false;
        private bool isUnderline = false;
        private bool isStrikethrough = false;
        private SolidColorBrush currentColor = Brushes.Black;

        public MainWindow()
        {
            InitializeComponent();
            editor.TextChanged += Editor_TextChanged;
            SetActiveColorButton(BlackBtn);
        }

        // Форматирование
        private void Bold_Click(object sender, RoutedEventArgs e)
        {
            isBold = !isBold;
            BoldBtn.Background = isBold ? Brushes.Orange : Brushes.Transparent;
        }

        private void Italic_Click(object sender, RoutedEventArgs e)
        {
            isItalic = !isItalic;
            ItalicBtn.Background = isItalic ? Brushes.Orange : Brushes.Transparent;
        }

        private void Underline_Click(object sender, RoutedEventArgs e)
        {
            isUnderline = !isUnderline;
            UnderlineBtn.Background = isUnderline ? Brushes.Orange : Brushes.Transparent;
        }

        private void Strikethrough_Click(object sender, RoutedEventArgs e)
        {
            isStrikethrough = !isStrikethrough;
            StrikethroughBtn.Background = isStrikethrough ? Brushes.Orange : Brushes.Transparent;
        }

        private void ToggleStyle(DependencyProperty prop, object onValue, object offValue)
        {
            TextSelection selection = editor.Selection;
            if (!selection.IsEmpty)
            {
                object current = selection.GetPropertyValue(prop);
                object newValue = current.Equals(onValue) ? offValue : onValue;
                selection.ApplyPropertyValue(prop, newValue);
            }
        }

        private void ToggleDecoration(TextDecorationCollection decoration)
        {
            TextSelection selection = editor.Selection;
            if (!selection.IsEmpty)
            {
                var range = new TextRange(selection.Start, selection.End);
                var currentDecorations = range.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection;

                if (currentDecorations != null && currentDecorations.Contains(decoration[0]))
                    range.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                else
                    range.ApplyPropertyValue(Inline.TextDecorationsProperty, decoration);
            }
        }

        // Цвет
        private void SetActiveColorButton(Button active)
        {
            Button[] buttons = { RedBtn, BlueBtn, GreenBtn, BlackBtn };
            foreach (var btn in buttons)
            {
                btn.BorderThickness = new Thickness(1);
                btn.BorderBrush = Brushes.Gray;
            }

            active.BorderThickness = new Thickness(3);
            active.BorderBrush = Brushes.Orange;
        }


        private void Red_Click(object sender, RoutedEventArgs e)
        {
            currentColor = Brushes.Red;
            SetActiveColorButton(RedBtn);
        }

        private void Blue_Click(object sender, RoutedEventArgs e)
        {
            currentColor = Brushes.Blue;
            SetActiveColorButton(BlueBtn);
        }

        private void Green_Click(object sender, RoutedEventArgs e)
        {
            currentColor = Brushes.Green;
            SetActiveColorButton(GreenBtn);
        }

        private void Black_Click(object sender, RoutedEventArgs e)
        {
            currentColor = Brushes.Black;
            SetActiveColorButton(BlackBtn);
        }

        // Файлы
        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            editor.Document.Blocks.Clear();
            currentFilePath = null;
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog { Filter = "RTF файлы (*.rtf)|*.rtf|Все файлы (*.*)|*.*" };
                if (dlg.ShowDialog() == true)
                {
                    TextRange range = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);
                    using (FileStream fs = new FileStream(dlg.FileName, FileMode.Open))
                    {
                        range.Load(fs, DataFormats.Rtf);
                        currentFilePath = dlg.FileName;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(currentFilePath))
                {
                    SaveFileDialog dlg = new SaveFileDialog { Filter = "RTF файлы (*.rtf)|*.rtf" };
                    if (dlg.ShowDialog() == true)
                        currentFilePath = dlg.FileName;
                    else
                        return;
                }

                TextRange range = new TextRange(editor.Document.ContentStart, editor.Document.ContentEnd);
                using (FileStream fs = new FileStream(currentFilePath, FileMode.Create))
                {
                    range.Save(fs, DataFormats.Rtf);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Горячие клавиши
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.N) NewFile_Click(null, null);
                if (e.Key == Key.O) OpenFile_Click(null, null);
                if (e.Key == Key.S) SaveFile_Click(null, null);
            }
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            var caretPos = editor.CaretPosition;
            var prev = caretPos.GetPositionAtOffset(-1, LogicalDirection.Backward);
            if (prev != null)
            {
                TextRange range = new TextRange(prev, caretPos);
                range.ApplyPropertyValue(TextElement.FontWeightProperty, isBold ? FontWeights.Bold : FontWeights.Normal);
                range.ApplyPropertyValue(TextElement.FontStyleProperty, isItalic ? FontStyles.Italic : FontStyles.Normal);

                if (isUnderline || isStrikethrough)
                {
                    var decorations = new TextDecorationCollection();
                    if (isUnderline) decorations.Add(TextDecorations.Underline[0]);
                    if (isStrikethrough) decorations.Add(TextDecorations.Strikethrough[0]);
                    range.ApplyPropertyValue(Inline.TextDecorationsProperty, decorations);
                }
                else
                {
                    range.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                }

                range.ApplyPropertyValue(TextElement.ForegroundProperty, currentColor);
            }
        }
        private void ClearFormatting_Click(object sender, RoutedEventArgs e)
        {
            isBold = isItalic = isUnderline = isStrikethrough = false;
            currentColor = Brushes.Black;

            BoldBtn.Background = ItalicBtn.Background = UnderlineBtn.Background = StrikethroughBtn.Background = Brushes.Transparent;

            SetActiveColorButton(BlackBtn);
        }
    }
}
