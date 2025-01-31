using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class MarkdownRenderer
{
    private GUIStyle bodyStyle;
    private GUIStyle headingStyle1;
    private GUIStyle headingStyle2;
    private GUIStyle headingStyle3;
    private GUIStyle linkStyle;
    private GUIStyle codeStyle;
    private GUIStyle listStyle;

    public MarkdownRenderer()
    {
        InitializeStyles();
    }

    private void InitializeStyles()
    {
        bodyStyle = new GUIStyle(EditorStyles.label);
        bodyStyle.wordWrap = true;
        bodyStyle.richText = true;
        bodyStyle.fontSize = 14;

        headingStyle1 = new GUIStyle(bodyStyle);
        headingStyle1.fontSize = 26;
        headingStyle1.fontStyle = FontStyle.Bold;

        headingStyle2 = new GUIStyle(bodyStyle);
        headingStyle2.fontSize = 20;
        headingStyle2.fontStyle = FontStyle.Bold;

        headingStyle3 = new GUIStyle(bodyStyle);
        headingStyle3.fontSize = 16;
        headingStyle3.fontStyle = FontStyle.Bold;

        linkStyle = new GUIStyle(bodyStyle);
        linkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
        linkStyle.hover.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 0.8f);

        codeStyle = new GUIStyle(EditorStyles.textArea);
        codeStyle.wordWrap = true;
        codeStyle.fontSize = 12;
        codeStyle.fontStyle = FontStyle.Bold;

        listStyle = new GUIStyle(bodyStyle);
        listStyle.padding.left = 20;
    }

    public void RenderMarkdown(string markdown)
    {
        if (string.IsNullOrEmpty(markdown)) return;

        var blocks = SplitIntoBlocks(markdown);
        foreach (var block in blocks)
        {
            RenderBlock(block);
        }
    }

    private string[] SplitIntoBlocks(string markdown)
    {
        return markdown.Split(new[] { "\n\n", "\r\n\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
    }

    private void RenderBlock(string block)
    {
        block = block.Trim();

        if (block.StartsWith("# "))
        {
            RenderHeading(block.Substring(2), headingStyle1);
        }
        else if (block.StartsWith("## "))
        {
            RenderHeading(block.Substring(3), headingStyle2);
        }
        else if (block.StartsWith("### "))
        {
            RenderHeading(block.Substring(4), headingStyle3);
        }
        else if (block.StartsWith("```"))
        {
            RenderCodeBlock(block);
        }
        else if (block.StartsWith("- ") || block.StartsWith("* "))
        {
            RenderList(block);
        }
        else
        {
            RenderParagraph(block);
        }

        GUILayout.Space(8); // Add space between blocks
    }

    private void RenderHeading(string text, GUIStyle style)
    {
        text = ProcessInlineFormatting(text);
        EditorGUILayout.LabelField(text, style);
    }

    private void RenderParagraph(string text)
    {
        text = ProcessInlineFormatting(text);
        // Split the text into segments (link and non-link parts)
        var segments = SplitTextIntoSegments(text);

        EditorGUILayout.BeginVertical();
        var lineContent = new StringBuilder();

        foreach (var segment in segments)
        {
            if (segment.isLink)
            {
                // Flush any accumulated regular text
                if (lineContent.Length > 0)
                {
                    EditorGUILayout.LabelField(lineContent.ToString(), bodyStyle);
                    lineContent.Clear();
                }

                // Render the link
                if (LinkLabel(new GUIContent(segment.displayText)))
                {
                    HandleLinkClick(segment.linkType, segment.target);
                }
            }
            else
            {
                lineContent.Append(segment.text);
            }
        }

        // Flush any remaining text
        if (lineContent.Length > 0)
        {
            EditorGUILayout.LabelField(lineContent.ToString(), bodyStyle);
        }

        EditorGUILayout.EndVertical();
    }

    private class TextSegment
    {
        public string text;
        public bool isLink;
        public string linkType;
        public string target;
        public string displayText;
    }

    private List<TextSegment> SplitTextIntoSegments(string text)
    {
        var segments = new List<TextSegment>();
        var linkPattern = @"<link=""(.+?):(.+?)"">(.*?)</link>";
        var matches = Regex.Matches(text, linkPattern);
        int lastIndex = 0;

        foreach (Match match in matches)
        {
            // Add text before the link
            if (match.Index > lastIndex)
            {
                segments.Add(new TextSegment
                {
                    text = text.Substring(lastIndex, match.Index - lastIndex),
                    isLink = false
                });
            }

            // Add the link
            segments.Add(new TextSegment
            {
                isLink = true,
                linkType = match.Groups[1].Value,
                target = match.Groups[2].Value,
                displayText = match.Groups[3].Value
            });

            lastIndex = match.Index + match.Length;
        }

        // Add remaining text after last link
        if (lastIndex < text.Length)
        {
            segments.Add(new TextSegment
            {
                text = text.Substring(lastIndex),
                isLink = false
            });
        }

        return segments;
    }

    private void HandleLinkClick(string linkType, string target)
    {
        switch (linkType)
        {
            case "asset":
                var wikiPage = AssetDatabase.LoadAssetAtPath<WikiPage>(target);
                if (wikiPage != null)
                {
                    Selection.activeObject = wikiPage;
                }
                break;

            case "file":
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(target);
                if (asset != null)
                {
                    AssetDatabase.OpenAsset(asset);
                }
                break;

            case "url":
                Application.OpenURL(target);
                break;
        }
    }

    private bool LinkLabel(GUIContent label)
    {
        var position = GUILayoutUtility.GetRect(label, linkStyle);

        Handles.BeginGUI();
        Handles.color = linkStyle.normal.textColor;
        Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMin + label.text.Length * 7f, position.yMax));
        Handles.color = Color.white;
        Handles.EndGUI();

        EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

        return GUI.Button(position, label, linkStyle);
    }

    private void RenderCodeBlock(string block)
    {
        var lines = block.Split('\n');
        var code = string.Join("\n",
            lines.Skip(1).Take(lines.Length - 2)); // Remove ``` lines

        EditorGUILayout.TextArea(code, codeStyle);
    }

    private void RenderList(string block)
    {
        var items = block.Split('\n')
            .Where(line => line.StartsWith("- ") || line.StartsWith("* "))
            .Select(line => line.Substring(2));

        foreach (var item in items)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("•", GUILayout.Width(15));

            var segments = SplitTextIntoSegments(ProcessInlineFormatting(item));
            var lineContent = new StringBuilder();

            foreach (var segment in segments)
            {
                if (segment.isLink)
                {
                    // Flush any accumulated regular text
                    if (lineContent.Length > 0)
                    {
                        EditorGUILayout.LabelField(lineContent.ToString(), bodyStyle);
                        lineContent.Clear();
                    }

                    // Render the link
                    if (LinkLabel(new GUIContent(segment.displayText)))
                    {
                        HandleLinkClick(segment.linkType, segment.target);
                    }
                }
                else
                {
                    lineContent.Append(segment.text);
                }
            }

            // Flush any remaining text
            if (lineContent.Length > 0)
            {
                EditorGUILayout.LabelField(lineContent.ToString(), bodyStyle);
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private string ProcessInlineFormatting(string text)
    {
        // Bold
        text = Regex.Replace(text, @"\*\*(.+?)\*\*", "<b>$1</b>");
        text = Regex.Replace(text, @"__(.+?)__", "<b>$1</b>");

        // Italic
        text = Regex.Replace(text, @"\*(.+?)\*", "<i>$1</i>");
        text = Regex.Replace(text, @"_(.+?)_", "<i>$1</i>");

        // Inline code
        text = Regex.Replace(text, @"`(.+?)`", "<color=#bdc4cb><b>$1</b></color>");

        // Unity asset links with special syntax: [[WikiPage:path/to/asset]] or [[File:path/to/file]]
        text = Regex.Replace(text, @"\[\[WikiPage:(.+?)\]\]", match => {
            var path = "Assets/" + match.Groups[1].Value;
            if (!path.Contains("."))
                path = path + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<WikiPage>(path.TrimStart().TrimEnd());
            if (asset != null)
            {
                return $"<link=\"asset:{path}\">{asset.title}</link>";
            }
            return $"<color=red>Missing: {path}</color>";
        });

        text = Regex.Replace(text, @"\[\[File:(.+?)\]\]", match => {
            var path = "Assets/" + match.Groups[1].Value;
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (asset != null)
            {
                return $"<link=\"file:{path}\">{Path.GetFileName(path)}</link>";
            }
            return $"<color=red>Missing: {path}</color>";
        });

        // Regular markdown links
        text = Regex.Replace(text, @"\[(.+?)\]\((.+?)\)", "<link=\"url:$2\">$1</link>");

        return text;
    }

    public bool HandleLinks(string text, Vector2 position)
    {
        var linkMatches = Regex.Matches(text, @"\[(.+?)\]\((.+?)\)");
        foreach (Match match in linkMatches)
        {
            var linkText = match.Groups[1].Value;
            var url = match.Groups[2].Value;

            // Calculate link position and check if clicked
            var linkRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown &&
                linkRect.Contains(Event.current.mousePosition))
            {
                if (url.StartsWith("http"))
                {
                    Application.OpenURL(url);
                    return true;
                }
                // Handle internal page links here
            }
        }
        return false;
    }
}