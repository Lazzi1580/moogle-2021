namespace MoogleEngine;

public class SearchItem : IComparable
{
    public SearchItem(string title, string snippet, double score)
    {
        this.Title = title;
        this.Snippet = snippet;
        this.Score = score;
    }

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        SearchItem aux = obj as SearchItem;

        return (this.Score >= aux.Score) ? -1 : 1;
    }

    public string Title { get; private set; }

    public string Snippet { get; private set; }

    public double Score { get; private set; }

}
