using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeamBoard.Services;
using TeamBoard.Infrastructure;
using Xunit;

namespace TeamBoard.Tests.Services
{
	public class QueryTests
	{
		[Fact]
		public void CanCreateValidQuery()
		{
			Query query = new Query("teamboard", new Dictionary<string, string>()
						{
							{"Id","System.Id"},
							{"Summary", "System.Title"},
							{"Description", "System.Description"},
							{"Priority", "CodeStudio.Custom"}
						});

			query.AddColumn("Id");
			query.AddColumn("Summary");
			query.AddColumn("Description");
			query.AddColumn("Priority");

			Assert.Equal("SELECT [System.Id],[System.Title],[System.Description],[CodeStudio.Custom] FROM WorkItems WHERE [System.TeamProject] = 'teamboard' AND [System.State] <> 'Closed'",
				query.ToString());
		}
	}
}
