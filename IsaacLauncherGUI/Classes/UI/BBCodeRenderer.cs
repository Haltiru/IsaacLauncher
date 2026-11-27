//BBCodeRenderer.cs
using System.Text.RegularExpressions;

namespace IsaacLauncherGUI
{
    // One renderer instance per RichTextBox. Dispose when the owning form closes.
    public sealed class BBCodeRenderer : IDisposable
    {
        private sealed class FormatState
        {
            public FontStyle Style;
            public float Size;
            public Color Color;
            public string Url;
            public bool Monospace;

            public FormatState Copy()
            {
                return (FormatState)MemberwiseClone();
            }
        }

        private sealed class TagState
        {
            public string Name;
            public FormatState Previous;
        }

        private sealed class ListState
        {
            public bool Ordered;
            public int Number;
        }

        private sealed class LinkRange
        {
            public int Start;
            public int Length;
            public string Url;
        }

        private static readonly Regex PlainUrlRegex = new Regex(
            @"https?://[^\s<>\[\]""']+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly List<LinkRange> _links = new List<LinkRange>();
        private RichTextBox _richTextBox;
        private bool _disposed;

        public void Render(RichTextBox richTextBox, string bbcode)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(BBCodeRenderer));
            if (richTextBox == null) throw new ArgumentNullException(nameof(richTextBox));

            Attach(richTextBox);
            _links.Clear();
            bbcode = NormalizeNewlines(bbcode ?? string.Empty).Trim();

            var fontCache = new Dictionary<string, Font>();
            var buffer = new StringBuilder();
            var tagStack = new Stack<TagState>();
            var listStack = new Stack<ListState>();

            float baseSize = richTextBox.Font.SizeInPoints;
            FormatState state = new FormatState
            {
                Style = FontStyle.Regular,
                Size = baseSize,
                Color = richTextBox.ForeColor
            };

