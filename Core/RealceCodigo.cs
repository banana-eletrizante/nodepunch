using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace NodePunch.Core
{
    internal static class RealceCodigo
    {
        internal static void Aplicar(RichTextBox editor, string extensao)
        {
            editor.SelectAll(); editor.SelectionColor = Tema.Texto;
            editor.Select(0, 0);
            // Long files remain available as plain text; coloring thousands of ranges blocks the UI.
            if (editor.TextLength > 40000 || !(extensao is ".js" or ".json" or ".sql" or ".env" or ".example" or "")) return;
            string comentarios = extensao is ".env" or ".example" or "" ? @"\#[^\r\n]*" : extensao == ".sql" ? @"--[^\r\n]*" : @"//[^\r\n]*|/\*[\s\S]*?\*/";
            string padrao = "(?<string>\"(?:\\\\.|[^\"\\\\])*\"|'(?:\\\\.|[^'\\\\])*'|`(?:\\\\.|[^`\\\\])*`)|(?<comment>" + comentarios + @")|(?<keyword>\b(?:const|let|var|async|await|return|if|else|try|catch|throw|new|class|extends|require|module|exports|true|false|null|SELECT|FROM|WHERE|CREATE|TABLE|INSERT|INTO|VALUES|UPDATE|DELETE)\b)|(?<number>\b\d+(?:\.\d+)?\b)";
            try
            {
                var tokens = Regex.Matches(editor.Text, padrao, RegexOptions.None, TimeSpan.FromMilliseconds(100));
                foreach (Match token in tokens)
                {
                    editor.Select(token.Index, token.Length);
                    editor.SelectionColor = token.Groups["comment"].Success ? Tema.Secundario : token.Groups["string"].Success ? Color.FromArgb(118, 222, 172) : token.Groups["number"].Success ? Color.FromArgb(255, 184, 117) : Color.FromArgb(197, 164, 255);
                }
            }
            catch (RegexMatchTimeoutException) { editor.SelectAll(); editor.SelectionColor = Tema.Texto; }
            finally { editor.Select(0, 0); }
        }
    }
}
