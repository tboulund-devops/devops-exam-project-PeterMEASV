using System;
using System.Collections.Generic;

namespace efscaffold;

public partial class UsersMovie
{
    public string UserId { get; set; } = null!;

    public string MovieId { get; set; } = null!;

    public int? Rating { get; set; }

    public bool? Seen { get; set; }

    public string? Comment { get; set; }
}