            richTextBox.SuspendLayout();
            try
            {
                richTextBox.Clear();
                int index = 0;

                while (index < bbcode.Length)
                {
                    if (bbcode[index] != '[')
                    {
                        buffer.Append(bbcode[index++]);
                        continue;
                    }

                    int end = bbcode.IndexOf(']', index + 1);
                    if (end < 0)
                    {
                        buffer.Append(bbcode[index++]);
                        continue;
                    }

                    string rawTag = bbcode.Substring(index, end - index + 1);
                    string content = bbcode.Substring(index + 1, end - index - 1).Trim();
                    bool closing = content.StartsWith("/", StringComparison.Ordinal);
                    string tagContent = closing ? content.Substring(1) : content;
                    int equalsIndex = tagContent.IndexOf('=');
                    string tag = (equalsIndex >= 0 ? tagContent.Substring(0, equalsIndex) : tagContent).Trim().ToLowerInvariant();
                    string value = equalsIndex >= 0 ? tagContent.Substring(equalsIndex + 1).Trim() : null;

                    // Images are deliberately links: WinForms RichTextBox cannot reliably
                    // embed animated remote images without an additional image control.
                    if (!closing && tag == "img")
                    {
                        int closeIndex = bbcode.IndexOf("[/img]", end + 1, StringComparison.OrdinalIgnoreCase);
                        if (closeIndex >= 0)
                        {
                            FlushBuffer(richTextBox, buffer, state, fontCache);
                            string imageUrl = bbcode.Substring(end + 1, closeIndex - end - 1).Trim();
                            AppendLink(richTextBox, "[View image]", imageUrl, state, fontCache);
                            index = closeIndex + 6;
                            continue;
                        }
                    }

                    // URL without a parameter: [url]https://example.com[/url]
                    if (!closing && tag == "url" && string.IsNullOrWhiteSpace(value))
                    {
                        int closeIndex = bbcode.IndexOf("[/url]", end + 1, StringComparison.OrdinalIgnoreCase);
                        if (closeIndex >= 0)
                        {
                            FlushBuffer(richTextBox, buffer, state, fontCache);
                            string url = bbcode.Substring(end + 1, closeIndex - end - 1).Trim();
                            AppendLink(richTextBox, url, url, state, fontCache);
                            index = closeIndex + 6;
                            continue;
                        }
                    }

                    if (tag == "list" || tag == "olist")
                    {
                        if (closing && listStack.Count > 0)
                            TrimTrailingListWhitespace(buffer);
                        FlushBuffer(richTextBox, buffer, state, fontCache);
                        if (!closing)
                        {
                            listStack.Push(new ListState
                            {
                                Ordered = tag == "olist" || value == "1" ||
                                    string.Equals(value, "decimal", StringComparison.OrdinalIgnoreCase)
                            });
                            index = SkipFormattingWhitespace(bbcode, end + 1);
                        }
                        else if (listStack.Count > 0)
                        {
                            listStack.Pop();
                            index = end + 1;
                            // The source usually contains a newline after [/list].
                            // Preserve it, but do not add another one here.
                        }
                        else
                        {
                            buffer.Append(rawTag);
                            index = end + 1;
                        }
                        continue;
                    }

                    if (!closing && tag == "*" && listStack.Count > 0)
                    {
                        TrimTrailingListWhitespace(buffer);
                        FlushBuffer(richTextBox, buffer, state, fontCache);
                        EnsureNewLine(richTextBox, state, fontCache);

                        ListState list = listStack.Peek();
                        list.Number++;
                        AppendText(richTextBox, list.Ordered ? list.Number + ". " : "• ", state, fontCache);
                        index = SkipFormattingWhitespace(bbcode, end + 1);
                        continue;
                    }

                    if (!closing && (tag == "hr" || tag == "hr/"))
                    {
                        FlushBuffer(richTextBox, buffer, state, fontCache);
                        EnsureNewLine(richTextBox, state, fontCache);
                        AppendText(richTextBox, "────────────────────\n", state, fontCache);
                        index = end + 1;
                        continue;
                    }

                    // RichTextBox has no table layout. Render readable text separators.
                    if (tag == "table" || tag == "tr" || tag == "td" || tag == "th")
                    {
                        FlushBuffer(richTextBox, buffer, state, fontCache);
                        if (tag == "tr" && closing)
                            EnsureNewLine(richTextBox, state, fontCache);
                        else if ((tag == "td" || tag == "th") && closing)
                            AppendText(richTextBox, "  |  ", state, fontCache);
                        index = end + 1;
                        continue;
                    }

                    if (!IsFormatTag(tag))
                    {
                        buffer.Append(rawTag);
                        index = end + 1;
                        continue;
                    }

                    if (!closing)
                    {
                        FlushBuffer(richTextBox, buffer, state, fontCache);
                        tagStack.Push(new TagState { Name = tag, Previous = state.Copy() });
                        ApplyTag(state, tag, value, baseSize);
                        index = end + 1;
                        continue;
                    }

                    // Only close the matching innermost tag. Malformed BBCode stays visible.
                    if (tagStack.Count > 0 && tagStack.Peek().Name == tag)
                    {
                        FlushBuffer(richTextBox, buffer, state, fontCache);
                        state = tagStack.Pop().Previous;
                    }
                    else
                    {
                        buffer.Append(rawTag);
                    }
                    index = end + 1;
                }

                FlushBuffer(richTextBox, buffer, state, fontCache);
                richTextBox.SelectionStart = 0;
                richTextBox.SelectionLength = 0;
                richTextBox.ScrollToCaret();
            }
            finally
            {
                foreach (Font font in fontCache.Values)
                    font.Dispose();
                richTextBox.ResumeLayout();
            }
        }

        private static string NormalizeNewlines(string text)
        {
            return text.Replace("\r\n", "\n").Replace('\r', '\n');
        }

        private static int SkipFormattingWhitespace(string text, int index)
        {
            // Consume only whitespace immediately after structural list tags.
            // Never touch whitespace inside a normal paragraph.
            while (index < text.Length && (text[index] == ' ' || text[index] == '\t'))
                index++;
            if (index < text.Length && text[index] == '\n')
            {
                index++;
                while (index < text.Length && (text[index] == ' ' || text[index] == '\t'))
                    index++;
            }
            return index;
        }

