using System;
using System.Windows;
using System.Windows.Controls;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers
{
    /// <summary>
    /// Translates texts in textblocks (and FrameworkContentElemnet that presents text in TextBlocks by default)
    /// Inherits down through the Visualtree
    /// </summary>
    /// <remarks>
    /// Usage: StackPanel VisualTreeTranslator.Translate="true" will translate everything within the element
    /// Because it has to look at all the elements, if you have a lot of VisualElements and know that you dont need
    /// translation, you can turn off the translation for that region by setting it to false.
    /// Default is false
    /// Created by: henrika
    /// Created date: 2008-12-08
    /// </remarks>
    public static class VisualTreeTranslator
    {
        public static bool GetTranslate(DependencyObject obj)
        {
            return (bool)obj.GetValue(TranslateProperty);
        }
        public static void SetTranslate(DependencyObject obj, bool value)
        {
            obj.SetValue(TranslateProperty, value);
        }
        public static readonly DependencyProperty TranslateProperty =
        DependencyProperty.RegisterAttached("Translate",
        typeof(bool),
        typeof(VisualTreeTranslator),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, SetText));

        private static void SetText(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBlock textBlockToTranslate = d as TextBlock;
            if (textBlockToTranslate != null && (bool)e.NewValue)
            {
                textBlockToTranslate.Loaded += TextHelper_Loaded;
            }
        }
        static void TextHelper_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock target = sender as TextBlock;
            if (target != null)
            {
                string translatedText = LanguageResourceHelper.Translate(target.Text);
                if (!String.IsNullOrEmpty(translatedText)) target.Text = LanguageResourceHelper.Translate(target.Text);
                target.Loaded -= TextHelper_Loaded;
            }
        }
    }
}

