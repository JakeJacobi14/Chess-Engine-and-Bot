using System.Collections.Generic;

public class OpeningMove
{
	public Move Move { get; set; }

	public List<OpeningMove> Responses { get; set; } = new List<OpeningMove>();
}
