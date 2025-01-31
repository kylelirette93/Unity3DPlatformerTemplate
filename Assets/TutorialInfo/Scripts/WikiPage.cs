using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WikiPage", menuName = "Wiki/Page")]
public class WikiPage : ScriptableObject
{
    public string title;
    public string markdownContent;
    [Tooltip("Optional icon to display at the top of the page")]
    public Texture2D icon;
    [Tooltip("References to other wiki pages that can be linked to")]
    public WikiLink[] links;

    public Section[] sections;
    public bool loadedLayout;

    [Serializable]
    public class Section
    {
        public string heading, text, linkText, url;
    }
}

[System.Serializable]
public class WikiLink
{
    public string linkText;
    public WikiPage targetPage;
    [Tooltip("Optional URL for external links")]
    public string externalUrl;
}