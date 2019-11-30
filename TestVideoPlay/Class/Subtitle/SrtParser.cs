﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace TestVideoPlay {
    class SrtParser {
        // Properties -----------------------------------------------------------------------

        private readonly string[] _delimiters = { "-->", "- >", "->" };


        // Constructors --------------------------------------------------------------------

        public SrtParser() { }


        // Methods -------------------------------------------------------------------------

        private List<SubtitleItem> ParseStream(Stream srtStream, Encoding encoding) {
            srtStream.Position = 0;
            var reader = new StreamReader(srtStream, encoding, true);
            var items = new List<SubtitleItem>();
            var srtSubParts = GetSrtSubTitleParts(reader).ToList();
            foreach (var srtSubPart in srtSubParts) {
                var lines = srtSubPart.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Select(s => s.Trim()).Where(l => !String.IsNullOrEmpty(l)).ToList();
                var item = new SubtitleItem();
                foreach (var line in lines) {
                    if (item.StartTime == 0 && item.EndTime == 0) {
                        if (TryParseTimecodeLine(line, out double startTc, out double endTc)) {
                            item.StartTime = startTc;
                            item.EndTime = endTc;
                        }
                    } else {
                        item.Lines.Add(GetPanel(line));
                    }
                }
                if ((item.StartTime != 0 || item.EndTime != 0) && item.Lines.Any()) {
                    // parsing succeeded
                    items.Add(item);
                }
            }
            srtStream.Close();
            return items;
        }

        public List<SubtitleItem> ParseFile(string file, Encoding encoding) {
            var srtStream = File.Open(file, FileMode.Open);
            return ParseStream(srtStream, encoding);
        }

        public List<SubtitleItem> ParseText(string text) {
            return ParseStream(GenerateStreamFromString(text), new UTF8Encoding());
        }

        public static Stream GenerateStreamFromString(string s) {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private StackPanel GetPanel(string line) {
            StackPanel panel = new StackPanel();
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.Orientation = Orientation.Horizontal;
            panel.Margin = new Thickness(0, 0, 0, 10);
            var block = GetTextBlock(line);
            Regex rgx = new Regex("<.*>.*</.*>", RegexOptions.IgnoreCase);
            var matches = rgx.Matches(line);
            if (matches.Count > 0) {
                EditText(line).ForEach(x => panel.Children.Add(x));
            } else {
                panel.Children.Add(block);
            }
            return panel;
        }

        private List<TextBlock> EditText(string text) {
            List<TextBlock> blocks = new List<TextBlock>();
            List<string> toApply = new List<string>();
            string temp = text;
            var block = GetTextBlock();
            List<string> tags = new List<string>() { "<i>", "<b>", "<font color=\"#[0-9a-zA-Z]{3,8}\">", "</i>", "</b>", "</font>" };
            while (!String.IsNullOrEmpty(text)) {
                if (text[0] == '<') {
                    for (int x = 0; x < tags.Count(); x++) {
                        var match = Regex.Match(text, tags[x]);
                        if (match.Success && match.Index == 0) {
                            blocks.Add(block);
                            block = GetTextBlock();
                            text = text.Remove(0, match.Length);
                            text = '\u00AD' + text;
                            switch (x) {
                                case 0:
                                    toApply.Add("i");
                                    break;
                                case 1:
                                    toApply.Add("b");
                                    break;
                                case 2:
                                    var color = match.Value.Remove(0, 13);
                                    toApply.Add(color.Remove(match.Length - 15, 2));
                                    break;
                                case 3:
                                    toApply.Remove("i");
                                    break;
                                case 4:
                                    toApply.Remove("b");
                                    break;
                                case 5:
                                    for (int i = toApply.Count() - 1; i >= 0; i--) {
                                        if (toApply[i].StartsWith("#")) toApply.Remove(toApply[i]);
                                    }
                                    break;
                            }
                        }
                    }
                }
                if (toApply.Contains("i")) {
                    block.FontStyle = FontStyles.Italic;
                }
                if (toApply.Contains("b")) {
                    block.FontWeight = FontWeights.Bold;
                }
                Match m = null;
                if (toApply.Any(x => (m = Regex.Match(x, "#[0-9a-zA-Z]{3,8}")).Success)) {
                    block.Foreground = GetColorFromHex(m.Value);
                }
                if (text.Count() > 0) {
                    block.Text += text[0];
                    text = text.Remove(0, 1);
                }
            }
            blocks.Add(block);
            for (int i = blocks.Count - 1; i >= 0; i--) {
                if (String.IsNullOrEmpty(blocks[i].Text)) {
                    blocks.Remove(blocks[i]);
                }
            }
            return blocks;
        }



        private TextBlock GetTextBlock(string text) {
            var block = GetTextBlock();
            block.Text = text;
            return block;
        }

        private TextBlock GetTextBlock() {
            DropShadowEffect effect = new DropShadowEffect();
            effect.Color = Colors.Black;
            effect.BlurRadius = 1;
            effect.ShadowDepth = 3;
            TextBlock block = new TextBlock();
            block.Effect = effect;
            block.HorizontalAlignment = HorizontalAlignment.Center;
            block.Foreground = GetColorFromHex("#F5F5F5");
            return block;
        }

        private Brush GetColorFromHex(string hex) {
            try {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            } catch (Exception) {
                return Brushes.Black;
            }
        }

        /// <summary>
        /// Splits srt file to lines
        /// </summary>
        private IEnumerable<string> GetSrtSubTitleParts(TextReader reader) {
            string line;
            var sb = new StringBuilder();
            while ((line = reader.ReadLine()) != null) {
                if (string.IsNullOrEmpty(line.Trim())) {
                    var res = sb.ToString().TrimEnd();
                    if (!string.IsNullOrEmpty(res)) {
                        yield return res;
                    }
                    sb = new StringBuilder();
                } else {
                    sb.AppendLine(line);
                }
            }
            if (sb.Length > 0) {
                yield return sb.ToString();
            }
        }

        /// <summary>
        /// Tries to parse line to start and end in double
        /// </summary>
        private bool TryParseTimecodeLine(string line, out double startTc, out double endTc) {
            var parts = line.Split(_delimiters, StringSplitOptions.None);
            if (parts.Length != 2) {
                // this is not a timecode line
                startTc = endTc = -1;
                return false;
            } else {
                startTc = ParseSrtTimecode(parts[0]);
                endTc = ParseSrtTimecode(parts[1]);
                return true;
            }
        }

        /// <summary>
        /// Parse string to double
        /// </summary>
        private static double ParseSrtTimecode(string s) {
            var match = Regex.Match(s, "[0-9]+:[0-9]+:[0-9]+([,\\.][0-9]+)?");
            if (match.Success) {
                s = match.Value;
                TimeSpan result;
                if (TimeSpan.TryParse(s.Replace(',', '.'), out result)) {
                    var nbOfMs = result.TotalMilliseconds;
                    return nbOfMs;
                }
            }
            return -1;
        }
    }
}