        private static void TrimTrailingListWhitespace(StringBuilder buffer)
        {
            while (buffer.Length > 0 && char.IsWhiteSpace(buffer[buffer.Length - 1]))
                buffer.Length--;
        }

        private static bool IsFormatTag(string tag)
        {
            switch (tag)
            {
                case "b":
                case "i":
                case "u":
                case "s":
                case "strike":
                case "h1":
                case "h2":
                case "h3":
                case "url":
                case "color":
                case "size":
                case "quote":
                case "code":
                case "spoiler":
                    return true;
                default:
                    return false;
            }
        }

        private static void ApplyTag(FormatState state, string tag, string value, float baseSize)
        {
            switch (tag)
            {
                case "b": state.Style |= FontStyle.Bold; break;
                case "i": state.Style |= FontStyle.Italic; break;
                case "u": state.Style |= FontStyle.Underline; break;
                case "s": case "strike": state.Style |= FontStyle.Strikeout; break;
                case "h1": state.Style |= FontStyle.Bold; state.Size = baseSize * 1.6f; break;
                case "h2": state.Style |= FontStyle.Bold; state.Size = baseSize * 1.4f; break;
                case "h3": state.Style |= FontStyle.Bold; state.Size = baseSize * 1.2f; break;
                case "quote": state.Style |= FontStyle.Italic; break;
                case "code": state.Monospace = true; break;
                case "spoiler": state.Style |= FontStyle.Italic; break;
                case "url":
                    string url;
                    if (TryGetSafeUrl(value, out url)) state.Url = url;
                    break;
                case "color":
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            Color color = ColorTranslator.FromHtml(value);
                            if (!color.IsEmpty) state.Color = color;
                        }
                    }
                    catch (Exception) { /* Keep inherited color. */ }
                    break;
                case "size":
                    float size;
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out size) &&
                        !float.IsNaN(size) && !float.IsInfinity(size))
                        state.Size = Math.Max(8f, Math.Min(28f, size));
                    break;
            }
        }

        private static bool TryGetSafeUrl(string input, out string url)
        {
            url = null;
            if (string.IsNullOrWhiteSpace(input)) return false;
            Uri uri;
            if (!Uri.TryCreate(input.Trim(), UriKind.Absolute, out uri)) return false;
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;
            url = uri.AbsoluteUri;
            return true;
        }

        private static Font GetFont(RichTextBox box, FormatState state, Dictionary<string, Font> cache)
        {
            FontFamily family = state.Monospace ? FontFamily.GenericMonospace : box.Font.FontFamily;
            string key = family.Name + "|" + state.Size.ToString(CultureInfo.InvariantCulture) + "|" + (int)state.Style;
            Font font;
            if (!cache.TryGetValue(key, out font))
            {
                font = new Font(family, state.Size, state.Style, GraphicsUnit.Point);
                cache.Add(key, font);
            }
            return font;
        }

        private void FlushBuffer(RichTextBox box, StringBuilder buffer, FormatState state, Dictionary<string, Font> cache)
        {
            if (buffer.Length == 0) return;
            AppendText(box, buffer.ToString(), state, cache);
            buffer.Clear();
        }

        private void AppendText(RichTextBox box, string text, FormatState state, Dictionary<string, Font> cache)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (state.Url != null)
            {
                AppendSegment(box, text, state, cache, state.Url);
                return;
            }

            // Make ordinary bare HTTP(S) links clickable as well.
            int cursor = 0;
            foreach (Match match in PlainUrlRegex.Matches(text))
            {
                if (match.Index > cursor)
                    AppendSegment(box, text.Substring(cursor, match.Index - cursor), state, cache, null);

                string candidate = match.Value.TrimEnd('.', ',', ';', ':', '!', '?', ')');
                string safe;
                if (TryGetSafeUrl(candidate, out safe))
                    AppendSegment(box, candidate, state, cache, safe);
                else
                    AppendSegment(box, candidate, state, cache, null);

                int suffixStart = match.Index + candidate.Length;
                int suffixLength = match.Length - candidate.Length;
                if (suffixLength > 0)
                    AppendSegment(box, text.Substring(suffixStart, suffixLength), state, cache, null);
                cursor = match.Index + match.Length;
            }
            if (cursor < text.Length)
                AppendSegment(box, text.Substring(cursor), state, cache, null);
        }

        private void AppendLink(RichTextBox box, string label, string inputUrl, FormatState state, Dictionary<string, Font> cache)
        {
            string safe;
            if (TryGetSafeUrl(inputUrl, out safe))
                AppendSegment(box, label, state, cache, safe);
            else
                AppendSegment(box, label, state, cache, null);
        }

        private void AppendSegment(RichTextBox box, string text, FormatState state, Dictionary<string, Font> cache, string url)
        {
            if (string.IsNullOrEmpty(text)) return;
            int start = box.TextLength;
            box.SelectionStart = start;
            box.SelectionLength = 0;
            box.SelectionFont = GetFont(box, state, cache);
            box.SelectionColor = url == null ? state.Color : Color.LightSkyBlue;
            box.AppendText(text);

            if (url != null)
            {
                _links.Add(new LinkRange { Start = start, Length = text.Length, Url = url });
                // Add underline only to link text, without changing the active style.
                box.SelectionStart = start;
                box.SelectionLength = text.Length;
                FormatState linkState = state.Copy();
                linkState.Style |= FontStyle.Underline;
                box.SelectionFont = GetFont(box, linkState, cache);
                box.SelectionStart = box.TextLength;
                box.SelectionLength = 0;
            }
        }

        private void EnsureNewLine(RichTextBox box, FormatState state, Dictionary<string, Font> cache)
        {
            if (box.TextLength > 0 && !box.Text.EndsWith("\n", StringComparison.Ordinal))
                AppendSegment(box, "\n", state, cache, null);
        }

        private void Attach(RichTextBox richTextBox)
        {
            if (_richTextBox == richTextBox) return;
            if (_richTextBox != null && !_richTextBox.IsDisposed)
            {
                _richTextBox.MouseMove -= OnMouseMove;
                _richTextBox.MouseClick -= OnMouseClick;
            }
            _richTextBox = richTextBox;
            _richTextBox.MouseMove += OnMouseMove;
            _richTextBox.MouseClick += OnMouseClick;
        }

        private LinkRange FindLink(int index)
        {
            foreach (LinkRange link in _links)
                if (index >= link.Start && index < link.Start + link.Length)
                    return link;
            return null;
        }

        private LinkRange GetLinkAtPoint(RichTextBox box, Point point)
        {
            int index = box.GetCharIndexFromPosition(point);
            LinkRange link = FindLink(index);
            if (link == null) return null;

            // GetCharIndexFromPosition returns the nearest character even in blank space.
            Point start = box.GetPositionFromCharIndex(index);
            Point next = box.GetPositionFromCharIndex(Math.Min(index + 1, box.TextLength));
            int width = next.Y == start.Y ? Math.Max(1, next.X - start.X) : 8;
            int height = Math.Max(1, box.Font.Height + 4);
            if (point.X < start.X || point.X > start.X + width ||
                point.Y < start.Y || point.Y > start.Y + height)
                return null;
            return link;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            RichTextBox box = sender as RichTextBox;
            if (box != null) box.Cursor = GetLinkAtPoint(box, e.Location) == null ? Cursors.IBeam : Cursors.Hand;
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            RichTextBox box = sender as RichTextBox;
            if (box == null) return;
            LinkRange link = GetLinkAtPoint(box, e.Location);
            if (link == null) return;

            try
            {
                Process.Start(new ProcessStartInfo { FileName = link.Url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not open BBCode link: " + ex);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_richTextBox != null && !_richTextBox.IsDisposed)
            {
                _richTextBox.MouseMove -= OnMouseMove;
                _richTextBox.MouseClick -= OnMouseClick;
            }
            _richTextBox = null;
            _links.Clear();
        }
    }
}
