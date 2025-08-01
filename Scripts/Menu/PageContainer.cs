using Godot;
using System;
using System.Collections.Generic;

public partial class PageContainer : Control
{
    [Export] private Label pageTitleLabel;
    [Export] private Label pageNumberLabel;
    [Export] private bool showNumberOnTitle;
    private List<Page> pages = new List<Page>();
    private int currentPage = 0;

    // Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Godot.Collections.Array<Node> nodes;

        if (GetChildCount() == 1 && !(GetChild(0) is Page))
            nodes = GetChild(0).GetChildren();
        else
            nodes = GetChildren();
            

		for (int i = 0; i < nodes.Count; i++)
		{
            Page page = nodes[i] as Page;

            pages.Add(page);
            page.Visible = i == 0;
		}

        UpdatePageLabels();
	}
    

    private void UpdatePageLabels()
    {
        if (pageTitleLabel != null)
            pageTitleLabel.Text = (showNumberOnTitle ? (currentPage + 1) + ". " : "") + pages[currentPage].Title;

        if (pageNumberLabel != null)
            pageNumberLabel.Text = (currentPage + 1) + "/" + pages.Count;
    }

    public void SetPage(int page)
    {
        pages[currentPage].Visible = false;
        pages[page].Visible = true;

        currentPage = page;
        UpdatePageLabels();
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
            SetPage(currentPage - 1);
    }

    public void NextPage()
    {
        if (currentPage < pages.Count - 1)
            SetPage(currentPage + 1);
    }
}
