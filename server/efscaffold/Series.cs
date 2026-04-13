using System;
using System.Collections.Generic;

namespace efscaffold;

public partial class Series
{
    public string Id { get; set; } = null!;

    public string Title { get; set; } = null!;

    public int Year { get; set; }

    public string? Description { get; set; }

    public string? Starring { get; set; }

    public int? Episodes { get; set; }

    public int? Seasons { get; set; }
}
