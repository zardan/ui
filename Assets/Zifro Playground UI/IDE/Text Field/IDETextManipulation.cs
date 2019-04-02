using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace PM
{
	public class IDETextManipulation
	{
		public static char MyValidate(
			char c,
			int charIndex,
			bool isPasting,
			List<IndentLevel> toAddLevels,
			string currentText,
			IDETextField theTextField)
		{
			if (isPasting)
			{
				return c;
			}

			if (c == '\n')
			{
				toAddLevels.Clear();
				toAddLevels.Add(new IndentLevel(charIndex, IDEParser.GetIndentLevel(charIndex, currentText)));
				theTextField.SetNewCaretPos(toAddLevels[0].GetNewCaretPos());

				return '\n';
			}

			return c;
		}

		public static int CountCodeLines(List<string> textLines)
		{
			int count = 0;
			var regex = new Regex(@"^#|^\s*$|^\s*#");
			foreach (string line in textLines)
			{
				line.Trim();
				if (regex.Match(line).Success || line == "")
				{
				}
				else
				{
					count++;
				}
			}

			return count;
		}

		public static bool ValidateText(string fullText, int maxLines, int maxPerLine)
		{
			List<string> preCodeTextLines = IDEParser.ParseIntoLines(PMWrapper.preCode);
			int preCodeLineCount = CountCodeLines(preCodeTextLines);

			List<string> mainCodeTextLines = IDEParser.ParseIntoLines(fullText);
			int mainCodeLineCount = CountCodeLines(mainCodeTextLines);

			Progress.instance.levelData[PMWrapper.currentLevel.id].codeLineCount = mainCodeLineCount + preCodeLineCount;
			UISingleton.instance.rowsLimit.UpdateRowsLeft(mainCodeLineCount, maxLines);

			if (mainCodeLineCount > maxLines)
			{
				// Too many rows
				UISingleton.instance.rowsLimit.redness = 1;
				return false;
			}

			foreach (string t in mainCodeTextLines)
			{
				if (LineSize(t) > maxPerLine)
				{
					return false;
				}
			}

			return true;
		}

		private static int LineSize(string lineText)
		{
			int sizeCounter = 0;

			foreach (char c in lineText)
			{
				sizeCounter++;
				if (c == '\t')
				{
					sizeCounter += 4 - sizeCounter % 4;
				}
			}

			return sizeCounter;
		}
	}

	public sealed class IndentLevel
	{
		public readonly int caretPos;
		public readonly string indentText;
		public readonly int indentLevel;

		public IndentLevel(int caretPos, int indentLevel)
		{
			this.indentLevel = indentLevel;
			this.caretPos = caretPos + 1;
			indentText = new string('\t', indentLevel);
		}

		public int GetNewCaretPos()
		{
			return indentLevel + caretPos;
		}
	}
}