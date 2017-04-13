using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Common
{
    public enum LastSelectionWas
    {
        Right,
        Left,
        None
    }

    public static class TextBoxNameExtender
    {
        #region Keyhandling - ContextMenu handling

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public static LastSelectionWas KeyDown(TextBoxBase textBox, KeyEventArgs e, LastSelectionWas lastSelectionWas, bool addSemicolonToPaste)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.C)
                CopyItem(textBox);

            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.V)
				PasteItem(textBox, addSemicolonToPaste);

            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.X)
                CutItem(textBox);

            else if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                DeleteItem(textBox);

            else if (!IsValidKey(e.KeyCode, e.Modifiers))
                e.SuppressKeyPress = true;

            return MoveSelection(e, textBox, lastSelectionWas);
        }

        private static bool IsValidKey(Keys key, Keys modifiers)
        {
            return (key == Keys.Left ||
                    key == Keys.Right ||
                    key == Keys.Return ||
                    key == Keys.Back ||
                    key == Keys.Delete ||
                    key == Keys.Home ||
                    key == Keys.End ||
                    key == Keys.Tab ||
                    modifiers == Keys.Control ||
                    modifiers == Keys.Shift);
        }

        public  static void CopyItem(TextBoxBase textBox)
        {
            Clipboard.SetData(DataFormats.Text, textBox.SelectedText);
        }

        public static void CutItem(TextBoxBase textBox)
        {
            Clipboard.SetData(DataFormats.Text, textBox.SelectedText);
            var first = textBox.Text.Substring(0, textBox.SelectionStart);
            var last = textBox.Text.Substring(textBox.SelectionStart + textBox.SelectionLength,
                                              textBox.Text.Length - (textBox.SelectionStart + textBox.SelectionLength));
            textBox.Text = first + last;
            textBox.Select(first.Length, 0);
            GetSelected(textBox);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void PasteItem(TextBoxBase textBox, bool addSemicolonToPaste)
        {
			if (!Clipboard.ContainsData(DataFormats.Text))
				return;

            if (textBox.SelectionLength > 0)
            {
                var first = textBox.Text.Substring(0, textBox.SelectionStart);
                var copy = Clipboard.GetData(DataFormats.Text).ToString();
                var last = textBox.Text.Substring(textBox.SelectionStart + textBox.SelectionLength,
                                                  textBox.Text.Length - (textBox.SelectionStart + textBox.SelectionLength));
                textBox.Text = first + copy + last;
                textBox.Select(first.Length, copy.Length);
            }
            else
            {
                var textLength = textBox.SelectionStart == 0 ? textBox.TextLength : textBox.TextLength + 2;
				var text = AddSemicolon(addSemicolonToPaste);
                textBox.Text += text;
                textBox.Select(textLength, Clipboard.GetData(DataFormats.Text).ToString().Length);
            }
        }

        public static void DeleteItem(TextBoxBase textBox)
        {
            var first = textBox.Text.Substring(0, textBox.SelectionStart);
            var last = textBox.Text.Substring(textBox.SelectionStart + textBox.SelectionLength,
                                              textBox.Text.Length - (textBox.SelectionStart + textBox.SelectionLength));
            textBox.Text = first + last;
            textBox.Select(first.Length, 0);
            GetSelected(textBox);
        }

        public static void UpdateContextMenu(TextBoxBase control, ToolStrip contextMenu)
        {
            contextMenu.Items[0].Enabled = false;
            contextMenu.Items[1].Enabled = false;
            contextMenu.Items[2].Enabled = false;
            contextMenu.Items[4].Enabled = false;

            if (String.IsNullOrEmpty(control.SelectedText))
            {
                contextMenu.Items[0].Enabled = true;
                contextMenu.Items[1].Enabled = true;
                contextMenu.Items[4].Enabled = true;
            }
            if (Clipboard.ContainsText())
                contextMenu.Items[2].Enabled = true;
        }

        public static string AddSemicolon(bool addSemicolonToPaste)
        {
            var clipboardCopy = Clipboard.GetData(DataFormats.Text).ToString();
			if (!addSemicolonToPaste) return clipboardCopy;
            if (clipboardCopy.Substring(0, 2) != "; ")
                clipboardCopy = clipboardCopy.Insert(0, "; ");

            return clipboardCopy;
        }
        #endregion

        #region SelectionHandling

        public static LastSelectionWas MoveSelection(KeyEventArgs e, TextBoxBase textBox, LastSelectionWas lastSelectionWas)
        {
            lastSelectionWas = e.Modifiers == Keys.Shift ? 
                MoveSelectionWithShift(e, textBox, lastSelectionWas) : 
                MoveSelectionWithoutShift(e, textBox, lastSelectionWas);

            return lastSelectionWas;
        }

        public static LastSelectionWas MoveSelectionWithoutShift(KeyEventArgs e, TextBoxBase textBox, LastSelectionWas lastSelectionWas)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    // cant move left
                    if (textBox.SelectionStart - 2 <= 0)
                        textBox.Select(0, 0);

                    else
                        textBox.Select(textBox.SelectionStart - 2, 0);

                    GetSelected(textBox);
                    break;

                case Keys.Right:
                    // cant move right
                    if (textBox.SelectionStart + textBox.SelectionLength >= textBox.TextLength)
                        textBox.Select(textBox.Text.Length, 0);

                    else
                    {
                        textBox.Select(textBox.SelectionStart + textBox.SelectionLength + 2, 0);
                        GetSelected(textBox);
                    }
                    break;

                case Keys.End:
                    textBox.Select(textBox.Text.Length, 0);
                    break;

                case Keys.Home:
                    textBox.Select(0, 0);
                    GetSelected(textBox);
                    break;
            }
            return lastSelectionWas;
        }

        public static LastSelectionWas MoveSelectionWithShift(KeyEventArgs e, TextBoxBase textBox, LastSelectionWas lastSelectionWas)
        {
            var selectedTextSplit = textBox.SelectedText.Split(new[] { "; " },
                                                                   StringSplitOptions.None);
            if (selectedTextSplit.Count() <= 1)
                lastSelectionWas = LastSelectionWas.None;

            switch (e.KeyCode)
            {
                case Keys.Right:
                    if (lastSelectionWas == LastSelectionWas.Left)
                        textBox.Select(textBox.SelectionStart + selectedTextSplit[0].Length + 2,
                                       selectedTextSplit.Sum(s => s.Length) + 2 * (selectedTextSplit.Count() - 1) - 2
                                       - selectedTextSplit[0].Length);

                    else
                    {
                        // end of textbox
                        if (textBox.SelectionStart + textBox.SelectionLength >= textBox.TextLength)
                            textBox.Select(textBox.Text.Length, 0);

                        else
                        {
                            textBox.Select(textBox.SelectionStart, textBox.SelectionLength + 2);
                            GetSelected(textBox);
                        }
                        lastSelectionWas = LastSelectionWas.Right;
                    }
                    break;

                case Keys.Left:
                    if (lastSelectionWas == LastSelectionWas.Right)
                    {
                        textBox.Select(textBox.SelectionStart,
                                       selectedTextSplit.Sum(s => s.Length + 2) - 2 -
                                       selectedTextSplit.Last().Length - 2);
                    }
                    else
                    {
                        // start of textbox
                        if (textBox.SelectionStart == 0)
                            textBox.Select(textBox.SelectionStart, selectedTextSplit[0].Length);

                        else
                        {
                            textBox.Select(textBox.SelectionStart - 2, textBox.SelectionLength + 2);
                            GetSelected(textBox);
                        }
                        lastSelectionWas = LastSelectionWas.Left;
                    }
                    break;

                case Keys.End:
                    // selection already is at end
                    if (textBox.SelectionStart + textBox.SelectionLength == textBox.Text.Length)
                        textBox.Select(textBox.Text.Length, 0);
                    else
                    {
                        textBox.Select(textBox.SelectionStart, textBox.Text.Length);
                        lastSelectionWas = LastSelectionWas.Right;
                    }
                    break;

                case Keys.Home:
                    // selection already is at start
                    if (textBox.SelectionStart == 0)
                    {
                        textBox.Select(0, 0);
                        GetSelected(textBox);
                    }
                    else
                    {
                        textBox.Select(0, textBox.SelectionStart + selectedTextSplit.Sum(s => s.Length + 2) - 1);
                        lastSelectionWas = LastSelectionWas.Left;
                    }
                    break;
            }

            return lastSelectionWas;
        }

		public static IList<int> SelectedIndexes(TextBoxBase textBox)
		{
			var selectStart = textBox.SelectionStart;
			var selectEnd = selectStart + textBox.SelectionLength;
			var selectedIndexes = new List<int>();
			var currentIndex = 0;

			for (int i = 0; i < textBox.TextLength; i++)
			{
				if (textBox.Text[i] == ';')
					currentIndex++;

				if(i >= selectStart && i < selectEnd && !selectedIndexes.Contains(currentIndex))
					selectedIndexes.Add(currentIndex);

				if (i >= selectEnd)
					break;
			}

			return selectedIndexes;
		}

        public static void GetSelected(TextBoxBase textBox)
        {
            var index = textBox.SelectionStart;
            var participantsFromText = textBox.Text.Split(new[] { "; " },
                                                          StringSplitOptions.None);
            var selectedTextList = textBox.SelectedText.Split(new[] { "; " },
                                                              StringSplitOptions.None);

            int counter = 0, selectedLength = 0, start = 0;

            for (var i = 0; i < participantsFromText.Length; i++)
            {
                if (index < counter) break;
                if (index == textBox.Text.Length) return;

                start = participantsFromText[i].Length;
                selectedLength = start;

                if (i == 0) counter += start + 1;
                else counter += start + 2;

                if (selectedTextList.Count() <= 1) continue;
                for (var j = 1; j < selectedTextList.Count(); j++)
                    selectedLength += participantsFromText[i + j].Length + 2;
            }
            var selectionStart = counter - start - 1 > 0 ? counter - start - 1 : 0;
            textBox.Select(selectionStart, selectedLength);
            textBox.ScrollToCaret();
        }
        #endregion
    }
}
