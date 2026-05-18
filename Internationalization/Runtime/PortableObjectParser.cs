using System.Drawing;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine.UIElements;

namespace Xuan25.Internationalization
{ 
    enum ParseState
    {
        Error,
        None,
        MsgID,
        MsgStr,
    }

    enum ParseLineType
    {
        Empty,
        Comments,
        MsgID,
        MsgIDMultiline,
        MsgStr,
        MsgStrContinue,
    }

    static class PortableObjectParser
    {

        private static void EvaluateNextLine(string line, ParseState currentState, out ParseState nextState, out ParseLineType lineType)
        {
            string trimmedLine = line.Trim();

            switch (currentState)
            {
                case ParseState.Error:
                    break;
                case ParseState.None:
                    if (trimmedLine == "")
                    {
                        lineType = ParseLineType.Empty;
                        nextState = currentState;
                        return;
                    }
                    if (trimmedLine.StartsWith("#"))
                    {
                        lineType = ParseLineType.Comments;
                        nextState = currentState;
                        return;
                    }
                    if (trimmedLine.StartsWith("msgid"))
                    {
                        lineType = ParseLineType.MsgID;
                        nextState = ParseState.MsgID;
                        return;
                    }
                    break;
                case ParseState.MsgID:
                    if (trimmedLine == "")
                    {
                        lineType = ParseLineType.Empty;
                        nextState = currentState;
                        return;
                    }
                    if (trimmedLine.StartsWith("#"))
                    {
                        lineType = ParseLineType.Comments;
                        nextState = currentState;
                        return;
                    }
                    if (trimmedLine.StartsWith("\""))
                    {
                        lineType = ParseLineType.MsgIDMultiline;
                        nextState = currentState;
                        return;
                    }
                    if (trimmedLine.StartsWith("msgstr"))
                    {
                        lineType = ParseLineType.MsgStr;
                        nextState = ParseState.MsgStr;
                        return;
                    }
                    break;
                case ParseState.MsgStr:
                    if (trimmedLine == "")
                    {
                        lineType = ParseLineType.Empty;
                        nextState = currentState;
                        return;
                    }
                    if (trimmedLine.StartsWith("#"))
                    {
                        lineType = ParseLineType.Comments;
                        nextState = currentState;
                        return;
                    }
                    if (trimmedLine.StartsWith("\""))
                    {
                        lineType = ParseLineType.MsgStrContinue;
                        nextState = currentState;
                        return;
                    }
                    if (trimmedLine.StartsWith("msgid"))
                    {
                        lineType = ParseLineType.MsgID;
                        nextState = ParseState.MsgID;
                        return;
                    }
                    break;
            }
            lineType = ParseLineType.Empty;
            nextState = ParseState.Error;
            return;
        }

        private static void FlushCurrentEntry(VRC.SDK3.Data.DataDictionary translations, System.Text.StringBuilder keyBuffer, System.Text.StringBuilder valueBuffer)
        {
            string key = keyBuffer.ToString();
            string value = valueBuffer.ToString();

            translations[key] = value;

            keyBuffer.Clear();
            valueBuffer.Clear();
        }

        public static int ParseTranslations(string content, VRC.SDK3.Data.DataDictionary translations)
        {
            System.Text.StringBuilder keyBuffer = new System.Text.StringBuilder();
            System.Text.StringBuilder valueBuffer = new System.Text.StringBuilder();

            ParseState state = ParseState.None;
            int lineNumber = 0;
            
            // NOTE: System.IO.StringReader is not exposed in Udon, so we have to split the content manually.
            // System.IO.StringReader reader = new System.IO.StringReader(content);
            // while (reader.Peek() != -1)
            // {
            //     string line = reader.ReadLine();
            //     lineNumber++;
            
            string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
            foreach (string line in lines)
            {
                lineNumber++;

                EvaluateNextLine(line, state, out ParseState nextState, out ParseLineType lineType);

                if (nextState == ParseState.Error)
                    return lineNumber;

                if (state == ParseState.MsgStr && nextState != ParseState.MsgStr)
                {
                    FlushCurrentEntry(translations, keyBuffer, valueBuffer);
                }

                switch (lineType)
                {
                    case ParseLineType.Empty:
                    case ParseLineType.Comments:
                        break;
                    case ParseLineType.MsgID:
                        keyBuffer.Clear();
                        string unscapedKey = Regex.Unescape(line.Substring(5).Trim().Trim('"'));
                        keyBuffer.Append(unscapedKey);
                        break;
                    case ParseLineType.MsgIDMultiline:
                        string unscapedKeyContinue = Regex.Unescape(line.Trim().Trim('"'));
                        keyBuffer.Append(unscapedKeyContinue);
                        break;
                    case ParseLineType.MsgStr:
                        valueBuffer.Clear();
                        string unscapedValue = Regex.Unescape(line.Substring(6).Trim().Trim('"'));
                        valueBuffer.Append(unscapedValue);
                        break;
                    case ParseLineType.MsgStrContinue:
                        string unscapedValueContinue = Regex.Unescape(line.Trim().Trim('"'));
                        valueBuffer.Append(unscapedValueContinue);
                        break;
                }

                state = nextState;
            }
            
            if (state == ParseState.MsgStr)
            {
                FlushCurrentEntry(translations, keyBuffer, valueBuffer);
            }

            return 0;
        }

        public static int ParseHeaders(string content, VRC.SDK3.Data.DataDictionary headers)
        {
            string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
            foreach (string line in lines)
            {
                if (line.Trim() == "") continue;

                int separatorIndex = line.IndexOf(':');
                if (separatorIndex == -1) continue;

                string key = line.Substring(0, separatorIndex).Trim();
                string value = line.Substring(separatorIndex + 1).Trim();

                headers[key] = value;
            }

            return 0;
        }
    }
}
